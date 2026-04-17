using FootballBlog.Core.Models;

namespace FootballBlog.Core.Interfaces.Services;

public interface ITelegramService
{
    /// <summary>Gửi nhận định prediction lên Telegram channel, trả về message ID để edit sau.</summary>
    Task<long?> SendPredictionAsync(MatchPrediction prediction, Match match, CancellationToken ct = default);

    /// <summary>Edit message khi có kết quả thực tế sau trận.</summary>
    Task EditResultAsync(long messageId, Match match, MatchPrediction prediction, CancellationToken ct = default);
}
