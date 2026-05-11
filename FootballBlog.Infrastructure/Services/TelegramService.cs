using System.Text;
using FootballBlog.Core.Interfaces.Services;
using FootballBlog.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
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

    public async Task EditHalfTimeAsync(long messageId, Match match, MatchPrediction preMatchPrediction, MatchPrediction htPrediction, CancellationToken ct = default)
    {
        if (_channelId == 0)
        {
            return;
        }

        var text = BuildHalfTimeMessage(preMatchPrediction, htPrediction, match);

        try
        {
            await _bot.EditMessageText(
                chatId: _channelId,
                messageId: (int)messageId,
                text: text,
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: ct);

            _logger.LogInformation("Telegram message updated with HT analysis for match {MatchId}", match.Id);
        }
        catch (ApiRequestException ex) when (ex.Message.Contains("message is not modified"))
        {
            // Idempotent: job đã edit thành công ở lần trước, Hangfire retry là bình thường
            _logger.LogDebug("Telegram message {MessageId} already has HT analysis (not modified), skip", messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể edit Telegram message {MessageId} với HT analysis", messageId);
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

    private static string BuildHalfTimeMessage(MatchPrediction preMatch, MatchPrediction htP, Match m)
    {
        var sb = new StringBuilder();

        // Giữ nguyên toàn bộ PreMatch message
        sb.Append(BuildPredictionMessage(preMatch, m));

        // Separator + section HT
        sb.AppendLine();
        sb.AppendLine(EscapeMd("---"));
        sb.AppendLine($"⏱ *Phân tích hiệp 2*");

        var homeTeam = EscapeMd(m.HomeTeam.Name);
        var awayTeam = EscapeMd(m.AwayTeam.Name);
        var outcomeVi = htP.PredictedOutcome switch
        {
            "HomeWin" => $"🏠 {homeTeam} thắng",
            "AwayWin" => $"✈️ {awayTeam} thắng",
            _ => "🤝 Hòa"
        };

        sb.AppendLine($"Dự đoán: *{htP.PredictedHomeScore} \\- {htP.PredictedAwayScore}* \\({outcomeVi}\\)");
        sb.AppendLine($"Độ tự tin: *{EscapeMd(htP.ConfidenceScore.ToString())}%*");
        sb.AppendLine();

        var summary = htP.AnalysisSummary.Length > 500
            ? htP.AnalysisSummary[..497] + "..."
            : htP.AnalysisSummary;
        sb.AppendLine(EscapeMd(summary));

        return sb.ToString();
    }

    private static string EscapeMd(string text) =>
        System.Text.RegularExpressions.Regex.Replace(text, @"([_*\[\]()~`>#+\-=|{}.!\\])", @"\$1");
}
