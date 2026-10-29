using System;
using System.Linq;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Economy;

public sealed partial class FamilyEconomy
{
  public FamilyEconomy SynchronizeMembers(FamilyAggregate family)
  {
    ArgumentNullException.ThrowIfNull(family);
    Rules.Require(
      family.Id == Id,
      DomainError.InvalidState,
      "The family economy belongs to another family."
    );
    var nextMembers = SnapshotMembers(family);
    var nextActiveMemberIds = nextMembers
      .Where(item => !item.InvitationPending)
      .Select(item => item.Id)
      .ToHashSet();
    var departingMemberIds = _members
      .Where(item => !item.InvitationPending && !nextActiveMemberIds.Contains(item.Id))
      .Select(item => item.Id)
      .ToArray();

    foreach (var memberId in departingMemberIds)
    {
      RequireMemberCanLeave(memberId);
    }

    var walletStates = nextActiveMemberIds
      .OrderBy(item => item.Value, StringComparer.Ordinal)
      .Select(memberId =>
      {
        var wallet = _wallets.Find(item => item.Id == memberId);

        return wallet is null
          ? new MemberWalletState(memberId, 0, 0)
          : new MemberWalletState(memberId, wallet.Balance, wallet.Reserved);
      })
      .ToArray();
    var state = CaptureState() with { Wallets = walletStates };

    return Reconstitute(family, state);
  }

  private void RequireMemberCanLeave(FamilyMemberId memberId)
  {
    var wallet = _wallets.Find(item => item.Id == memberId);
    Rules.Require(
      wallet is null || wallet.Balance == 0 && wallet.Reserved == 0,
      DomainError.InvalidState,
      "A family member with wallet funds or reservations cannot be removed."
    );
    Rules.Require(
      !HasActiveContributionReference(memberId)
        && !HasActiveSavingsReference(memberId)
        && !HasPendingGiftReference(memberId)
        && !HasPendingRedemptionReference(memberId),
      DomainError.InvalidState,
      "A family member with active economy references cannot be removed."
    );
  }

  private bool HasActiveContributionReference(FamilyMemberId memberId)
  {
    return _contributions.Any(item =>
      item.Status != ContributionStatus.Approved
        && (item.AssigneeId == memberId || item.InvitedPartners.Contains(memberId))
    );
  }

  private bool HasActiveSavingsReference(FamilyMemberId memberId)
  {
    return _goals
      .Where(item => !GoalIsTerminal(item))
      .Any(item =>
        item.Owner.MemberId == memberId
          || item.Participants.Contains(memberId)
          || item.RefundableContributions.ContainsKey(memberId)
          || item.CancellationRequest?.RequestedByMemberId == memberId
          || item.CancellationRequest?.ConsentedMemberIds.Contains(memberId) == true
      );
  }

  private bool HasPendingGiftReference(FamilyMemberId memberId)
  {
    return _guardianGifts.Any(item =>
      item.OwnerId == memberId || item.GuardianId == memberId
    );
  }

  private bool HasPendingRedemptionReference(FamilyMemberId memberId)
  {
    return _requests.Any(item =>
      item.Status == RedemptionStatus.Pending && item.RequesterId == memberId
    );
  }

  private bool GoalIsTerminal(SavingGoal goal)
  {
    return goal.Cancelled
      || _savingsVouchers.Any(item => item.Id == goal.Id && item.RedeemedAt is not null);
  }
}
