using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class FamilyMemberEntity
{
  public Guid FamilyId { get; set; }
  public string MemberId { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public int Role { get; set; }
  public int GuardianAccess { get; set; }
  public bool InvitationPending { get; set; }
  public string? Nickname { get; set; }
  public string? EmailAddress { get; set; }
  public bool ParticipatesInWeeklyGoal { get; set; }
  public string? Avatar { get; set; }
  public string? Color { get; set; }
  public string? AppearanceAge { get; set; }
  public string? AppearanceSkinTone { get; set; }
  public string? AppearanceFaceShape { get; set; }
  public string? AppearanceFace { get; set; }
  public string? AppearanceHair { get; set; }
  public string? AppearanceHairColor { get; set; }
  public string? AppearanceOutfit { get; set; }
  public string? AppearanceOutfitColor { get; set; }
  public string? AppearanceAccessory { get; set; }
  public string? AppearanceFunAccessory { get; set; }
  public string? AppearanceSeasonalAccessory { get; set; }
  public FamilyRootEntity Family { get; set; } = null!;
}
