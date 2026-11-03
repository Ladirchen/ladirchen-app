using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBehaviorDisciplineLedgerKinds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FamilyEconomyLedgerEntries_Kind",
                table: "FamilyEconomyLedgerEntries");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FamilyEconomyLedgerEntries_SourceKind",
                table: "FamilyEconomyLedgerEntries");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FamilyEconomyLedgerEntries_Kind",
                table: "FamilyEconomyLedgerEntries",
                sql: "\"Kind\" >= 0 AND \"Kind\" <= 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FamilyEconomyLedgerEntries_SourceKind",
                table: "FamilyEconomyLedgerEntries",
                sql: "\"SourceKind\" >= 0 AND \"SourceKind\" <= 5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FamilyEconomyLedgerEntries_Kind",
                table: "FamilyEconomyLedgerEntries");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FamilyEconomyLedgerEntries_SourceKind",
                table: "FamilyEconomyLedgerEntries");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FamilyEconomyLedgerEntries_Kind",
                table: "FamilyEconomyLedgerEntries",
                sql: "\"Kind\" >= 0 AND \"Kind\" <= 7");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FamilyEconomyLedgerEntries_SourceKind",
                table: "FamilyEconomyLedgerEntries",
                sql: "\"SourceKind\" >= 0 AND \"SourceKind\" <= 4");
        }
    }
}
