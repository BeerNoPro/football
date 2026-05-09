using System.Diagnostics;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FootballBlog.API.Jobs;

/// <summary>
/// Fetch squad (cầu thủ) cho các đội có trận trong 7 ngày tới — tối đa ~20 đội/run.
/// Trigger thủ công từ Admin UI. Không schedule tự động.
/// </summary>
public class FetchSquadJob(
    IFootballApiClient apiClient,
    IUnitOfWork uow,
    IOptions<FootballApiOptions> options,
    ILogger<FetchSquadJob> logger)
{
    public async Task ExecuteAsync()
    {
        var sw = Stopwatch.StartNew();
        var premiumLeagues = new HashSet<int>(options.Value.PremiumLeagueIds);

        // Chỉ fetch squad cho đội trong premium leagues — tiết kiệm API quota
        var upcomingMatches = await uow.Matches.GetUpcomingAsync(hoursAhead: 7 * 24);
        var teams = upcomingMatches
            .Where(m => premiumLeagues.Contains(m.League.ExternalId))
            .SelectMany(m => new[] { (m.HomeTeamId, m.HomeTeam), (m.AwayTeamId, m.AwayTeam) })
            .DistinctBy(t => t.Item1)
            .Select(t => t.Item2)
            .Where(t => t is not null)
            .ToList();

        logger.LogInformation("FetchSquadJob started. Teams with upcoming matches: {Count}", teams.Count);

        int fetched = 0;
        int skipped = 0;

        foreach (var team in teams)
        {
            bool hasSquad = await uow.SquadMembers.HasSquadAsync(team.Id);
            if (hasSquad)
            {
                logger.LogDebug("Squad already exists for team {TeamName} ({TeamId}) — skipping", team.Name, team.Id);
                skipped++;
                continue;
            }

            var playerList = (await apiClient.GetSquadByTeamAsync(team.ExternalId))?.ToList();
            if (playerList is null)
            {
                logger.LogWarning("GetSquadByTeamAsync returned null for team {TeamName} — quota hit or error, aborting run", team.Name);
                break;
            }

            foreach (var p in playerList)
            {
                // Upsert Player
                var existing = await uow.Players.GetByExternalIdAsync(p.ExternalId);
                int playerId;

                if (existing is null)
                {
                    var player = new Player
                    {
                        ExternalId = p.ExternalId,
                        Name = p.Name,
                        Age = p.Age,
                        Position = p.Position,
                        Photo = p.Photo
                    };
                    await uow.Players.AddAsync(player);
                    await uow.CommitAsync();
                    playerId = player.Id;
                }
                else
                {
                    // Cập nhật tuổi/ảnh nếu thay đổi (position ổn định hơn, giữ nguyên)
                    existing.Age = p.Age;
                    if (!string.IsNullOrEmpty(p.Photo))
                    {
                        existing.Photo = p.Photo;
                    }

                    await uow.Players.UpdateAsync(existing);
                    playerId = existing.Id;
                }

                // Upsert SquadMember
                var member = await uow.SquadMembers.GetByTeamAndPlayerAsync(team.Id, playerId);
                if (member is null)
                {
                    await uow.SquadMembers.AddAsync(new SquadMember
                    {
                        TeamId = team.Id,
                        PlayerId = playerId,
                        Number = p.Number,
                        Position = p.Position
                    });
                }
                else
                {
                    member.Number = p.Number;
                    member.Position = p.Position;
                    await uow.SquadMembers.UpdateAsync(member);
                }
            }

            await uow.CommitAsync();
            fetched++;
            logger.LogInformation("Fetched squad for {TeamName}: {PlayerCount} players", team.Name, playerList.Count);
        }

        sw.Stop();
        logger.LogInformation(
            "FetchSquadJob finished. Fetched={Fetched}, Skipped={Skipped}, Duration={DurationMs}ms",
            fetched, skipped, sw.ElapsedMilliseconds);
    }
}
