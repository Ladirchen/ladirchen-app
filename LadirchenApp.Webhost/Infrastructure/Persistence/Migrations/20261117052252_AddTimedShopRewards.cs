using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTimedShopRewards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "FamilyEconomyShopRewards",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "FamilyEconomyRedemptionRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TimerCancelledAt",
                table: "FamilyEconomyRedemptionRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimerRemainingSeconds",
                table: "FamilyEconomyRedemptionRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TimerStartedAt",
                table: "FamilyEconomyRedemptionRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE \"FamilyEconomyShopRewards\" SET \"DurationMinutes\" = 120 WHERE \"Category\" = 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FamilyEconomyShopRewards_Duration",
                table: "FamilyEconomyShopRewards",
                sql: "(\"Category\" = 0 AND \"DurationMinutes\" >= 1 AND \"DurationMinutes\" <= 1440) OR (\"Category\" <> 0 AND \"DurationMinutes\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FamilyEconomyRedemptionRequests_Timer",
                table: "FamilyEconomyRedemptionRequests",
                sql: "(\"DurationMinutes\" IS NULL AND \"TimerRemainingSeconds\" IS NULL AND \"TimerStartedAt\" IS NULL AND \"TimerCancelledAt\" IS NULL) OR (\"DurationMinutes\" >= 1 AND \"DurationMinutes\" <= 1440 AND \"TimerRemainingSeconds\" >= 0 AND (\"TimerCancelledAt\" IS NULL OR \"TimerStartedAt\" IS NULL))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FamilyEconomyShopRewards_Duration",
                table: "FamilyEconomyShopRewards");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FamilyEconomyRedemptionRequests_Timer",
                table: "FamilyEconomyRedemptionRequests");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "FamilyEconomyShopRewards");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "FamilyEconomyRedemptionRequests");

            migrationBuilder.DropColumn(
                name: "TimerCancelledAt",
                table: "FamilyEconomyRedemptionRequests");

            migrationBuilder.DropColumn(
                name: "TimerRemainingSeconds",
                table: "FamilyEconomyRedemptionRequests");

            migrationBuilder.DropColumn(
                name: "TimerStartedAt",
                table: "FamilyEconomyRedemptionRequests");
        }
    }
}
