using FootballBlog.Core.DTOs;
using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface IFootballApiClient
{
    /// <summary>Lấy TẤT CẢ live fixtures trong 1 request duy nhất — GET /fixtures?live=all.</summary>
    Task<IEnumerable<LiveMatch>?> GetAllLiveFixturesAsync();

    /// <summary>Head-to-head history. Dùng cho PreMatchDataJob 5h trước kickoff.</summary>
    Task<IEnumerable<FixtureRawDto>?> GetHeadToHeadAsync(int homeTeamExternalId, int awayTeamExternalId, int last = 10);

    /// <summary>Dữ liệu lineup dạng raw JSON. Dùng cho PreMatchDataJob 15min trước kickoff (Phase 5 xử lý).</summary>
    Task<string?> GetLineupsRawAsync(int fixtureId);

    /// <summary>Lấy danh sách đội kèm venue cho một giải. GET /teams?league=X&amp;season=Y.</summary>
    Task<IEnumerable<TeamRawDto>?> GetTeamsByLeagueAsync(int leagueId, int season);

    /// <summary>Lấy bảng xếp hạng. GET /standings?league=X&amp;season=Y.</summary>
    Task<IEnumerable<StandingRawDto>?> GetStandingsAsync(int leagueId, int season);

    /// <summary>Lấy fixtures trong khoảng ngày. GET /fixtures?league=X&amp;season=Y&amp;from=...&amp;to=...</summary>
    Task<IEnumerable<FixtureRawDto>?> GetFixturesByRangeAsync(int leagueId, int season, DateOnly from, DateOnly to);

    /// <summary>Lấy TẤT CẢ fixtures theo ngày — GET /fixtures?date=yyyy-MM-dd. 1 request/ngày, không giới hạn league.</summary>
    Task<IEnumerable<FixtureRawDto>?> GetFixturesByDateAsync(DateOnly date);

    /// <summary>Lấy dữ liệu HT: statistics + events H1. 2 API req. Dùng cho HalfTimePredictionJob.</summary>
    Task<HalfTimeContext?> GetFixtureHalfTimeDataAsync(int fixtureExternalId, int htHomeScore, int htAwayScore, int homeTeamExternalId, int awayTeamExternalId);

    /// <summary>Lấy squad (danh sách cầu thủ) của 1 đội. GET /players/squads?team={teamExternalId}.</summary>
    Task<IEnumerable<SquadPlayerDto>?> GetSquadByTeamAsync(int teamExternalId);

    /// <summary>Fetch toàn bộ statistics + events sau FT. 2 API req. Trả raw JSON để lưu vào Match.StatsJson / EventsJson.</summary>
    Task<(string? StatsJson, string? EventsJson)> GetFixturePostMatchDataAsync(int fixtureExternalId);
}
