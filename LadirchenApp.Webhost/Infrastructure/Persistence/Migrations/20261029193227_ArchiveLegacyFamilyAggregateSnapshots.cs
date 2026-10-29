using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ArchiveLegacyFamilyAggregateSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "FamilyAggregateSnapshots",
                newName: "LegacyFamilyAggregateSnapshots");

            migrationBuilder.RenameIndex(
                name: "PK_FamilyAggregateSnapshots",
                table: "LegacyFamilyAggregateSnapshots",
                newName: "PK_LegacyFamilyAggregateSnapshots");

            migrationBuilder.AlterColumn<Guid>(
                name: "CommandId",
                table: "FamilyAggregateOutboxMessages",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DELETE FROM \"FamilyAggregateOutboxMessages\" WHERE \"CommandId\" IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "CommandId",
                table: "FamilyAggregateOutboxMessages",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.RenameIndex(
                name: "PK_LegacyFamilyAggregateSnapshots",
                table: "LegacyFamilyAggregateSnapshots",
                newName: "PK_FamilyAggregateSnapshots");

            migrationBuilder.RenameTable(
                name: "LegacyFamilyAggregateSnapshots",
                newName: "FamilyAggregateSnapshots");
        }
    }
}
