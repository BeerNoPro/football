using System.Diagnostics;
using FootballBlog.API.Hubs;
using FootballBlog.Core.DTOs;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FootballBlog.API.Jobs;

public class LiveScorePollingJob(
    IFootballApiClient apiClient,
    IUnitOfWork uow,
    IHubContext<LiveScoreHub, ILiveScoreClient> hubContext,
    ILogger<LiveScorePollingJob> logger)
{
    public async Task ExecuteAsync()
    {
        var sw = Stopwatch.StartNew();
        // Adaptive gate — kiểm tra DB trước (0 API cost), thoát sớm nếu không có live match
        IEnumerable<LiveMatch> liveInDb = await uow.LiveMatches.GetLiveMatchesAsync();
        List<LiveMatch> liveInDbList = liveInDb.ToList();

        if (liveInDbList.Count == 0)
        {
            logger.LogDebug("No live matches in DB. Skipping polling cycle.");
            return;
        }

        logger.LogInformation("LiveScorePollingJob started. Live matches in DB: {Count}", liveInDbList.Count);

        // 1 request duy nhất lấy TẤT CẢ live matches
        IEnumerable<LiveMatch>? liveFromApi = await apiClient.GetAllLiveFixturesAsync();
        if (liveFromApi is null)
        {
            logger.LogWarning("Live fixture fetch returned null (rate limit or HTTP error). Skipping update.");
            return;
        }

        List<LiveMatch> liveFromApiList = liveFromApi.ToList();
        HashSet<int> apiExternalIds = liveFromApiList.Select(m => m.ExternalId).ToHashSet();

        int inserted = 0;
        int updated = 0;

        // Upsert live matches từ API
        foreach (LiveMatch fixture in liveFromApiList)
        {
            LiveMatch? existing = await uow.LiveMatches.GetByExternalIdAsync(fixture.ExternalId);

            if (existing is null)
            {
                // Trận mới vào live — tìm parent Match để set FK
                Match? parentMatch = await uow.Matches.GetByExternalIdAsync(fixture.ExternalId);
                fixture.MatchId = parentMatch?.Id;
                await uow.LiveMatches.AddAsync(fixture);
                inserted++;
            }
            else
            {
                existing.HomeScore = fixture.HomeScore;
                existing.AwayScore = fixture.AwayScore;
                existing.Status = fixture.Status;
                existing.Minute = fixture.Minute;
                await uow.LiveMatches.UpdateAsync(existing);
                updated++;
            }
        }

        // Đánh dấu Finished — trận không còn trong API response nhưng vẫn Live trong DB
        foreach (LiveMatch dbLive in liveInDbList.Where(m => !apiExternalIds.Contains(m.ExternalId)))
        {
            dbLive.Status = MatchStatus.Finished;
            await uow.LiveMatches.UpdateAsync(dbLive);

            // Cập nhật parent Match
            if (dbLive.ExternalId > 0)
            {
                Match? parentMatch = await uow.Matches.GetByExternalIdAsync(dbLive.ExternalId);
                if (parentMatch is not null)
                {
                    parentMatch.Status = MatchStatus.Finished;
                    await uow.Matches.UpdateAsync(parentMatch);

                    // Trigger cập nhật kết quả lên Telegram
                    BackgroundJob.Enqueue<TelegramNotificationJob>(j => j.SendResultAsync(parentMatch.Id));
                }
            }
        }

        await uow.CommitAsync();

        // Broadcast mỗi live match đã update tới subscribers
        foreach (LiveMatch fixture in liveFromApiList.Where(m => m.MatchId.HasValue))
        {
            LiveMatchDto dto = new(
                fixture.Id,
                fixture.ExternalId,
                fixture.HomeTeam,
                fixture.AwayTeam,
                fixture.HomeScore,
                fixture.AwayScore,
                fixture.Status.ToString(),
                fixture.Minute,
                fixture.StartedAt,
                fixture.Events.Select(e => new MatchEventDto(e.Id, e.Minute, e.Type.ToString(), e.Description)).ToList());

            await hubContext.Clients
                .Group($"match-{fixture.MatchId}")
                .MatchUpdated(dto);
        }

        sw.Stop();
        logger.LogInformation(
            "LiveScorePollingJob finished. Inserted={Inserted}, Updated={Updated}, Duration={DurationMs}ms, Broadcasts={BroadcastCount}",
            inserted, updated, sw.ElapsedMilliseconds, liveFromApiList.Count);
    }
}
