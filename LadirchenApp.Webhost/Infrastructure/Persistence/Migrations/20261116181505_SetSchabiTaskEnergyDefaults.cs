using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SetSchabiTaskEnergyDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "FamilyEconomyContributions"
                SET "Reward" = 0, "EnergyPercent" = 9
                WHERE "ContributionId" LIKE 'schabi-%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "FamilyEconomyContributions"
                SET "Reward" = 10, "EnergyPercent" = 0
                WHERE "ContributionId" LIKE 'schabi-%';
                """);
        }
    }
}
