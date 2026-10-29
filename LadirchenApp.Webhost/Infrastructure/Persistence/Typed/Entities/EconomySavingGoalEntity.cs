using System;
using System.Collections.Generic;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class EconomySavingGoalEntity
{
  public Guid FamilyId { get; set; }
  public string GoalId { get; set; } = string.Empty;
  public string? OwnerMemberId { get; set; }
  public string Title { get; set; } = string.Empty;
  public decimal Target { get; set; }
  public decimal Saved { get; set; }
  public decimal StarterBonus { get; set; }
  public decimal InterestEarned { get; set; }
  public int Visibility { get; set; }
  public bool Cheered { get; set; }
  public bool Cancelled { get; set; }
  public DateTimeOffset CreatedAt { get; set; }
  public int? LastInterestYear { get; set; }
  public int? LastInterestWeek { get; set; }
  public decimal? LastInterestRate { get; set; }
  public FamilyEconomyRootEntity Economy { get; set; } = null!;
  public ICollection<EconomySavingGoalParticipantEntity> Participants { get; set; } =
    new List<EconomySavingGoalParticipantEntity>();
  public ICollection<EconomySavingGoalRefundEntity> RefundableContributions { get; set; } =
    new List<EconomySavingGoalRefundEntity>();
  public EconomyGoalCancellationEntity? CancellationRequest { get; set; }
}
