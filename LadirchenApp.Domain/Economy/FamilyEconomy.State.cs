using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Economy;

public sealed partial class FamilyEconomy
{
  private FamilyEconomy(FamilyAggregate family, FamilyEconomyState state)
    : base(family?.Id ?? throw new ArgumentNullException(nameof(family)))
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(state.ContributionSettings);
    ArgumentNullException.ThrowIfNull(state.Wallets);
    ArgumentNullException.ThrowIfNull(state.Contributions);
    ArgumentNullException.ThrowIfNull(state.UsedContributionIds);
    ArgumentNullException.ThrowIfNull(state.Promotions);
    ArgumentNullException.ThrowIfNull(state.AppliedOperationIds);
    ArgumentNullException.ThrowIfNull(state.Ledger);
    ArgumentNullException.ThrowIfNull(state.SavingGoals);
    ArgumentNullException.ThrowIfNull(state.SavingsVouchers);
    ArgumentNullException.ThrowIfNull(state.GuardianGifts);
    ArgumentNullException.ThrowIfNull(state.SettledInterestPeriods);
    ArgumentNullException.ThrowIfNull(state.ShopRewards);
    ArgumentNullException.ThrowIfNull(state.RedemptionRequests);
    Rules.Require(state.Id == family.Id, DomainError.InvalidState, "The economy must belong to the supplied family.");

    _timeZone = family.TimeZone;
    _members = SnapshotMembers(family);
    _wallets = _members
      .Where(item => !item.InvitationPending)
      .Select(item => new MemberWallet(item.Id))
      .ToList();
    _contributionSettings = state.ContributionSettings;
    ReconstituteWallets(state.Wallets);
    ReconstituteContributions(state.Contributions, state.UsedContributionIds);
    ReconstitutePromotions(state.Promotions);
    ReconstituteSavingGoals(state.SavingGoals, state.SavingsVouchers);
    ReconstituteSavingsVouchers(state.SavingsVouchers);
    ReconstituteGuardianGifts(state.GuardianGifts);
    ReconstituteSettledInterestPeriods(state.SettledInterestPeriods);
    ValidateGoalInterestHistory();
    ReconstituteShopRewards(state.ShopRewards);
    ReconstituteRedemptionRequests(state.RedemptionRequests);
    ValidateShopReservations();
    ReconstituteOperations(state.AppliedOperationIds);
    ReconstituteLedger(state.Ledger);
  }

  internal static FamilyEconomy Reconstitute(FamilyAggregate family, FamilyEconomyState state)
  {
    ArgumentNullException.ThrowIfNull(family);
    ArgumentNullException.ThrowIfNull(state);

    return new FamilyEconomy(family, state);
  }

  internal FamilyEconomyState CaptureState()
  {
    return new FamilyEconomyState(
      Id,
      _contributionSettings,
      _wallets.Select(item => new MemberWalletState(item.Id, item.Balance, item.Reserved)).ToArray(),
      _contributions.Select(CaptureContribution).ToArray(),
      _usedContributionIds.OrderBy(item => item.Value, StringComparer.Ordinal).ToArray(),
      _promotions.Select(CapturePromotion).ToArray(),
      _operations.OrderBy(item => item.Value, StringComparer.Ordinal).ToArray(),
      _ledger.Select(CaptureLedgerEntry).ToArray(),
      _goals.Select(CaptureSavingGoal).ToArray(),
      _savingsVouchers.Select(CaptureSavingsVoucher).ToArray(),
      _guardianGifts.Select(CaptureGuardianGift).ToArray(),
      _settledInterestPeriods.OrderBy(item => item.Start).ToArray(),
      _shopRewards.Select(CaptureShopReward).ToArray(),
      _requests.Select(CaptureRedemptionRequest).ToArray()
    );
  }

  private static IReadOnlyList<FamilyEconomyMember> SnapshotMembers(FamilyAggregate family)
  {
    var members = family.Members.Select(item => new FamilyEconomyMember(
      item.Id,
      item.Role,
      item.Access,
      item.InvitationPending
    )).ToArray();
    Rules.Require(
      members.Any(item => item.IsAdministrator),
      DomainError.InvalidState,
      "A family economy requires an active administrator."
    );

    return members;
  }

  private void ReconstituteWallets(IReadOnlyList<MemberWalletState> states)
  {
    Rules.Require(states.All(item => item is not null), DomainError.InvalidState, "Wallets cannot be null.");
    Rules.Require(
      states.Select(item => item.MemberId).Distinct().Count() == states.Count,
      DomainError.InvalidState,
      "Wallet member identifiers must be unique."
    );
    var expectedMemberIds = _members
      .Where(item => !item.InvitationPending)
      .Select(item => item.Id)
      .ToHashSet();
    Rules.Require(
      states.All(item => expectedMemberIds.Contains(item.MemberId)),
      DomainError.InvalidState,
      "A stored wallet must belong to an active family member."
    );

    foreach (var state in states)
    {
      RestoreWallet(state.MemberId, state.Balance, state.Reserved);
    }
  }

  private void ReconstituteContributions(
    IReadOnlyList<ContributionState> states,
    IReadOnlyList<ContributionId> usedIds
  )
  {
    Rules.Require(states.All(item => item is not null), DomainError.InvalidState, "Contributions cannot be null.");
    Rules.Require(
      states.Select(item => item.Id).Distinct().Count() == states.Count,
      DomainError.InvalidState,
      "Contribution identifiers must be unique."
    );
    Rules.Require(
      usedIds.All(item => item is not null),
      DomainError.InvalidState,
      "Used contribution IDs cannot be null."
    );
    Rules.Require(
      usedIds.Distinct().Count() == usedIds.Count,
      DomainError.InvalidState,
      "Used contribution identifiers must be unique."
    );
    var usedIdSet = usedIds.ToHashSet();
    Rules.Require(
      states.All(item => usedIdSet.Contains(item.Id)),
      DomainError.InvalidState,
      "Every stored contribution identity must be marked as used."
    );

    foreach (var state in states)
    {
      ArgumentNullException.ThrowIfNull(state.InvitedPartnerIds);
      Rules.Require(
        state.InvitedPartnerIds.Distinct().Count() == state.InvitedPartnerIds.Count,
        DomainError.InvalidState,
        "Contribution partner identifiers must be unique."
      );
      Rules.Require(
        state.InvitedPartnerIds.Count == 0 || state.Kind == ContributionKind.Extra,
        DomainError.InvalidState,
        "Only an extra contribution can retain invited partners."
      );
      Rules.Require(
        state.Status == ContributionStatus.Available || state.AssigneeId is not null,
        DomainError.InvalidState,
        "A submitted or approved contribution requires an assignee."
      );
      var approved = state.Status == ContributionStatus.Approved;
      Rules.Require(
        approved
          ? state.Rating is not null && state.EarnedReward is not null && state.ApprovedAt is not null
          : state.Rating is null && state.EarnedReward is null && state.ApprovedAt is null,
        DomainError.InvalidState,
        "Contribution approval details must match its status."
      );
      RestoreContribution(
        state.Id,
        new ContributionDefinition(state.Title, state.Kind, state.Reward, state.Energy, state.AssigneeId),
        state.Status,
        state.Rating,
        state.EarnedReward,
        state.ApprovedAt,
        state.InvitedPartnerIds,
        state.Status != ContributionStatus.Approved
      );
    }

    foreach (var usedId in usedIds.Where(item => !_usedContributionIds.Contains(item)))
    {
      RestoreUsedContributionId(usedId);
    }
  }

  private void ReconstitutePromotions(IReadOnlyList<PromotionState> states)
  {
    Rules.Require(states.All(item => item is not null), DomainError.InvalidState, "Promotions cannot be null.");
    Rules.Require(
      states.Select(item => item.Id).Distinct().Count() == states.Count,
      DomainError.InvalidState,
      "Promotion identifiers must be unique."
    );

    foreach (var state in states)
    {
      RestorePromotion(
        state.Id,
        state.ContributionId,
        state.Multiplier,
        state.TeamworkBonus,
        state.Deadline,
        state.Active
      );
    }
  }

  private void ReconstituteSavingGoals(
    IReadOnlyList<SavingGoalState> states,
    IReadOnlyList<SavingsVoucherState> voucherStates
  )
  {
    Rules.Require(states.All(item => item is not null), DomainError.InvalidState, "Savings goals cannot be null.");
    Rules.Require(
      voucherStates.All(item => item is not null),
      DomainError.InvalidState,
      "Savings vouchers cannot be null."
    );
    Rules.Require(
      states.Select(item => item.Id).Distinct().Count() == states.Count,
      DomainError.InvalidState,
      "Savings-goal identifiers must be unique."
    );

    var redeemedGoalIds = voucherStates
      .Where(item => item.RedeemedAt is not null)
      .Select(item => item.GoalId)
      .ToHashSet();

    foreach (var state in states)
    {
      ArgumentNullException.ThrowIfNull(state.ParticipantIds);
      ArgumentNullException.ThrowIfNull(state.RefundableContributions);
      Rules.Require(
        state.ParticipantIds.Distinct().Count() == state.ParticipantIds.Count,
        DomainError.InvalidState,
        "Savings-goal participant identifiers must be unique."
      );
      Rules.Require(
        state.OwnerId is not null || state.Visibility == GoalVisibility.Family,
        DomainError.InvalidState,
        "A family savings goal must use family visibility."
      );
      Rules.Require(
        (state.LastInterestPeriod is null) == (state.LastInterestRate is null),
        DomainError.InvalidState,
        "A savings goal must store its last interest period and rate together."
      );
      Rules.Require(
        !state.Cancelled || state.CancellationRequest is null,
        DomainError.InvalidState,
        "A cancelled savings goal cannot retain a cancellation request."
      );
      Rules.Require(
        state.RefundableContributions.All(item => item is not null),
        DomainError.InvalidState,
        "Refundable contributions cannot be null."
      );
      Rules.Require(
        state.RefundableContributions.Select(item => item.MemberId).Distinct().Count()
          == state.RefundableContributions.Count,
        DomainError.InvalidState,
        "Refundable contributors must be unique."
      );
      var refundableContributions = state.RefundableContributions.ToDictionary(
        item => item.MemberId,
        item => item.Amount
      );
      var cancellationRequest = state.CancellationRequest;

      if (cancellationRequest is not null)
      {
        ArgumentNullException.ThrowIfNull(cancellationRequest.ConsentedMemberIds);
      }

      RestoreSavingGoal(
        state.Id,
        new SavingGoalDefinition(
          state.OwnerId is null ? GoalOwner.Family : GoalOwner.Member(state.OwnerId),
          state.Title,
          new LadirchenAmount(state.Target),
          state.Visibility,
          state.ParticipantIds
        ),
        state.CreatedAt,
        state.Saved,
        state.StarterBonus,
        state.InterestEarned,
        refundableContributions,
        cancellationRequest?.RequestedByMemberId,
        cancellationRequest?.ConsentedMemberIds ?? Array.Empty<FamilyMemberId>(),
        state.Cheered,
        state.Cancelled,
        state.LastInterestPeriod,
        state.LastInterestRate,
        !state.Cancelled && !redeemedGoalIds.Contains(state.Id)
      );
    }
  }

  private void ReconstituteSavingsVouchers(IReadOnlyList<SavingsVoucherState> states)
  {
    Rules.Require(states.All(item => item is not null), DomainError.InvalidState, "Savings vouchers cannot be null.");
    Rules.Require(
      states.Select(item => item.GoalId).Distinct().Count() == states.Count,
      DomainError.InvalidState,
      "A savings goal can have only one voucher."
    );

    foreach (var state in states)
    {
      ArgumentNullException.ThrowIfNull(state.ParticipantIds);
      ArgumentNullException.ThrowIfNull(state.ConsentedMemberIds);
      RestoreSavingsVoucher(
        state.GoalId,
        state.Amount,
        state.OwnerId,
        state.ParticipantIds,
        state.ConsentedMemberIds,
        state.RedeemedAt
      );
    }
  }

  private void ReconstituteGuardianGifts(IReadOnlyList<GuardianGoalGiftState> states)
  {
    Rules.Require(states.All(item => item is not null), DomainError.InvalidState, "Guardian gifts cannot be null.");
    var identifiedGifts = states.Where(item => item.Id is not null).ToArray();
    Rules.Require(
      identifiedGifts.Select(item => item.Id).Distinct().Count() == identifiedGifts.Length,
      DomainError.InvalidState,
      "Guardian gift identifiers must be unique."
    );

    foreach (var state in states)
    {
      RequireChild(state.OwnerId);

      if (state.GuardianId is not null)
      {
        ActiveMember(state.GuardianId);
      }

      if (state.GoalId is not null)
      {
        var goal = _goals.SingleOrDefault(item => item.Id == state.GoalId && !item.Cancelled);
        Rules.Require(
          goal is not null && goal.Owner.MemberId == state.OwnerId,
          DomainError.InvalidState,
          "A goal gift requires the recipient's active personal savings goal."
        );
      }

      Rules.Require(
        state.Destination != GiftDestination.Balance || state.GoalId is null,
        DomainError.InvalidState,
        "A balance gift cannot reference a savings goal."
      );

      _guardianGifts.Add(new GuardianGoalGift(
        state.Id,
        state.OwnerId,
        state.GuardianId,
        state.Amount,
        state.GoalId,
        state.Destination
      ));
    }
  }

  private void ReconstituteSettledInterestPeriods(IReadOnlyList<IsoWeekPeriod> states)
  {
    Rules.Require(states.All(item => item is not null), DomainError.InvalidState, "Interest periods cannot be null.");

    foreach (var period in states)
    {
      RestoreSettledInterestPeriod(period);
    }
  }

  private void ValidateGoalInterestHistory()
  {
    Rules.Require(
      _goals.All(item => item.LastInterestPeriod is null || _settledInterestPeriods.Contains(item.LastInterestPeriod)),
      DomainError.InvalidState,
      "A goal's last interest period must be a settled family period."
    );
  }

  private void ReconstituteShopRewards(IReadOnlyList<ShopRewardState> states)
  {
    Rules.Require(states.All(item => item is not null), DomainError.InvalidState, "Shop rewards cannot be null.");
    Rules.Require(
      states.Select(item => item.Id).Distinct().Count() == states.Count,
      DomainError.InvalidState,
      "Shop reward identifiers must be unique."
    );

    foreach (var state in states)
    {
      var reward = new ShopReward(
        state.Id,
        new ShopRewardDefinition(
          state.Title,
          state.Category,
          state.Price,
          state.Quantity,
          state.IsVisible,
          state.Window
        )
      );
      reward.Restore(state.Status, state.Deleted);
      _shopRewards.Add(reward);
    }
  }

  private void ReconstituteRedemptionRequests(IReadOnlyList<RedemptionRequestState> states)
  {
    Rules.Require(
      states.All(item => item is not null),
      DomainError.InvalidState,
      "Redemption requests cannot be null."
    );
    Rules.Require(
      states.Select(item => item.Id).Distinct().Count() == states.Count,
      DomainError.InvalidState,
      "Redemption request identifiers must be unique."
    );

    foreach (var state in states)
    {
      if (state.Status == RedemptionStatus.Pending)
      {
        RequireChild(state.RequesterId);
      }

      Rules.Require(
        _shopRewards.Any(item => item.Id == state.RewardId),
        DomainError.InvalidState,
        "A redemption request requires its shop reward."
      );
      var request = new RedemptionRequest(
        state.Id,
        state.RewardId,
        state.RequesterId,
        state.Price,
        state.RequestedAt
      );
      request.Restore(state.Status, state.DecidedAt);
      _requests.Add(request);
    }
  }

  private void ValidateShopReservations()
  {
    var pendingRequests = _requests.Where(item => item.Status == RedemptionStatus.Pending).ToArray();
    Rules.Require(
      pendingRequests.Select(item => item.RewardId).Distinct().Count() == pendingRequests.Length,
      DomainError.InvalidState,
      "A shop reward can have only one pending request."
    );

    foreach (var reward in _shopRewards)
    {
      var pendingRequest = pendingRequests.SingleOrDefault(item => item.RewardId == reward.Id);
      Rules.Require(
        reward.Status == ShopRewardStatus.Requested ? pendingRequest is not null : pendingRequest is null,
        DomainError.InvalidState,
        "A requested reward and its pending request must be restored together."
      );

      if (pendingRequest is not null)
      {
        Rules.Require(
          pendingRequest.Price == reward.Price,
          DomainError.InvalidState,
          "A pending request must retain the current reward price."
        );
      }
    }

    foreach (var wallet in _wallets)
    {
      var expectedReservation = pendingRequests
        .Where(item => item.RequesterId == wallet.Id)
        .Sum(item => item.Price.Value);
      Rules.Require(
        wallet.Reserved == expectedReservation,
        DomainError.InvalidState,
        "Wallet reservations must match pending shop requests."
      );
    }
  }

  private void ReconstituteOperations(IReadOnlyList<TransactionId> states)
  {
    Rules.Require(states.All(item => item is not null), DomainError.InvalidState, "Operation IDs cannot be null.");

    foreach (var operationId in states)
    {
      RestoreAppliedOperation(operationId);
    }
  }

  private void ReconstituteLedger(IReadOnlyList<LedgerEntryState> states)
  {
    Rules.Require(states.All(item => item is not null), DomainError.InvalidState, "Ledger entries cannot be null.");
    Rules.Require(
      states.Select(item => item.EntryId).Distinct().Count() == states.Count,
      DomainError.InvalidState,
      "Ledger entry identifiers must be unique."
    );

    foreach (var state in states)
    {
      Rules.Require(
        _operations.Contains(state.OperationId),
        DomainError.InvalidState,
        "Every ledger entry requires a stored operation."
      );
      Rules.Require(state.EntryId != Guid.Empty, DomainError.InvalidState, "A ledger entry ID cannot be empty.");
      ArgumentNullException.ThrowIfNull(state.MemberId);
      ArgumentNullException.ThrowIfNull(state.Source);
      Rules.Defined(state.Kind);
      Rules.Require(state.Amount >= 0, DomainError.InvalidState, "A ledger entry amount cannot be negative.");
      Rules.Require(
        !string.IsNullOrWhiteSpace(state.Reason),
        DomainError.InvalidState,
        "A ledger entry requires a reason."
      );
      var entry = new LedgerEntry(
        state.OperationId,
        state.MemberId,
        state.GoalId,
        state.Kind,
        state.Amount,
        ReconstituteLedgerSource(state.Source),
        state.Reason,
        state.OccurredAtUtc.ToUniversalTime(),
        state.InitiatorId
      )
      {
        EntryId = state.EntryId,
      };
      RestoreLedgerEntry(entry);
    }
  }

  private static ContributionState CaptureContribution(Contribution contribution)
  {
    return new ContributionState(
      contribution.Id,
      contribution.Title,
      contribution.Kind,
      contribution.Reward,
      contribution.Energy,
      contribution.AssigneeId,
      contribution.Status,
      contribution.Rating,
      contribution.EarnedReward,
      contribution.ApprovedAt,
      contribution.InvitedPartners.ToArray()
    );
  }

  private static PromotionState CapturePromotion(Promotion promotion)
  {
    return new PromotionState(
      promotion.Id,
      promotion.ContributionId,
      promotion.Multiplier,
      promotion.TeamworkBonus,
      promotion.Deadline,
      promotion.Active
    );
  }

  private static LedgerEntryState CaptureLedgerEntry(LedgerEntry entry)
  {
    return new LedgerEntryState(
      entry.EntryId,
      entry.OperationId,
      entry.MemberId,
      entry.GoalId,
      entry.Kind,
      entry.Amount,
      CaptureLedgerSource(entry.SourceId),
      entry.Reason,
      entry.OccurredAtUtc,
      entry.InitiatorId
    );
  }

  private static SavingGoalState CaptureSavingGoal(SavingGoal goal)
  {
    var cancellationRequest = goal.CancellationRequest is null
      ? null
      : new GoalCancellationRequestState(
        goal.CancellationRequest.RequestedByMemberId,
        goal.CancellationRequest.ConsentedMemberIds
          .OrderBy(item => item.Value, StringComparer.Ordinal)
          .ToArray()
      );

    return new SavingGoalState(
      goal.Id,
      goal.Owner.MemberId,
      goal.Title,
      goal.Target,
      goal.Visibility,
      goal.CreatedAt,
      goal.Participants.OrderBy(item => item.Value, StringComparer.Ordinal).ToArray(),
      goal.Saved,
      goal.StarterBonus,
      goal.InterestEarned,
      goal.RefundableContributions
        .OrderBy(item => item.Key.Value, StringComparer.Ordinal)
        .Select(item => new RefundableContributionState(item.Key, item.Value))
        .ToArray(),
      cancellationRequest,
      goal.Cheered,
      goal.Cancelled,
      goal.LastInterestPeriod,
      goal.LastInterestRate
    );
  }

  private static SavingsVoucherState CaptureSavingsVoucher(GoalSavingsVoucher voucher)
  {
    return new SavingsVoucherState(
      voucher.Id,
      voucher.Amount,
      voucher.OwnerId,
      voucher.Participants.OrderBy(item => item.Value, StringComparer.Ordinal).ToArray(),
      voucher.Consents.OrderBy(item => item.Value, StringComparer.Ordinal).ToArray(),
      voucher.RedeemedAt
    );
  }

  private static GuardianGoalGiftState CaptureGuardianGift(GuardianGoalGift gift)
  {
    return new GuardianGoalGiftState(
      gift.Id,
      gift.OwnerId,
      gift.GuardianId,
      gift.Amount,
      gift.GoalId,
      gift.Destination
    );
  }

  private static ShopRewardState CaptureShopReward(ShopReward reward)
  {
    return new ShopRewardState(
      reward.Id,
      reward.Title,
      reward.Category,
      reward.Price,
      reward.Quantity,
      reward.IsVisible,
      reward.Window,
      reward.Status,
      reward.Deleted
    );
  }

  private static RedemptionRequestState CaptureRedemptionRequest(RedemptionRequest request)
  {
    return new RedemptionRequestState(
      request.Id,
      request.RewardId,
      request.RequesterId,
      request.Price,
      request.RequestedAt,
      request.Status,
      request.DecidedAt
    );
  }

  private static LedgerSourceState CaptureLedgerSource(DomainIdentifier source)
  {
    return source switch
    {
      ContributionId id => new LedgerSourceState(LedgerSourceKind.Contribution, id.Value),
      FamilyMemberId id => new LedgerSourceState(LedgerSourceKind.FamilyMember, id.Value),
      HouseContentId id => new LedgerSourceState(LedgerSourceKind.HouseContent, id.Value),
      RedemptionRequestId id => new LedgerSourceState(LedgerSourceKind.RedemptionRequest, id.Value),
      SavingGoalId id => new LedgerSourceState(LedgerSourceKind.SavingGoal, id.Value),
      _ => throw new DomainRuleException(DomainError.InvalidState, "The ledger source type is unsupported."),
    };
  }

  private static DomainIdentifier ReconstituteLedgerSource(LedgerSourceState source)
  {
    return source.Kind switch
    {
      LedgerSourceKind.Contribution => new ContributionId(source.Id),
      LedgerSourceKind.FamilyMember => new FamilyMemberId(source.Id),
      LedgerSourceKind.HouseContent => new HouseContentId(source.Id),
      LedgerSourceKind.RedemptionRequest => new RedemptionRequestId(source.Id),
      LedgerSourceKind.SavingGoal => new SavingGoalId(source.Id),
      _ => throw new DomainRuleException(DomainError.InvalidState, "The ledger source type is invalid."),
    };
  }
}
