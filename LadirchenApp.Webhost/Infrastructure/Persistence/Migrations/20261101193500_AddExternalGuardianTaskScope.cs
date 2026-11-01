using LadirchenApp.Webhost.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations;

[DbContext(typeof(LadirchenDbContext))]
[Migration("20261101193500_AddExternalGuardianTaskScope")]
public sealed class AddExternalGuardianTaskScope : Migration
{
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropCheckConstraint(
      name: "CK_FamilyMembers_GuardianAccess",
      table: "FamilyMembers"
    );
    migrationBuilder.AddColumn<string>(
      name: "CreatorId",
      table: "FamilyEconomyContributions",
      type: "character varying(128)",
      maxLength: 128,
      nullable: true
    );
    migrationBuilder.AddCheckConstraint(
      name: "CK_FamilyMembers_GuardianAccess",
      table: "FamilyMembers",
      sql: "\"GuardianAccess\" >= 0 AND \"GuardianAccess\" <= 2"
    );
  }

  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropCheckConstraint(
      name: "CK_FamilyMembers_GuardianAccess",
      table: "FamilyMembers"
    );
    migrationBuilder.DropColumn(
      name: "CreatorId",
      table: "FamilyEconomyContributions"
    );
    migrationBuilder.AddCheckConstraint(
      name: "CK_FamilyMembers_GuardianAccess",
      table: "FamilyMembers",
      sql: "\"GuardianAccess\" >= 0 AND \"GuardianAccess\" <= 1"
    );
  }
}
