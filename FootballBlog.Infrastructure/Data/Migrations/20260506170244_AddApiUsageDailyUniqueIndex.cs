using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballBlog.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApiUsageDailyUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Service",
                table: "ApiUsageDaily",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_ApiUsageDaily_Date_Service",
                table: "ApiUsageDaily",
                columns: new[] { "Date", "Service" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApiUsageDaily_Date_Service",
                table: "ApiUsageDaily");

            migrationBuilder.AlterColumn<string>(
                name: "Service",
                table: "ApiUsageDaily",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
