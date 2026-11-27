using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFocusCompletionState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FocusCompleted",
                table: "ContributionPresentations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                """
                UPDATE "ContributionPresentations"
                SET "FocusCompleted" = TRUE
                WHERE "FocusBonusPaid" = TRUE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FocusCompleted",
                table: "ContributionPresentations");
        }
    }
}
