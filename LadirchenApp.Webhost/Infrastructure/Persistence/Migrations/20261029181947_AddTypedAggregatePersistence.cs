using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTypedAggregatePersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Families",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: false),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    LadirchenPerCurrencyUnit = table.Column<int>(type: "integer", nullable: false),
                    ActiveChildId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OnboardingCompleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Families", x => x.FamilyId);
                    table.CheckConstraint("CK_Families_Currency", "\"Currency\" >= 0 AND \"Currency\" <= 2");
                    table.CheckConstraint("CK_Families_ExchangeRate", "\"LadirchenPerCurrencyUnit\" >= 1");
                    table.CheckConstraint("CK_Families_Revision", "\"Revision\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomies",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PerfectRatingBonusPercent = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomies", x => x.FamilyId);
                    table.CheckConstraint("CK_FamilyEconomies_PerfectRatingBonus", "\"PerfectRatingBonusPercent\" >= 0 AND \"PerfectRatingBonusPercent\" <= 100");
                    table.CheckConstraint("CK_FamilyEconomies_Revision", "\"Revision\" >= 0");
                    table.ForeignKey(
                        name: "FK_FamilyEconomies_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyMembers",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    GuardianAccess = table.Column<int>(type: "integer", nullable: false),
                    InvitationPending = table.Column<bool>(type: "boolean", nullable: false),
                    Nickname = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    EmailAddress = table.Column<string>(type: "text", nullable: true),
                    ParticipatesInWeeklyGoal = table.Column<bool>(type: "boolean", nullable: false),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true),
                    AppearanceAge = table.Column<string>(type: "text", nullable: true),
                    AppearanceSkinTone = table.Column<string>(type: "text", nullable: true),
                    AppearanceFaceShape = table.Column<string>(type: "text", nullable: true),
                    AppearanceFace = table.Column<string>(type: "text", nullable: true),
                    AppearanceHair = table.Column<string>(type: "text", nullable: true),
                    AppearanceHairColor = table.Column<string>(type: "text", nullable: true),
                    AppearanceOutfit = table.Column<string>(type: "text", nullable: true),
                    AppearanceOutfitColor = table.Column<string>(type: "text", nullable: true),
                    AppearanceAccessory = table.Column<string>(type: "text", nullable: true),
                    AppearanceFunAccessory = table.Column<string>(type: "text", nullable: true),
                    AppearanceSeasonalAccessory = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyMembers", x => new { x.FamilyId, x.MemberId });
                    table.CheckConstraint("CK_FamilyMembers_GuardianAccess", "\"GuardianAccess\" >= 0 AND \"GuardianAccess\" <= 1");
                    table.CheckConstraint("CK_FamilyMembers_PendingInvitation", "NOT \"InvitationPending\" OR \"Role\" = 1");
                    table.CheckConstraint("CK_FamilyMembers_Role", "\"Role\" >= 0 AND \"Role\" <= 1");
                    table.CheckConstraint("CK_FamilyMembers_RoleAccess", "\"Role\" = 1 OR \"GuardianAccess\" = 1");
                    table.ForeignKey(
                        name: "FK_FamilyMembers_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyPets",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    PetId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyPets", x => new { x.FamilyId, x.PetId });
                    table.CheckConstraint("CK_FamilyPets_Kind", "\"Kind\" >= 0 AND \"Kind\" <= 4");
                    table.ForeignKey(
                        name: "FK_FamilyPets_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyProgressions",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StoredLevel = table.Column<int>(type: "integer", nullable: false),
                    CompletedWeeklyStreak = table.Column<int>(type: "integer", nullable: false),
                    CurrentWeekTarget = table.Column<int>(type: "integer", nullable: false),
                    MaximumSavingsInterestRate = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyProgressions", x => x.FamilyId);
                    table.CheckConstraint("CK_FamilyProgressions_CompletedWeeklyStreak", "\"CompletedWeeklyStreak\" >= 0");
                    table.CheckConstraint("CK_FamilyProgressions_CurrentWeekTarget", "\"CurrentWeekTarget\" >= 1");
                    table.CheckConstraint("CK_FamilyProgressions_MaximumSavingsInterestRate", "\"MaximumSavingsInterestRate\" > 0 AND \"MaximumSavingsInterestRate\" <= 100");
                    table.CheckConstraint("CK_FamilyProgressions_Revision", "\"Revision\" >= 0");
                    table.CheckConstraint("CK_FamilyProgressions_StoredLevel", "\"StoredLevel\" >= 0 AND \"StoredLevel\" <= 4");
                    table.ForeignKey(
                        name: "FK_FamilyProgressions_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HouseCustomizations",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SelectedThemeId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubscriptionTier = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseCustomizations", x => x.FamilyId);
                    table.CheckConstraint("CK_HouseCustomizations_Revision", "\"Revision\" >= 0");
                    table.CheckConstraint("CK_HouseCustomizations_SubscriptionTier", "\"SubscriptionTier\" >= 0 AND \"SubscriptionTier\" <= 1");
                    table.ForeignKey(
                        name: "FK_HouseCustomizations_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContributionPresentations",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContributionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: false),
                    RewardCelebrated = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContributionPresentations", x => new { x.FamilyId, x.ContributionId });
                    table.ForeignKey(
                        name: "FK_ContributionPresentations_FamilyEconomies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyEconomies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomyAppliedOperations",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomyAppliedOperations", x => new { x.FamilyId, x.OperationId });
                    table.ForeignKey(
                        name: "FK_FamilyEconomyAppliedOperations_FamilyEconomies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyEconomies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomyGuardianGifts",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageId = table.Column<Guid>(type: "uuid", nullable: false),
                    GiftId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OwnerMemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    GuardianMemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    GoalId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Destination = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomyGuardianGifts", x => new { x.FamilyId, x.StorageId });
                    table.CheckConstraint("CK_FamilyEconomyGuardianGifts_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_FamilyEconomyGuardianGifts_BalanceDestination", "\"Destination\" <> 0 OR \"GoalId\" IS NULL");
                    table.CheckConstraint("CK_FamilyEconomyGuardianGifts_Destination", "\"Destination\" >= 0 AND \"Destination\" <= 1");
                    table.ForeignKey(
                        name: "FK_FamilyEconomyGuardianGifts_FamilyEconomies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyEconomies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomySavingGoals",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OwnerMemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Target = table.Column<decimal>(type: "numeric", nullable: false),
                    Saved = table.Column<decimal>(type: "numeric", nullable: false),
                    StarterBonus = table.Column<decimal>(type: "numeric", nullable: false),
                    InterestEarned = table.Column<decimal>(type: "numeric", nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    Cheered = table.Column<bool>(type: "boolean", nullable: false),
                    Cancelled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastInterestYear = table.Column<int>(type: "integer", nullable: true),
                    LastInterestWeek = table.Column<int>(type: "integer", nullable: true),
                    LastInterestRate = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomySavingGoals", x => new { x.FamilyId, x.GoalId });
                    table.CheckConstraint("CK_FamilyEconomySavingGoals_Amounts", "\"Saved\" >= 0 AND \"StarterBonus\" >= 0 AND \"InterestEarned\" >= 0");
                    table.CheckConstraint("CK_FamilyEconomySavingGoals_LastInterestPeriod", "(\"LastInterestYear\" IS NULL AND \"LastInterestWeek\" IS NULL AND \"LastInterestRate\" IS NULL) OR (\"LastInterestYear\" IS NOT NULL AND \"LastInterestWeek\" IS NOT NULL AND \"LastInterestRate\" IS NOT NULL)");
                    table.CheckConstraint("CK_FamilyEconomySavingGoals_LastInterestRate", "\"LastInterestRate\" IS NULL OR (\"LastInterestRate\" >= 0 AND \"LastInterestRate\" <= 100)");
                    table.CheckConstraint("CK_FamilyEconomySavingGoals_LastInterestWeek", "\"LastInterestYear\" IS NULL OR (\"LastInterestYear\" >= 1 AND \"LastInterestYear\" <= 9999 AND \"LastInterestWeek\" >= 1 AND \"LastInterestWeek\" <= 53)");
                    table.CheckConstraint("CK_FamilyEconomySavingGoals_OwnerVisibility", "\"OwnerMemberId\" IS NOT NULL OR \"Visibility\" = 0");
                    table.CheckConstraint("CK_FamilyEconomySavingGoals_Target", "\"Target\" > 0");
                    table.CheckConstraint("CK_FamilyEconomySavingGoals_Visibility", "\"Visibility\" >= 0 AND \"Visibility\" <= 2");
                    table.ForeignKey(
                        name: "FK_FamilyEconomySavingGoals_FamilyEconomies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyEconomies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomySettledInterestPeriods",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsoYear = table.Column<int>(type: "integer", nullable: false),
                    IsoWeek = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomySettledInterestPeriods", x => new { x.FamilyId, x.IsoYear, x.IsoWeek });
                    table.CheckConstraint("CK_FamilyEconomySettledInterestPeriods_Week", "\"IsoYear\" >= 1 AND \"IsoYear\" <= 9999 AND \"IsoWeek\" >= 1 AND \"IsoWeek\" <= 53");
                    table.ForeignKey(
                        name: "FK_FamilyEconomySettledInterestPeriods_FamilyEconomies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyEconomies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomyShopRewards",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RewardId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    AvailableFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    AvailableUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomyShopRewards", x => new { x.FamilyId, x.RewardId });
                    table.CheckConstraint("CK_FamilyEconomyShopRewards_AvailabilityWindow", "\"AvailableFrom\" IS NULL OR \"AvailableUntil\" IS NULL OR \"AvailableFrom\" <= \"AvailableUntil\"");
                    table.CheckConstraint("CK_FamilyEconomyShopRewards_Category", "\"Category\" >= 0 AND \"Category\" <= 5");
                    table.CheckConstraint("CK_FamilyEconomyShopRewards_Price", "\"Price\" >= 0");
                    table.CheckConstraint("CK_FamilyEconomyShopRewards_Quantity", "\"Quantity\" >= 0");
                    table.CheckConstraint("CK_FamilyEconomyShopRewards_Status", "\"Status\" >= 0 AND \"Status\" <= 2");
                    table.ForeignKey(
                        name: "FK_FamilyEconomyShopRewards_FamilyEconomies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyEconomies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomyUsedContributionIds",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContributionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomyUsedContributionIds", x => new { x.FamilyId, x.ContributionId });
                    table.ForeignKey(
                        name: "FK_FamilyEconomyUsedContributionIds_FamilyEconomies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyEconomies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomyWallets",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomyWallets", x => new { x.FamilyId, x.MemberId });
                    table.CheckConstraint("CK_FamilyEconomyWallets_Balance", "\"Balance\" >= 0");
                    table.ForeignKey(
                        name: "FK_FamilyEconomyWallets_FamilyEconomies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyEconomies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuardianGiftPresentations",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    GiftId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SenderName = table.Column<string>(type: "text", nullable: false),
                    GoalTitle = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuardianGiftPresentations", x => new { x.FamilyId, x.GiftId });
                    table.ForeignKey(
                        name: "FK_GuardianGiftPresentations_FamilyEconomies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyEconomies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavingGoalPresentations",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingGoalPresentations", x => new { x.FamilyId, x.GoalId });
                    table.ForeignKey(
                        name: "FK_SavingGoalPresentations_FamilyEconomies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyEconomies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShopRewardPresentations",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RewardId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: false),
                    Conditions = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopRewardPresentations", x => new { x.FamilyId, x.RewardId });
                    table.ForeignKey(
                        name: "FK_ShopRewardPresentations_FamilyEconomies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyEconomies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClosedFamilyWeeks",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsoYear = table.Column<int>(type: "integer", nullable: false),
                    IsoWeek = table.Column<int>(type: "integer", nullable: false),
                    Outcome = table.Column<int>(type: "integer", nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClosedFamilyWeeks", x => new { x.FamilyId, x.IsoYear, x.IsoWeek });
                    table.CheckConstraint("CK_ClosedFamilyWeeks_IsoWeek", "\"IsoWeek\" >= 1 AND \"IsoWeek\" <= 53");
                    table.CheckConstraint("CK_ClosedFamilyWeeks_IsoYear", "\"IsoYear\" >= 1 AND \"IsoYear\" <= 9999");
                    table.CheckConstraint("CK_ClosedFamilyWeeks_Outcome", "\"Outcome\" >= 0 AND \"Outcome\" <= 1");
                    table.ForeignKey(
                        name: "FK_ClosedFamilyWeeks_FamilyProgressions_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyProgressions",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyProgressionParticipants",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StoredWeeklyStreak = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyProgressionParticipants", x => new { x.FamilyId, x.MemberId });
                    table.CheckConstraint("CK_FamilyProgressionParticipants_StoredWeeklyStreak", "\"StoredWeeklyStreak\" >= 0");
                    table.ForeignKey(
                        name: "FK_FamilyProgressionParticipants_FamilyProgressions_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyProgressions",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HouseAccessories",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessoryId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Owned = table.Column<bool>(type: "boolean", nullable: false),
                    Equipped = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseAccessories", x => new { x.FamilyId, x.AccessoryId });
                    table.CheckConstraint("CK_HouseAccessories_Equipped", "NOT \"Equipped\" OR \"Owned\"");
                    table.ForeignKey(
                        name: "FK_HouseAccessories_HouseCustomizations_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "HouseCustomizations",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HousePlacements",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlacementId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Area = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    X = table.Column<decimal>(type: "numeric", nullable: false),
                    Y = table.Column<decimal>(type: "numeric", nullable: false),
                    Scale = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HousePlacements", x => new { x.FamilyId, x.PlacementId });
                    table.CheckConstraint("CK_HousePlacements_Scale", "\"Scale\" > 0");
                    table.ForeignKey(
                        name: "FK_HousePlacements_HouseCustomizations_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "HouseCustomizations",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HouseRoomDesigns",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomDesignId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Owned = table.Column<bool>(type: "boolean", nullable: false),
                    Selected = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseRoomDesigns", x => new { x.FamilyId, x.RoomDesignId });
                    table.CheckConstraint("CK_HouseRoomDesigns_Selected", "NOT \"Selected\" OR \"Owned\"");
                    table.ForeignKey(
                        name: "FK_HouseRoomDesigns_HouseCustomizations_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "HouseCustomizations",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HouseThemes",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Owned = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseThemes", x => new { x.FamilyId, x.ThemeId });
                    table.ForeignKey(
                        name: "FK_HouseThemes_HouseCustomizations_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "HouseCustomizations",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomyLedgerEntries",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    GoalId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    SourceKind = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    InitiatorMemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomyLedgerEntries", x => new { x.FamilyId, x.EntryId });
                    table.CheckConstraint("CK_FamilyEconomyLedgerEntries_Amount", "\"Amount\" >= 0");
                    table.CheckConstraint("CK_FamilyEconomyLedgerEntries_Kind", "\"Kind\" >= 0 AND \"Kind\" <= 7");
                    table.CheckConstraint("CK_FamilyEconomyLedgerEntries_SourceKind", "\"SourceKind\" >= 0 AND \"SourceKind\" <= 4");
                    table.ForeignKey(
                        name: "FK_FamilyEconomyLedgerEntries_FamilyEconomies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyEconomies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FamilyEconomyLedgerEntries_FamilyEconomyAppliedOperations_F~",
                        columns: x => new { x.FamilyId, x.OperationId },
                        principalTable: "FamilyEconomyAppliedOperations",
                        principalColumns: new[] { "FamilyId", "OperationId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomyGoalCancellationRequests",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RequestedByMemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomyGoalCancellationRequests", x => new { x.FamilyId, x.GoalId });
                    table.ForeignKey(
                        name: "FK_FamilyEconomyGoalCancellationRequests_FamilyEconomySavingGo~",
                        columns: x => new { x.FamilyId, x.GoalId },
                        principalTable: "FamilyEconomySavingGoals",
                        principalColumns: new[] { "FamilyId", "GoalId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomySavingGoalParticipants",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomySavingGoalParticipants", x => new { x.FamilyId, x.GoalId, x.MemberId });
                    table.ForeignKey(
                        name: "FK_FamilyEconomySavingGoalParticipants_FamilyEconomySavingGoal~",
                        columns: x => new { x.FamilyId, x.GoalId },
                        principalTable: "FamilyEconomySavingGoals",
                        principalColumns: new[] { "FamilyId", "GoalId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomySavingGoalRefunds",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomySavingGoalRefunds", x => new { x.FamilyId, x.GoalId, x.MemberId });
                    table.CheckConstraint("CK_FamilyEconomySavingGoalRefunds_Amount", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_FamilyEconomySavingGoalRefunds_FamilyEconomySavingGoals_Fam~",
                        columns: x => new { x.FamilyId, x.GoalId },
                        principalTable: "FamilyEconomySavingGoals",
                        principalColumns: new[] { "FamilyId", "GoalId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomySavingsVouchers",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    OwnerMemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RedeemedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomySavingsVouchers", x => new { x.FamilyId, x.GoalId });
                    table.CheckConstraint("CK_FamilyEconomySavingsVouchers_Amount", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_FamilyEconomySavingsVouchers_FamilyEconomies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyEconomies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FamilyEconomySavingsVouchers_FamilyEconomySavingGoals_Famil~",
                        columns: x => new { x.FamilyId, x.GoalId },
                        principalTable: "FamilyEconomySavingGoals",
                        principalColumns: new[] { "FamilyId", "GoalId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomyRedemptionRequests",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RewardId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RequesterMemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DecidedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomyRedemptionRequests", x => new { x.FamilyId, x.RequestId });
                    table.CheckConstraint("CK_FamilyEconomyRedemptionRequests_Decision", "(\"Status\" = 0 AND \"DecidedAt\" IS NULL) OR (\"Status\" <> 0 AND \"DecidedAt\" IS NOT NULL)");
                    table.CheckConstraint("CK_FamilyEconomyRedemptionRequests_Price", "\"Price\" >= 0");
                    table.CheckConstraint("CK_FamilyEconomyRedemptionRequests_Status", "\"Status\" >= 0 AND \"Status\" <= 3");
                    table.ForeignKey(
                        name: "FK_FamilyEconomyRedemptionRequests_FamilyEconomies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyEconomies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FamilyEconomyRedemptionRequests_FamilyEconomyShopRewards_Fa~",
                        columns: x => new { x.FamilyId, x.RewardId },
                        principalTable: "FamilyEconomyShopRewards",
                        principalColumns: new[] { "FamilyId", "RewardId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomyContributions",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContributionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Reward = table.Column<decimal>(type: "numeric", nullable: false),
                    EnergyPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    AssigneeId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: true),
                    EarnedBaseReward = table.Column<decimal>(type: "numeric", nullable: true),
                    EarnedRatingBonus = table.Column<decimal>(type: "numeric", nullable: true),
                    EarnedPromotionMultiplier = table.Column<decimal>(type: "numeric", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomyContributions", x => new { x.FamilyId, x.ContributionId });
                    table.CheckConstraint("CK_FamilyEconomyContributions_Approval", "(\"Status\" = 2 AND \"Rating\" IS NOT NULL AND \"EarnedBaseReward\" IS NOT NULL AND \"EarnedRatingBonus\" IS NOT NULL AND \"EarnedPromotionMultiplier\" IS NOT NULL AND \"ApprovedAt\" IS NOT NULL) OR (\"Status\" <> 2 AND \"Rating\" IS NULL AND \"EarnedBaseReward\" IS NULL AND \"EarnedRatingBonus\" IS NULL AND \"EarnedPromotionMultiplier\" IS NULL AND \"ApprovedAt\" IS NULL)");
                    table.CheckConstraint("CK_FamilyEconomyContributions_Assignee", "\"Status\" = 0 OR \"AssigneeId\" IS NOT NULL");
                    table.CheckConstraint("CK_FamilyEconomyContributions_Energy", "\"EnergyPercent\" >= 0 AND \"EnergyPercent\" <= 100");
                    table.CheckConstraint("CK_FamilyEconomyContributions_Kind", "\"Kind\" >= 0 AND \"Kind\" <= 1");
                    table.CheckConstraint("CK_FamilyEconomyContributions_Rating", "\"Rating\" IS NULL OR (\"Rating\" >= 1 AND \"Rating\" <= 5)");
                    table.CheckConstraint("CK_FamilyEconomyContributions_Reward", "\"Reward\" >= 0");
                    table.CheckConstraint("CK_FamilyEconomyContributions_Status", "\"Status\" >= 0 AND \"Status\" <= 2");
                    table.ForeignKey(
                        name: "FK_FamilyEconomyContributions_FamilyEconomies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyEconomies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FamilyEconomyContributions_FamilyEconomyUsedContributionIds~",
                        columns: x => new { x.FamilyId, x.ContributionId },
                        principalTable: "FamilyEconomyUsedContributionIds",
                        principalColumns: new[] { "FamilyId", "ContributionId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClosedFamilyWeekEvaluations",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsoYear = table.Column<int>(type: "integer", nullable: false),
                    IsoWeek = table.Column<int>(type: "integer", nullable: false),
                    MemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StreakDays = table.Column<int>(type: "integer", nullable: false),
                    CompletionPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    AverageBasicRating = table.Column<decimal>(type: "numeric", nullable: false),
                    UncappedParticipationValue = table.Column<decimal>(type: "numeric", nullable: false),
                    CappedParticipationValue = table.Column<decimal>(type: "numeric", nullable: false),
                    LadiStageId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LadiStagePosition = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClosedFamilyWeekEvaluations", x => new { x.FamilyId, x.IsoYear, x.IsoWeek, x.MemberId });
                    table.CheckConstraint("CK_ClosedFamilyWeekEvaluations_AverageBasicRating", "\"AverageBasicRating\" >= 0 AND \"AverageBasicRating\" <= 5");
                    table.CheckConstraint("CK_ClosedFamilyWeekEvaluations_CappedValue", "\"CappedParticipationValue\" >= 0 AND \"CappedParticipationValue\" <= 8");
                    table.CheckConstraint("CK_ClosedFamilyWeekEvaluations_CompletionPercent", "\"CompletionPercent\" >= 0 AND \"CompletionPercent\" <= 100");
                    table.CheckConstraint("CK_ClosedFamilyWeekEvaluations_LadiStagePosition", "\"LadiStagePosition\" >= 1 AND \"LadiStagePosition\" <= 6");
                    table.CheckConstraint("CK_ClosedFamilyWeekEvaluations_StreakDays", "\"StreakDays\" >= 0");
                    table.CheckConstraint("CK_ClosedFamilyWeekEvaluations_UncappedValue", "\"UncappedParticipationValue\" >= 0");
                    table.ForeignKey(
                        name: "FK_ClosedFamilyWeekEvaluations_ClosedFamilyWeeks_FamilyId_IsoY~",
                        columns: x => new { x.FamilyId, x.IsoYear, x.IsoWeek },
                        principalTable: "ClosedFamilyWeeks",
                        principalColumns: new[] { "FamilyId", "IsoYear", "IsoWeek" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomyGoalCancellationConsents",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomyGoalCancellationConsents", x => new { x.FamilyId, x.GoalId, x.MemberId });
                    table.ForeignKey(
                        name: "FK_FamilyEconomyGoalCancellationConsents_FamilyEconomyGoalCanc~",
                        columns: x => new { x.FamilyId, x.GoalId },
                        principalTable: "FamilyEconomyGoalCancellationRequests",
                        principalColumns: new[] { "FamilyId", "GoalId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FamilyEconomyGoalCancellationConsents_FamilyEconomySavingGo~",
                        columns: x => new { x.FamilyId, x.GoalId, x.MemberId },
                        principalTable: "FamilyEconomySavingGoalParticipants",
                        principalColumns: new[] { "FamilyId", "GoalId", "MemberId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomySavingsVoucherParticipants",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomySavingsVoucherParticipants", x => new { x.FamilyId, x.GoalId, x.MemberId });
                    table.ForeignKey(
                        name: "FK_FamilyEconomySavingsVoucherParticipants_FamilyEconomySaving~",
                        columns: x => new { x.FamilyId, x.GoalId },
                        principalTable: "FamilyEconomySavingsVouchers",
                        principalColumns: new[] { "FamilyId", "GoalId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomyContributionPartners",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContributionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomyContributionPartners", x => new { x.FamilyId, x.ContributionId, x.MemberId });
                    table.ForeignKey(
                        name: "FK_FamilyEconomyContributionPartners_FamilyEconomyContribution~",
                        columns: x => new { x.FamilyId, x.ContributionId },
                        principalTable: "FamilyEconomyContributions",
                        principalColumns: new[] { "FamilyId", "ContributionId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomyPromotions",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    PromotionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ContributionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Multiplier = table.Column<decimal>(type: "numeric", nullable: false),
                    TeamworkBonus = table.Column<decimal>(type: "numeric", nullable: false),
                    DeadlineLocalTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    DeadlineLocalDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomyPromotions", x => new { x.FamilyId, x.PromotionId });
                    table.CheckConstraint("CK_FamilyEconomyPromotions_Multiplier", "\"Multiplier\" IN (2, 3)");
                    table.CheckConstraint("CK_FamilyEconomyPromotions_TeamworkBonus", "\"TeamworkBonus\" >= 0");
                    table.ForeignKey(
                        name: "FK_FamilyEconomyPromotions_FamilyEconomies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyEconomies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FamilyEconomyPromotions_FamilyEconomyContributions_FamilyId~",
                        columns: x => new { x.FamilyId, x.ContributionId },
                        principalTable: "FamilyEconomyContributions",
                        principalColumns: new[] { "FamilyId", "ContributionId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEconomySavingsVoucherConsents",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEconomySavingsVoucherConsents", x => new { x.FamilyId, x.GoalId, x.MemberId });
                    table.ForeignKey(
                        name: "FK_FamilyEconomySavingsVoucherConsents_FamilyEconomySavingsVou~",
                        columns: x => new { x.FamilyId, x.GoalId },
                        principalTable: "FamilyEconomySavingsVouchers",
                        principalColumns: new[] { "FamilyId", "GoalId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FamilyEconomySavingsVoucherConsents_FamilyEconomySavingsVo~1",
                        columns: x => new { x.FamilyId, x.GoalId, x.MemberId },
                        principalTable: "FamilyEconomySavingsVoucherParticipants",
                        principalColumns: new[] { "FamilyId", "GoalId", "MemberId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyEconomyGuardianGifts_FamilyId_GiftId",
                table: "FamilyEconomyGuardianGifts",
                columns: new[] { "FamilyId", "GiftId" },
                unique: true,
                filter: "\"GiftId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyEconomyLedgerEntries_FamilyId_MemberId_OccurredAtUtc",
                table: "FamilyEconomyLedgerEntries",
                columns: new[] { "FamilyId", "MemberId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyEconomyLedgerEntries_FamilyId_OperationId",
                table: "FamilyEconomyLedgerEntries",
                columns: new[] { "FamilyId", "OperationId" });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyEconomyPromotions_FamilyId_ContributionId",
                table: "FamilyEconomyPromotions",
                columns: new[] { "FamilyId", "ContributionId" });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyEconomyRedemptionRequests_FamilyId_RequesterMemberId_~",
                table: "FamilyEconomyRedemptionRequests",
                columns: new[] { "FamilyId", "RequesterMemberId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyEconomyRedemptionRequests_FamilyId_RewardId",
                table: "FamilyEconomyRedemptionRequests",
                columns: new[] { "FamilyId", "RewardId" },
                unique: true,
                filter: "\"Status\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMembers_FamilyId_EmailAddress",
                table: "FamilyMembers",
                columns: new[] { "FamilyId", "EmailAddress" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClosedFamilyWeekEvaluations");

            migrationBuilder.DropTable(
                name: "ContributionPresentations");

            migrationBuilder.DropTable(
                name: "FamilyEconomyContributionPartners");

            migrationBuilder.DropTable(
                name: "FamilyEconomyGoalCancellationConsents");

            migrationBuilder.DropTable(
                name: "FamilyEconomyGuardianGifts");

            migrationBuilder.DropTable(
                name: "FamilyEconomyLedgerEntries");

            migrationBuilder.DropTable(
                name: "FamilyEconomyPromotions");

            migrationBuilder.DropTable(
                name: "FamilyEconomyRedemptionRequests");

            migrationBuilder.DropTable(
                name: "FamilyEconomySavingGoalRefunds");

            migrationBuilder.DropTable(
                name: "FamilyEconomySavingsVoucherConsents");

            migrationBuilder.DropTable(
                name: "FamilyEconomySettledInterestPeriods");

            migrationBuilder.DropTable(
                name: "FamilyEconomyWallets");

            migrationBuilder.DropTable(
                name: "FamilyMembers");

            migrationBuilder.DropTable(
                name: "FamilyPets");

            migrationBuilder.DropTable(
                name: "FamilyProgressionParticipants");

            migrationBuilder.DropTable(
                name: "GuardianGiftPresentations");

            migrationBuilder.DropTable(
                name: "HouseAccessories");

            migrationBuilder.DropTable(
                name: "HousePlacements");

            migrationBuilder.DropTable(
                name: "HouseRoomDesigns");

            migrationBuilder.DropTable(
                name: "HouseThemes");

            migrationBuilder.DropTable(
                name: "SavingGoalPresentations");

            migrationBuilder.DropTable(
                name: "ShopRewardPresentations");

            migrationBuilder.DropTable(
                name: "ClosedFamilyWeeks");

            migrationBuilder.DropTable(
                name: "FamilyEconomyGoalCancellationRequests");

            migrationBuilder.DropTable(
                name: "FamilyEconomySavingGoalParticipants");

            migrationBuilder.DropTable(
                name: "FamilyEconomyAppliedOperations");

            migrationBuilder.DropTable(
                name: "FamilyEconomyContributions");

            migrationBuilder.DropTable(
                name: "FamilyEconomyShopRewards");

            migrationBuilder.DropTable(
                name: "FamilyEconomySavingsVoucherParticipants");

            migrationBuilder.DropTable(
                name: "HouseCustomizations");

            migrationBuilder.DropTable(
                name: "FamilyProgressions");

            migrationBuilder.DropTable(
                name: "FamilyEconomyUsedContributionIds");

            migrationBuilder.DropTable(
                name: "FamilyEconomySavingsVouchers");

            migrationBuilder.DropTable(
                name: "FamilyEconomySavingGoals");

            migrationBuilder.DropTable(
                name: "FamilyEconomies");

            migrationBuilder.DropTable(
                name: "Families");
        }
    }
}
