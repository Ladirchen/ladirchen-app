using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AllowRepeatedReadModelNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FamilyAggregateOutboxMessages_FamilyId_AggregateType_Revisi~",
                table: "FamilyAggregateOutboxMessages");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyAggregateOutboxMessages_FamilyId_AggregateType_Revisi~",
                table: "FamilyAggregateOutboxMessages",
                columns: new[] { "FamilyId", "AggregateType", "Revision" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FamilyAggregateOutboxMessages_FamilyId_AggregateType_Revisi~",
                table: "FamilyAggregateOutboxMessages");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyAggregateOutboxMessages_FamilyId_AggregateType_Revisi~",
                table: "FamilyAggregateOutboxMessages",
                columns: new[] { "FamilyId", "AggregateType", "Revision" },
                unique: true);
        }
    }
}
