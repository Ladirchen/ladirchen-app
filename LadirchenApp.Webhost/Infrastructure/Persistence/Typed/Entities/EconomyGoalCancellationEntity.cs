using System;
using System.Collections.Generic;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class EconomyGoalCancellationEntity
{
  public Guid FamilyId { get; set; }
  public string GoalId { get; set; } = string.Empty;
  public string RequestedByMemberId { get; set; } = string.Empty;
  public EconomySavingGoalEntity Goal { get; set; } = null!;
  public ICollection<EconomyGoalCancellationConsentEntity> Consents { get; set; } =
    new List<EconomyGoalCancellationConsentEntity>();
}
