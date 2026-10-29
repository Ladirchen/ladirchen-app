using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed partial class FamilyEconomy
{
  internal void RestoreWallet(FamilyMemberId memberId, decimal balance, decimal reserved)
  {
    ActiveMember(memberId);
    Rules.Require(balance >= 0, DomainError.InvalidValue, "A restored wallet balance cannot be negative.");
    Rules.Require(reserved >= 0 && reserved <= balance, DomainError.InvalidValue, "A restored reservation must be covered by the wallet balance.");
    var wallet = _wallets.Find(item => item.Id == memberId) ?? new MemberWallet(memberId);

    if (!_wallets.Contains(wallet))
    {
      _wallets.Add(wallet);
    }

    wallet.Set(balance, reserved);
  }

  internal void RestoreContribution(
    ContributionId id,
    ContributionDefinition definition,
    ContributionStatus status,
    StarRating? rating,
    ContributionRewardBreakdown? earnedReward,
    DateTimeOffset? approvedAt,
    IReadOnlyCollection<FamilyMemberId> invitedPartners,
    bool requireActiveReferences = true
  )
  {
    ArgumentNullException.ThrowIfNull(id);
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(invitedPartners);
    Rules.Require(
      _usedContributionIds.Add(id),
      DomainError.Duplicate,
      "This contribution identity was already restored."
    );
    var contribution = new Contribution(
      id,
      definition.Title,
      definition.Kind,
      definition.Reward,
      definition.Energy,
      definition.AssigneeId
    );

    if (requireActiveReferences && definition.AssigneeId is not null)
    {
      ActiveMember(definition.AssigneeId);
    }

    foreach (var partner in invitedPartners)
    {
      if (requireActiveReferences)
      {
        ActiveMember(partner);
      }

      Rules.Require(
        partner != definition.AssigneeId,
        DomainError.InvalidValue,
        "The assignee cannot also be a restored partner."
      );
    }

    contribution.SetPartners([.. invitedPartners]);

    switch (status)
    {
      case ContributionStatus.Available:
        break;
      case ContributionStatus.Pending:
        contribution.Submit();
        break;
      case ContributionStatus.Approved:
        Rules.Require(
          rating is not null && earnedReward is not null && approvedAt is not null,
          DomainError.InvalidValue,
          "An approved contribution requires its rating, reward and approval time."
        );
        contribution.Approve(rating!, earnedReward!, approvedAt!.Value);
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(status));
    }

    _contributions.Add(contribution);
  }

  internal void RestoreUsedContributionId(ContributionId id)
  {
    ArgumentNullException.ThrowIfNull(id);
    _usedContributionIds.Add(id);
  }

  internal void RestoreAppliedOperation(TransactionId operationId)
  {
    ArgumentNullException.ThrowIfNull(operationId);
    Rules.Require(
      _operations.Add(operationId),
      DomainError.Duplicate,
      "This applied operation was already restored."
    );
  }

  internal void RestoreLedgerEntry(LedgerEntry entry)
  {
    ArgumentNullException.ThrowIfNull(entry);
    Rules.Require(
      _ledger.All(item => item.EntryId != entry.EntryId),
      DomainError.Duplicate,
      "This ledger entry was already restored."
    );
    _operations.Add(entry.OperationId);
    _ledger.Add(entry);
  }

  internal void RestorePromotion(
    PromotionId id,
    ContributionId contributionId,
    RewardMultiplier multiplier,
    LadirchenAmount teamworkBonus,
    PromotionDeadline deadline,
    bool active
  )
  {
    ArgumentNullException.ThrowIfNull(id);
    FindContribution(contributionId);
    Rules.Require(_promotions.TrueForAll(item => item.Id != id), DomainError.Duplicate, "The promotion already exists.");
    var promotion = new Promotion(id, contributionId, multiplier, teamworkBonus, deadline);
    promotion.SetActive(active);
    _promotions.Add(promotion);
  }
}
