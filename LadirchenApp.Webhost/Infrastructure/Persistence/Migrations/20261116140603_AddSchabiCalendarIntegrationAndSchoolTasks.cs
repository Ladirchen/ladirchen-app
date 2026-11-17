using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSchabiCalendarIntegrationAndSchoolTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FamilyEconomyContributions_Kind",
                table: "FamilyEconomyContributions");

            migrationBuilder.CreateTable(
                name: "SchabiCalendarIntegrations",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SchabiUsername = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProtectedCalendarUrl = table.Column<string>(type: "text", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSuccessfulSyncAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchabiCalendarIntegrations", x => new { x.FamilyId, x.MemberId });
                    table.ForeignKey(
                        name: "FK_SchabiCalendarIntegrations_FamilyMembers_FamilyId_MemberId",
                        columns: x => new { x.FamilyId, x.MemberId },
                        principalTable: "FamilyMembers",
                        principalColumns: new[] { "FamilyId", "MemberId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_FamilyEconomyContributions_Kind",
                table: "FamilyEconomyContributions",
                sql: "\"Kind\" >= 0 AND \"Kind\" <= 2");

            migrationBuilder.CreateIndex(
                name: "IX_SchabiCalendarIntegrations_Enabled",
                table: "SchabiCalendarIntegrations",
                column: "Enabled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchabiCalendarIntegrations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FamilyEconomyContributions_Kind",
                table: "FamilyEconomyContributions");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FamilyEconomyContributions_Kind",
                table: "FamilyEconomyContributions",
                sql: "\"Kind\" >= 0 AND \"Kind\" <= 1");
        }
    }
}
