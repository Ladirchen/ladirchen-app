using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSchabiSyncHorizon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SyncHorizonDays",
                table: "SchabiCalendarIntegrations",
                type: "integer",
                nullable: false,
                defaultValue: 14);

            migrationBuilder.AddCheckConstraint(
                name: "CK_SchabiCalendarIntegrations_SyncHorizonDays",
                table: "SchabiCalendarIntegrations",
                sql: "\"SyncHorizonDays\" >= 1 AND \"SyncHorizonDays\" <= 90");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_SchabiCalendarIntegrations_SyncHorizonDays",
                table: "SchabiCalendarIntegrations");

            migrationBuilder.DropColumn(
                name: "SyncHorizonDays",
                table: "SchabiCalendarIntegrations");
        }
    }
}
