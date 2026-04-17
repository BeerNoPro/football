using FootballBlog.Core.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace FootballBlog.API.Hubs;

public interface ILiveScoreClient
{
    Task MatchUpdated(LiveMatchDto dto);
}

public class LiveScoreHub : Hub<ILiveScoreClient>
{
    public async Task JoinMatch(string matchId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"match-{matchId}");

    public async Task LeaveMatch(string matchId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"match-{matchId}");
}
