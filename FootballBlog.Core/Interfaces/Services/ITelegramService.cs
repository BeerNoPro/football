using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces.Services;

public interface ITelegramService
{
    /// <summary>Gửi nhận định prediction lên Telegram channel, trả về message ID để edit sau.</summary>
    Task<long?> SendPredictionAsync(MatchPrediction prediction, Match match, CancellationToken ct = default);

    /// <summary>Edit message gốc (PreMatch) — append section phân tích H2 vào cuối message.</summary>
    Task EditHalfTimeAsync(long messageId, Match match, MatchPrediction preMatchPrediction, MatchPrediction htPrediction, CancellationToken ct = default);
}
