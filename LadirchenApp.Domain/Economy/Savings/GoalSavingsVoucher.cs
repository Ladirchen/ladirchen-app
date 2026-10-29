using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed class GoalSavingsVoucher : Entity<SavingGoalId>
{
  private readonly SavingsVoucher _voucher;

  internal GoalSavingsVoucher(
    SavingGoalId goalId,
    decimal amount,
    FamilyMemberId? ownerId,
    IEnumerable<FamilyMemberId> participants
  ) : base(goalId)
  {
    _voucher = new SavingsVoucher(amount, ownerId, participants);
  }

  public decimal Amount
  {
    get
    {
      return _voucher.Amount;
    }
  }

  public FamilyMemberId? OwnerId
  {
    get
    {
      return _voucher.OwnerId;
    }
  }

  public IReadOnlySet<FamilyMemberId> Participants
  {
    get
    {
      return _voucher.Participants;
    }
  }

  public IReadOnlySet<FamilyMemberId> Consents
  {
    get
    {
      return _voucher.Consents;
    }
  }

  public bool Redeemable
  {
    get
    {
      return _voucher.Redeemable;
    }
  }

  public int RedemptionCount
  {
    get
    {
      return _voucher.RedemptionCount;
    }
  }
  public DateTimeOffset? RedeemedAt { get; private set; }

  internal bool Consent(FamilyMemberId memberId)
  {
    return _voucher.Consent(memberId);
  }

  internal bool Redeem(DateTimeOffset now)
  {
    if (!_voucher.Redeem())
    {
      return false;
    }

    RedeemedAt = now.ToUniversalTime();

    return true;
  }

  internal void Restore(IEnumerable<FamilyMemberId> consents, DateTimeOffset? redeemedAt)
  {
    ArgumentNullException.ThrowIfNull(consents);

    foreach (var memberId in consents)
    {
      Rules.Require(
        Consent(memberId),
        DomainError.InvalidValue,
        "Only a participant can have stored voucher consent."
      );
    }

    if (redeemedAt is not null)
    {
      Rules.Require(Redeem(redeemedAt.Value), DomainError.InvalidState, "The restored voucher is not redeemable.");
    }
  }
}
