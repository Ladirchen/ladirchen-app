using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class GuardianGiftPresentationEntity
{
  public Guid FamilyId { get; set; }
  public string GiftId { get; set; } = string.Empty;
  public string SenderName { get; set; } = string.Empty;
  public string GoalTitle { get; set; } = string.Empty;
  public FamilyEconomyRootEntity Economy { get; set; } = null!;
}
