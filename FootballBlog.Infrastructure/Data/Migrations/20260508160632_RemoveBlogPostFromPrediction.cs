using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballBlog.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBlogPostFromPrediction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchPredictions_Posts_BlogPostId",
                table: "MatchPredictions");

            migrationBuilder.DropIndex(
                name: "IX_MatchPredictions_BlogPostId",
                table: "MatchPredictions");

            migrationBuilder.DropColumn(
                name: "BlogPostId",
                table: "MatchPredictions");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "MatchPredictions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlogPostId",
                table: "MatchPredictions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "MatchPredictions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_MatchPredictions_BlogPostId",
                table: "MatchPredictions",
                column: "BlogPostId");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchPredictions_Posts_BlogPostId",
                table: "MatchPredictions",
                column: "BlogPostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
