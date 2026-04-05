using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballBlog.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixLiveMatchSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchEvents_LiveMatches_MatchId",
                table: "MatchEvents");

            migrationBuilder.RenameColumn(
                name: "MatchId",
                table: "MatchEvents",
                newName: "LiveMatchId");

            migrationBuilder.RenameIndex(
                name: "IX_MatchEvents_MatchId",
                table: "MatchEvents",
                newName: "IX_MatchEvents_LiveMatchId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "LiveMatches",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Scheduled",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "SCHEDULED");

            migrationBuilder.AddColumn<int>(
                name: "MatchId",
                table: "LiveMatches",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LiveMatches_MatchId",
                table: "LiveMatches",
                column: "MatchId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LiveMatches_Matches_MatchId",
                table: "LiveMatches",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchEvents_LiveMatches_LiveMatchId",
                table: "MatchEvents",
                column: "LiveMatchId",
                principalTable: "LiveMatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LiveMatches_Matches_MatchId",
                table: "LiveMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchEvents_LiveMatches_LiveMatchId",
                table: "MatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_LiveMatches_MatchId",
                table: "LiveMatches");

            migrationBuilder.DropColumn(
                name: "MatchId",
                table: "LiveMatches");

            migrationBuilder.RenameColumn(
                name: "LiveMatchId",
                table: "MatchEvents",
                newName: "MatchId");

            migrationBuilder.RenameIndex(
                name: "IX_MatchEvents_LiveMatchId",
                table: "MatchEvents",
                newName: "IX_MatchEvents_MatchId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "LiveMatches",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "SCHEDULED",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Scheduled");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchEvents_LiveMatches_MatchId",
                table: "MatchEvents",
                column: "MatchId",
                principalTable: "LiveMatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
