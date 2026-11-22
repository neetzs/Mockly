using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChallengePositionAndOpeningConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "TechnicalChallenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill: los retos de búsquedas existentes se numeran por CreatedAtUtc, el criterio que se usaba antes.
            migrationBuilder.Sql("""
                UPDATE "TechnicalChallenges" AS t
                SET "Position" = o.pos
                FROM (
                    SELECT "Id", ROW_NUMBER() OVER (PARTITION BY "JobOpeningId" ORDER BY "CreatedAtUtc", "Id") - 1 AS pos
                    FROM "TechnicalChallenges"
                    WHERE "JobOpeningId" IS NOT NULL
                ) AS o
                WHERE t."Id" = o."Id";
                """);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "JobOpenings",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "TechnicalChallenges");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "JobOpenings");
        }
    }
}
