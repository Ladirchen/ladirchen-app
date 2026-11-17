using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContributionScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DueDate",
                table: "ContributionPresentations",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Recurrence",
                table: "ContributionPresentations",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "none");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "ContributionPresentations");

            migrationBuilder.DropColumn(
                name: "Recurrence",
                table: "ContributionPresentations");
        }
    }
}
