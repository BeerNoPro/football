using System.Text;
using FootballBlog.Core.DTOs;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Interfaces.Services;
using FootballBlog.Core.Services;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FootballBlog.API.Jobs;

public class PublishPredictionJob(
    IUnitOfWork uow,
    IPostService postService,
    IConfiguration configuration,
    ILogger<PublishPredictionJob> logger)
{
    public async Task ExecuteAsync(int predictionId)
    {
        logger.LogInformation("PublishPredictionJob started for prediction {PredictionId}", predictionId);

        var pred = await uow.MatchPredictions.GetByIdAsync(predictionId);
        if (pred is null)
        {
            logger.LogWarning("Prediction {PredictionId} not found", predictionId);
            return;
        }

        if (pred.IsPublished)
        {
            logger.LogDebug("Prediction {PredictionId} already published", predictionId);
            return;
        }

        var match = await uow.Matches.GetWithPredictionAsync(pred.MatchId);
        if (match is null)
        {
            logger.LogWarning("Match {MatchId} not found for prediction {PredictionId}", pred.MatchId, predictionId);
            return;
        }

        int categoryId = configuration.GetValue<int>("Prediction:BlogCategoryId", 1);
        int authorId = configuration.GetValue<int>("Prediction:SystemAuthorId", 1);

        var homeTeam = match.HomeTeam.Name;
        var awayTeam = match.AwayTeam.Name;
        var kickoff = match.KickoffUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

        var title = $"Nhận định {homeTeam} vs {awayTeam} — {kickoff}";
        var slug = $"nhan-dinh-{SlugService.Generate(homeTeam)}-vs-{SlugService.Generate(awayTeam)}-{match.KickoffUtc:yyyyMMdd}";

        var content = BuildPostContent(match.HomeTeam.Name, match.AwayTeam.Name, match.League.Name, kickoff, pred);

        var dto = new CreatePostDto(
            Title: title,
            Slug: slug,
            Content: content,
            Thumbnail: null,
            CategoryId: categoryId,
            AuthorId: authorId,
            PublishNow: true
        );

        var post = await postService.CreateAsync(dto);

        pred.BlogPostId = post.Id;
        pred.IsPublished = true;
        await uow.MatchPredictions.UpdateAsync(pred);
        await uow.CommitAsync();

        BackgroundJob.Enqueue<TelegramNotificationJob>(j => j.SendPredictionAsync(predictionId));

        logger.LogInformation(
            "Prediction {PredictionId} published as post {PostId} (slug: {Slug})",
            predictionId, post.Id, slug);
    }

    private static string BuildPostContent(
        string homeTeam, string awayTeam, string league, string kickoff, Core.Models.MatchPrediction pred)
    {
        var sb = new StringBuilder();

        var scoreStr = pred.PredictedHomeScore.HasValue && pred.PredictedAwayScore.HasValue
            ? $"**Tỷ số dự đoán: {pred.PredictedHomeScore} - {pred.PredictedAwayScore}**"
            : string.Empty;

        var outcomeVi = pred.PredictedOutcome switch
        {
            "HomeWin" => $"{homeTeam} thắng",
            "AwayWin" => $"{awayTeam} thắng",
            _ => "Hòa"
        };

        sb.AppendLine($"## {homeTeam} vs {awayTeam}");
        sb.AppendLine($"**Giải:** {league} | **Thời gian:** {kickoff}");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"### Dự đoán AI ({pred.AIProvider} — {pred.AIModel})");
        sb.AppendLine();
        sb.AppendLine($"- **Kết quả:** {outcomeVi}");
        if (!string.IsNullOrEmpty(scoreStr))
        {
            sb.AppendLine($"- {scoreStr}");
        }

        sb.AppendLine($"- **Độ tự tin:** {pred.ConfidenceScore}%");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("### Phân tích");
        sb.AppendLine();
        sb.AppendLine(pred.AnalysisSummary);

        return sb.ToString();
    }

}
