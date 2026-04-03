using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FootballBlog.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchAndPrediction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExternalId = table.Column<int>(type: "integer", nullable: false),
                    HomeTeam = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AwayTeam = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HomeTeamExternalId = table.Column<int>(type: "integer", nullable: false),
                    AwayTeamExternalId = table.Column<int>(type: "integer", nullable: false),
                    LeagueId = table.Column<int>(type: "integer", nullable: false),
                    LeagueName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Season = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Round = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    KickoffUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Scheduled"),
                    HomeScore = table.Column<int>(type: "integer", nullable: true),
                    AwayScore = table.Column<int>(type: "integer", nullable: true),
                    VenueName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RefereeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FetchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatchPredictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MatchId = table.Column<int>(type: "integer", nullable: false),
                    AIProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AIModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PredictedHomeScore = table.Column<int>(type: "integer", nullable: true),
                    PredictedAwayScore = table.Column<int>(type: "integer", nullable: true),
                    PredictedOutcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    AnalysisSummary = table.Column<string>(type: "text", nullable: false),
                    PromptTokens = table.Column<int>(type: "integer", nullable: true),
                    CompletionTokens = table.Column<int>(type: "integer", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TelegramMessageId = table.Column<long>(type: "bigint", nullable: true),
                    BlogPostId = table.Column<int>(type: "integer", nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchPredictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchPredictions_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchPredictions_Posts_BlogPostId",
                        column: x => x.BlogPostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_ExternalId",
                table: "Matches",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchPredictions_BlogPostId",
                table: "MatchPredictions",
                column: "BlogPostId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchPredictions_MatchId",
                table: "MatchPredictions",
                column: "MatchId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchPredictions");

            migrationBuilder.DropTable(
                name: "Matches");
        }
    }
}
