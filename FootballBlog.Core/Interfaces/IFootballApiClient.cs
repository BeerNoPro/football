using FootballBlog.Core.DTOs;
using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces;

public interface IFootballApiClient
{
    /// <summary>Lấy fixtures sắp tới của một league. Trả null nếu rate limit hoặc lỗi HTTP.</summary>
    Task<IEnumerable<FixtureRawDto>?> GetUpcomingFixturesAsync(int leagueId, int next = 20);

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
}
