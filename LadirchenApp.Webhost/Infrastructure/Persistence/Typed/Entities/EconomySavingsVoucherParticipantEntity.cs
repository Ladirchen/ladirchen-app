using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class EconomySavingsVoucherParticipantEntity
{
  public Guid FamilyId { get; set; }
  public string GoalId { get; set; } = string.Empty;
  public string MemberId { get; set; } = string.Empty;
  public EconomySavingsVoucherEntity Voucher { get; set; } = null!;
}
