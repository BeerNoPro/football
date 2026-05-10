using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballBlog.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchStatusKickoffIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventsJson",
                table: "Matches",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatsJson",
                table: "Matches",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Match_Status_KickoffUtc",
                table: "Matches",
                columns: new[] { "Status", "KickoffUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Match_Status_KickoffUtc",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "EventsJson",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "StatsJson",
                table: "Matches");
        }
    }
}
