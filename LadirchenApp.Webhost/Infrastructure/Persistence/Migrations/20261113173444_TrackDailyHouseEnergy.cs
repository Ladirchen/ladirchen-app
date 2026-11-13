using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TrackDailyHouseEnergy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FamilyProgressionDailyEnergies",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Energy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyProgressionDailyEnergies", x => new { x.FamilyId, x.Date });
                    table.CheckConstraint("CK_FamilyProgressionDailyEnergies_Energy", "\"Energy\" >= 0 AND \"Energy\" <= 100");
                    table.ForeignKey(
                        name: "FK_FamilyProgressionDailyEnergies_FamilyProgressions_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyProgressions",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FamilyProgressionDailyEnergies");
        }
    }
}
