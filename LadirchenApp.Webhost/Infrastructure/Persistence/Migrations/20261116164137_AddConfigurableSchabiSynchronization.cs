using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurableSchabiSynchronization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SyncDayOfWeek",
                table: "SchabiCalendarIntegrations",
                type: "integer",
                nullable: false,
                defaultValue: 4);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "SyncTimeLocal",
                table: "SchabiCalendarIntegrations",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(6, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SyncDayOfWeek",
                table: "SchabiCalendarIntegrations");

            migrationBuilder.DropColumn(
                name: "SyncTimeLocal",
                table: "SchabiCalendarIntegrations");
        }
    }
}
