using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballBlog.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHalfTimePrediction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MatchPredictions_MatchId",
                table: "MatchPredictions");

            migrationBuilder.AddColumn<int>(
                name: "Phase",
                table: "MatchPredictions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RawResponse",
                table: "MatchPredictions",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchPredictions_MatchId_Phase",
                table: "MatchPredictions",
                columns: new[] { "MatchId", "Phase" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MatchPredictions_MatchId_Phase",
                table: "MatchPredictions");

            migrationBuilder.DropColumn(
                name: "Phase",
                table: "MatchPredictions");

            migrationBuilder.DropColumn(
                name: "RawResponse",
                table: "MatchPredictions");

            migrationBuilder.CreateIndex(
                name: "IX_MatchPredictions_MatchId",
                table: "MatchPredictions",
                column: "MatchId",
                unique: true);
        }
    }
}
