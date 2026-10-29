using System;
using System.Collections.Generic;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class EconomySavingsVoucherEntity
{
  public Guid FamilyId { get; set; }
  public string GoalId { get; set; } = string.Empty;
  public decimal Amount { get; set; }
  public string? OwnerMemberId { get; set; }
  public DateTimeOffset? RedeemedAt { get; set; }
  public FamilyEconomyRootEntity Economy { get; set; } = null!;
  public ICollection<EconomySavingsVoucherParticipantEntity> Participants { get; set; } =
    new List<EconomySavingsVoucherParticipantEntity>();
  public ICollection<EconomySavingsVoucherConsentEntity> Consents { get; set; } =
    new List<EconomySavingsVoucherConsentEntity>();
}
