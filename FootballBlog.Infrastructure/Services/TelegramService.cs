using System.Text;
using FootballBlog.Core.Interfaces.Services;
using FootballBlog.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace FootballBlog.Infrastructure.Services;

public class TelegramService : ITelegramService
{
    private readonly ITelegramBotClient _bot;
    private readonly long _channelId;
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(IConfiguration configuration, ILogger<TelegramService> logger)
    {
        _logger = logger;
        var token = configuration["Telegram:BotToken"]
            ?? throw new InvalidOperationException("Telegram:BotToken chưa được cấu hình");
        _channelId = configuration.GetValue<long>("Telegram:ChannelId");
        _bot = new TelegramBotClient(token);
    }

    public async Task<long?> SendPredictionAsync(MatchPrediction prediction, Match match, CancellationToken ct = default)
    {
        if (_channelId == 0)
        {
            _logger.LogWarning("Telegram:ChannelId chưa được cấu hình, bỏ qua gửi tin");
            return null;
        }

        var text = BuildPredictionMessage(prediction, match);

        try
        {
            var msg = await _bot.SendMessage(
                chatId: _channelId,
                text: text,
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: ct);

            _logger.LogInformation(
                "Telegram message sent for match {MatchId}, messageId={MessageId}",
                match.Id, msg.MessageId);

            return msg.MessageId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể gửi Telegram message cho match {MatchId}", match.Id);
            return null;
        }
    }

    public async Task EditResultAsync(long messageId, Match match, MatchPrediction prediction, CancellationToken ct = default)
    {
        if (_channelId == 0)
        {
            return;
        }

        var text = BuildResultMessage(prediction, match);

        try
        {
            await _bot.EditMessageText(
                chatId: _channelId,
                messageId: (int)messageId,
                text: text,
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: ct);

            _logger.LogInformation(
                "Telegram message updated with result for match {MatchId}", match.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể edit Telegram message {MessageId}", messageId);
        }
    }

    private static string BuildPredictionMessage(MatchPrediction p, Match m)
    {
        var sb = new StringBuilder();
        var homeTeam = EscapeMd(m.HomeTeam.Name);
        var awayTeam = EscapeMd(m.AwayTeam.Name);
        var league = EscapeMd(m.League.Name);
        var kickoff = EscapeMd(m.KickoffUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));

        var outcomeVi = p.PredictedOutcome switch
        {
            "HomeWin" => $"🏠 {homeTeam} thắng",
            "AwayWin" => $"✈️ {awayTeam} thắng",
            _ => "🤝 Hòa"
        };

        sb.AppendLine($"⚽ *{homeTeam} vs {awayTeam}*");
        sb.AppendLine($"🏆 {league} \\| 🕐 {kickoff}");
        sb.AppendLine();
        sb.AppendLine($"🤖 *Nhận định AI \\({EscapeMd(p.AIProvider)}\\)*");
        sb.AppendLine($"Kết quả: {outcomeVi}");

        if (p.PredictedHomeScore.HasValue && p.PredictedAwayScore.HasValue)
        {
            sb.AppendLine($"Tỷ số: *{p.PredictedHomeScore} \\- {p.PredictedAwayScore}*");
        }

        sb.AppendLine($"Độ tự tin: *{EscapeMd(p.ConfidenceScore.ToString())}%*");
        sb.AppendLine();

        // Giới hạn analysis để không quá dài, escape toàn bộ
        var summary = p.AnalysisSummary.Length > 500
            ? p.AnalysisSummary[..497] + "..."
            : p.AnalysisSummary;
        sb.AppendLine(EscapeMd(summary));

        return sb.ToString();
    }

    private static string BuildResultMessage(MatchPrediction p, Match m)
    {
        var sb = new StringBuilder();
        var homeTeam = EscapeMd(m.HomeTeam.Name);
        var awayTeam = EscapeMd(m.AwayTeam.Name);

        sb.AppendLine($"⚽ *{homeTeam} vs {awayTeam}* — KẾT THÚC");
        sb.AppendLine($"📊 Kết quả thực tế: *{m.HomeScore} \\- {m.AwayScore}*");
        sb.AppendLine();
        sb.AppendLine($"🤖 AI dự đoán: {p.PredictedHomeScore} \\- {p.PredictedAwayScore} \\({EscapeMd(p.PredictedOutcome)}\\)");

        var actualOutcome = (m.HomeScore, m.AwayScore) switch
        {
            var (h, a) when h > a => "HomeWin",
            var (h, a) when h < a => "AwayWin",
            _ => "Draw"
        };

        var correct = actualOutcome == p.PredictedOutcome ? "✅ Đúng\\!" : "❌ Sai";
        sb.AppendLine($"Dự đoán: {correct}");

        return sb.ToString();
    }

    private static string EscapeMd(string text) =>
        System.Text.RegularExpressions.Regex.Replace(text, @"([_*\[\]()~`>#+\-=|{}.!\\])", @"\$1");
}
