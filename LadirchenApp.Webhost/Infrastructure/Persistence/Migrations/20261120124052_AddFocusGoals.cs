using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFocusGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProtectedAppointmentsCalendarUrl",
                table: "SchabiCalendarIntegrations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FocusBonusPaid",
                table: "ContributionPresentations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "FocusCompletionBonus",
                table: "ContributionPresentations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FocusEndDate",
                table: "ContributionPresentations",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FocusId",
                table: "ContributionPresentations",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FocusPeriod",
                table: "ContributionPresentations",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FocusStartDate",
                table: "ContributionPresentations",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFocus",
                table: "ContributionPresentations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PreparationMinutes",
                table: "ContributionPresentations",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProtectedAppointmentsCalendarUrl",
                table: "SchabiCalendarIntegrations");

            migrationBuilder.DropColumn(
                name: "FocusBonusPaid",
                table: "ContributionPresentations");

            migrationBuilder.DropColumn(
                name: "FocusCompletionBonus",
                table: "ContributionPresentations");

            migrationBuilder.DropColumn(
                name: "FocusEndDate",
                table: "ContributionPresentations");

            migrationBuilder.DropColumn(
                name: "FocusId",
                table: "ContributionPresentations");

            migrationBuilder.DropColumn(
                name: "FocusPeriod",
                table: "ContributionPresentations");

            migrationBuilder.DropColumn(
                name: "FocusStartDate",
                table: "ContributionPresentations");

            migrationBuilder.DropColumn(
                name: "IsFocus",
                table: "ContributionPresentations");

            migrationBuilder.DropColumn(
                name: "PreparationMinutes",
                table: "ContributionPresentations");
        }
    }
}
