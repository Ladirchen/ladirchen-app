using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

internal sealed class SavingsVoucher
{
  private readonly HashSet<FamilyMemberId> _participants;
  private readonly HashSet<FamilyMemberId> _consents = new();

  public SavingsVoucher(decimal amount, FamilyMemberId? ownerId, IEnumerable<FamilyMemberId> participants)
  {
    Rules.Require(amount > 0, DomainError.InvalidValue, "A voucher amount must be positive.");
    Amount = amount;
    OwnerId = ownerId;
    _participants = new HashSet<FamilyMemberId>(participants ?? throw new ArgumentNullException(nameof(participants)));
  }

  public decimal Amount { get; }
  public FamilyMemberId? OwnerId { get; }
  public IReadOnlySet<FamilyMemberId> Participants
  {
    get
    {
      return _participants;
    }
  }
  public IReadOnlySet<FamilyMemberId> Consents
  {
    get
    {
      return _consents;
    }
  }
  public bool Redeemable
  {
    get
    {
      return RedemptionCount == 0
        && (OwnerId is not null || _participants.Count > 0 && _participants.All(_consents.Contains));
    }
  }
  public int RedemptionCount { get; private set; }

  public bool Consent(FamilyMemberId memberId)
  {
    ArgumentNullException.ThrowIfNull(memberId);

    return _participants.Contains(memberId) && _consents.Add(memberId);
  }

  public bool Redeem()
  {
    if (!Redeemable || RedemptionCount > 0)
    {
      return false;
    }

    RedemptionCount++;

    return true;
  }
}
