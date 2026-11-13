using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CleanupRedeemedSavingsVouchers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TEMP TABLE "RedeemedSavingsVoucherCleanup" ON COMMIT DROP AS
                SELECT "FamilyId", "GoalId"
                FROM "FamilyEconomySavingsVouchers"
                WHERE "RedeemedAt" IS NOT NULL;

                DELETE FROM "GuardianGiftPresentations" AS presentation
                USING "FamilyEconomyGuardianGifts" AS gift, "RedeemedSavingsVoucherCleanup" AS cleanup
                WHERE presentation."FamilyId" = gift."FamilyId"
                  AND presentation."GiftId" = gift."GiftId"
                  AND gift."FamilyId" = cleanup."FamilyId"
                  AND gift."GoalId" = cleanup."GoalId";

                DELETE FROM "FamilyEconomyGuardianGifts" AS gift
                USING "RedeemedSavingsVoucherCleanup" AS cleanup
                WHERE gift."FamilyId" = cleanup."FamilyId"
                  AND gift."GoalId" = cleanup."GoalId";

                DELETE FROM "SavingGoalPresentations" AS presentation
                USING "RedeemedSavingsVoucherCleanup" AS cleanup
                WHERE presentation."FamilyId" = cleanup."FamilyId"
                  AND presentation."GoalId" = cleanup."GoalId";

                DELETE FROM "FamilyEconomySavingsVoucherConsents" AS consent
                USING "RedeemedSavingsVoucherCleanup" AS cleanup
                WHERE consent."FamilyId" = cleanup."FamilyId"
                  AND consent."GoalId" = cleanup."GoalId";

                DELETE FROM "FamilyEconomySavingsVoucherParticipants" AS participant
                USING "RedeemedSavingsVoucherCleanup" AS cleanup
                WHERE participant."FamilyId" = cleanup."FamilyId"
                  AND participant."GoalId" = cleanup."GoalId";

                DELETE FROM "FamilyEconomySavingsVouchers" AS voucher
                USING "RedeemedSavingsVoucherCleanup" AS cleanup
                WHERE voucher."FamilyId" = cleanup."FamilyId"
                  AND voucher."GoalId" = cleanup."GoalId";

                DELETE FROM "FamilyEconomyGoalCancellationConsents" AS consent
                USING "RedeemedSavingsVoucherCleanup" AS cleanup
                WHERE consent."FamilyId" = cleanup."FamilyId"
                  AND consent."GoalId" = cleanup."GoalId";

                DELETE FROM "FamilyEconomyGoalCancellationRequests" AS request
                USING "RedeemedSavingsVoucherCleanup" AS cleanup
                WHERE request."FamilyId" = cleanup."FamilyId"
                  AND request."GoalId" = cleanup."GoalId";

                DELETE FROM "FamilyEconomySavingGoalRefunds" AS refund
                USING "RedeemedSavingsVoucherCleanup" AS cleanup
                WHERE refund."FamilyId" = cleanup."FamilyId"
                  AND refund."GoalId" = cleanup."GoalId";

                DELETE FROM "FamilyEconomySavingGoalParticipants" AS participant
                USING "RedeemedSavingsVoucherCleanup" AS cleanup
                WHERE participant."FamilyId" = cleanup."FamilyId"
                  AND participant."GoalId" = cleanup."GoalId";

                DELETE FROM "FamilyEconomySavingGoals" AS goal
                USING "RedeemedSavingsVoucherCleanup" AS cleanup
                WHERE goal."FamilyId" = cleanup."FamilyId"
                  AND goal."GoalId" = cleanup."GoalId";
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Redeemed vouchers and their completed goals cannot be reconstructed safely.
        }
    }
}
