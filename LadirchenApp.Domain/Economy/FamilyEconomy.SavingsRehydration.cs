using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed partial class FamilyEconomy
{
  public IReadOnlyList<SavingGoal> SavingsGoals
  {
    get
    {
      return _goals.AsReadOnly();
    }
  }

  public IReadOnlySet<IsoWeekPeriod> SettledInterestPeriods
  {
    get
    {
      return _settledInterestPeriods;
    }
  }

  public void RestoreSavingGoal(
    SavingGoalId id,
    SavingGoalDefinition definition,
    DateTimeOffset createdAt,
    decimal saved,
    decimal starterBonus,
    decimal interestEarned,
    IReadOnlyDictionary<FamilyMemberId, decimal> refundableContributions,
    FamilyMemberId? cancellationRequestedByMemberId,
    IReadOnlyCollection<FamilyMemberId> cancellationConsentedMemberIds,
    bool cheered,
    bool cancelled,
    IsoWeekPeriod? lastInterestPeriod,
    Percentage? lastInterestRate
  )
  {
    ArgumentNullException.ThrowIfNull(id);
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(refundableContributions);
    ArgumentNullException.ThrowIfNull(cancellationConsentedMemberIds);
    Rules.Require(_goals.All(item => item.Id != id), DomainError.Duplicate, "This savings goal was already restored.");
    var owner = definition.Owner;

    if (!owner.IsFamily)
    {
      ActiveMember(owner.MemberId!);
    }

    var participants = definition.ParticipantIds?.Distinct().ToArray() ?? Array.Empty<FamilyMemberId>();
    Rules.Require(
      !owner.IsFamily || participants.Length > 0,
      DomainError.InvalidValue,
      "A restored family savings goal requires at least one participant."
    );
    Rules.Require(
      owner.IsFamily || participants.Length == 0,
      DomainError.InvalidValue,
      "A restored personal savings goal cannot define family participants."
    );

    foreach (var participant in participants)
    {
      ActiveMember(participant);
    }

    foreach (var contributor in refundableContributions.Keys)
    {
      ActiveMember(contributor);
    }

    var goal = new SavingGoal(
      id,
      owner,
      definition.Title,
      definition.Target,
      definition.Visibility,
      createdAt,
      participants
    );
    goal.Restore(
      saved,
      starterBonus,
      interestEarned,
      refundableContributions,
      cheered,
      cancelled,
      lastInterestPeriod,
      lastInterestRate
    );

    if (cancellationRequestedByMemberId is null)
    {
      Rules.Require(
        cancellationConsentedMemberIds.Count == 0,
        DomainError.InvalidValue,
        "Cancellation consents require a cancellation requester."
      );
    }
    else
    {
      goal.RestoreCancellationRequest(
        cancellationRequestedByMemberId,
        cancellationConsentedMemberIds
      );
    }

    _goals.Add(goal);
  }

  public void RestoreSavingsVoucher(
    SavingGoalId goalId,
    decimal amount,
    FamilyMemberId? ownerId,
    IReadOnlyCollection<FamilyMemberId> participants,
    IReadOnlyCollection<FamilyMemberId> consents,
    DateTimeOffset? redeemedAt
  )
  {
    ArgumentNullException.ThrowIfNull(goalId);
    ArgumentNullException.ThrowIfNull(participants);
    ArgumentNullException.ThrowIfNull(consents);
    var goal = FindGoal(goalId);
    Rules.Require(goal.Completed, DomainError.InvalidState, "Only a completed savings goal can have a voucher.");
    Rules.Require(amount == goal.Saved, DomainError.InvalidValue, "The voucher must contain the full goal balance.");
    Rules.Require(ownerId == goal.Owner.MemberId, DomainError.InvalidValue, "The voucher owner must match the goal owner.");
    Rules.Require(
      participants.ToHashSet().SetEquals(goal.Participants),
      DomainError.InvalidValue,
      "The voucher participants must match the family goal participants."
    );
    Rules.Require(
      _savingsVouchers.All(item => item.Id != goalId),
      DomainError.Duplicate,
      "This savings voucher was already restored."
    );
    var voucher = new GoalSavingsVoucher(goalId, amount, ownerId, participants);
    voucher.Restore(consents, redeemedAt);
    _savingsVouchers.Add(voucher);
  }

  public void RestorePendingGuardianGift(
    GuardianGiftId id,
    FamilyMemberId recipientId,
    FamilyMemberId? senderId,
    decimal amount,
    SavingGoalId? goalId,
    GiftDestination destination
  )
  {
    ArgumentNullException.ThrowIfNull(id);
    ActiveMember(recipientId);

    if (senderId is not null)
    {
      ActiveMember(senderId);
    }

    if (goalId is not null)
    {
      _ = FindGoal(goalId);
    }

    Rules.Require(
      _guardianGifts.All(item => item.Id != id),
      DomainError.Duplicate,
      "This pending gift was already restored."
    );
    _guardianGifts.Add(new GuardianGoalGift(id, recipientId, senderId, amount, goalId, destination));
  }

  public void RestoreSettledInterestPeriod(IsoWeekPeriod period)
  {
    ArgumentNullException.ThrowIfNull(period);
    Rules.Require(
      _settledInterestPeriods.Add(period),
      DomainError.Duplicate,
      "This interest period was already restored."
    );
  }
}
