using System;

using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations
{
  /// <inheritdoc />
  public partial class AddPersonalMemberAccounts : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropCheckConstraint(
          name: "CK_FamilyMembers_PendingInvitation",
          table: "FamilyMembers");

      migrationBuilder.AddColumn<string>(
          name: "EmailAddress",
          table: "AuthenticationAccounts",
          type: "character varying(320)",
          maxLength: 320,
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "NormalizedEmail",
          table: "AuthenticationAccounts",
          type: "character varying(320)",
          maxLength: 320,
          nullable: true);

      migrationBuilder.CreateTable(
          name: "BetaFamilyRegistrations",
          columns: table => new
          {
            Id = table.Column<int>(type: "integer", nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
            RegisteredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_BetaFamilyRegistrations", x => x.Id);
            table.CheckConstraint("CK_BetaFamilyRegistrations_Singleton", "\"Id\" = 1");
          });

      migrationBuilder.CreateTable(
          name: "MemberAccountInvitations",
          columns: table => new
          {
            InvitationId = table.Column<Guid>(type: "uuid", nullable: false),
            FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
            FamilyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
            MemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
            EmailAddress = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
            NormalizedEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
            ActivationCodeHash = table.Column<byte[]>(type: "bytea", nullable: false),
            CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            RedeemedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_MemberAccountInvitations", x => x.InvitationId);
          });

      migrationBuilder.Sql(
          """
                WITH candidates AS (
                  SELECT account."Username",
                         member."EmailAddress",
                         lower(btrim(member."EmailAddress")) AS "NormalizedEmail",
                         count(*) OVER (
                           PARTITION BY lower(btrim(member."EmailAddress"))
                         ) AS "EmailCount"
                  FROM "AuthenticationAccounts" AS account
                  JOIN "FamilyMembers" AS member
                    ON account."FamilyId" = member."FamilyId"
                   AND account."MemberId" = member."MemberId"
                  WHERE member."EmailAddress" IS NOT NULL
                )
                UPDATE "AuthenticationAccounts" AS account
                SET "EmailAddress" = candidates."EmailAddress",
                    "NormalizedEmail" = CASE
                      WHEN candidates."EmailCount" = 1 THEN candidates."NormalizedEmail"
                      ELSE NULL
                    END
                FROM candidates
                WHERE account."Username" = candidates."Username";
                """);

      migrationBuilder.Sql(
          """
                INSERT INTO "BetaFamilyRegistrations" ("Id", "FamilyId", "RegisteredAt")
                SELECT 1, "FamilyId", CURRENT_TIMESTAMP
                FROM "Families"
                ORDER BY "UpdatedAt", "FamilyId"
                LIMIT 1
                ON CONFLICT ("Id") DO NOTHING;
                """);

      migrationBuilder.CreateIndex(
          name: "IX_AuthenticationAccounts_FamilyId_MemberId",
          table: "AuthenticationAccounts",
          columns: new[] { "FamilyId", "MemberId" },
          unique: true);

      migrationBuilder.CreateIndex(
          name: "IX_AuthenticationAccounts_NormalizedEmail",
          table: "AuthenticationAccounts",
          column: "NormalizedEmail",
          unique: true,
          filter: "\"NormalizedEmail\" IS NOT NULL");

      migrationBuilder.CreateIndex(
          name: "IX_BetaFamilyRegistrations_FamilyId",
          table: "BetaFamilyRegistrations",
          column: "FamilyId",
          unique: true);

      migrationBuilder.CreateIndex(
          name: "IX_MemberAccountInvitations_FamilyId_MemberId",
          table: "MemberAccountInvitations",
          columns: new[] { "FamilyId", "MemberId" },
          unique: true,
          filter: "\"RedeemedAt\" IS NULL");

      migrationBuilder.CreateIndex(
          name: "IX_MemberAccountInvitations_NormalizedEmail",
          table: "MemberAccountInvitations",
          column: "NormalizedEmail",
          unique: true,
          filter: "\"RedeemedAt\" IS NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql(
          """
                DELETE FROM "HousePlacements" AS placement
                USING "FamilyMembers" AS member
                WHERE member."InvitationPending"
                  AND member."Role" = 0
                  AND placement."FamilyId" = member."FamilyId"
                  AND placement."EntityType" = 'member'
                  AND placement."EntityId" = member."MemberId";

                DELETE FROM "FamilyMembers"
                WHERE "InvitationPending"
                  AND "Role" = 0;
                """);

      migrationBuilder.DropTable(
          name: "BetaFamilyRegistrations");

      migrationBuilder.DropTable(
          name: "MemberAccountInvitations");

      migrationBuilder.DropIndex(
          name: "IX_AuthenticationAccounts_FamilyId_MemberId",
          table: "AuthenticationAccounts");

      migrationBuilder.DropIndex(
          name: "IX_AuthenticationAccounts_NormalizedEmail",
          table: "AuthenticationAccounts");

      migrationBuilder.DropColumn(
          name: "EmailAddress",
          table: "AuthenticationAccounts");

      migrationBuilder.DropColumn(
          name: "NormalizedEmail",
          table: "AuthenticationAccounts");

      migrationBuilder.AddCheckConstraint(
          name: "CK_FamilyMembers_PendingInvitation",
          table: "FamilyMembers",
          sql: "NOT \"InvitationPending\" OR \"Role\" = 1");
    }
  }
}