using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballBlog.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTypeEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert existing string values → integer enum before changing column type.
            // Uses PostgreSQL USING clause to safely map old magic strings to new enum ints.
            // EventType: Goal=0, YellowCard=1, RedCard=2, Substitution=3, Penalty=4, Offside=5, Other=6
            migrationBuilder.Sql("""
                ALTER TABLE "MatchEvents"
                ALTER COLUMN "Type" TYPE integer
                USING CASE "Type"
                    WHEN 'Goal'         THEN 0
                    WHEN 'GOAL'         THEN 0
                    WHEN 'YellowCard'   THEN 1
                    WHEN 'YELLOW_CARD'  THEN 1
                    WHEN 'RedCard'      THEN 2
                    WHEN 'RED_CARD'     THEN 2
                    WHEN 'Substitution' THEN 3
                    WHEN 'SUBSTITUTION' THEN 3
                    WHEN 'Penalty'      THEN 4
                    WHEN 'PENALTY'      THEN 4
                    WHEN 'Offside'      THEN 5
                    WHEN 'OFFSIDE'      THEN 5
                    ELSE 6
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: convert integer enum → string representation
            migrationBuilder.Sql("""
                ALTER TABLE "MatchEvents"
                ALTER COLUMN "Type" TYPE character varying(50)
                USING CASE "Type"
                    WHEN 0 THEN 'Goal'
                    WHEN 1 THEN 'YellowCard'
                    WHEN 2 THEN 'RedCard'
                    WHEN 3 THEN 'Substitution'
                    WHEN 4 THEN 'Penalty'
                    WHEN 5 THEN 'Offside'
                    ELSE 'Other'
                END;
                """);
        }
    }
}
