using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShopRewardPromotions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "PromotionUntil",
                table: "FamilyEconomyShopRewards",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PromotionalPrice",
                table: "FamilyEconomyShopRewards",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_FamilyEconomyShopRewards_Promotion",
                table: "FamilyEconomyShopRewards",
                sql: "(\"PromotionalPrice\" IS NULL AND \"PromotionUntil\" IS NULL) OR (\"PromotionalPrice\" >= 0 AND \"PromotionalPrice\" < \"Price\" AND \"PromotionUntil\" IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FamilyEconomyShopRewards_Promotion",
                table: "FamilyEconomyShopRewards");

            migrationBuilder.DropColumn(
                name: "PromotionUntil",
                table: "FamilyEconomyShopRewards");

            migrationBuilder.DropColumn(
                name: "PromotionalPrice",
                table: "FamilyEconomyShopRewards");
        }
    }
}
