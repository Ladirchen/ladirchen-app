using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

internal static class SavingsReadModelProjector
{
  public static SavingsReadModelProjection Project(
    SavingsReadModelState state,
    FamilyMemberId viewerId
  )
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(viewerId);
    var viewer = state.Family.ActiveMember(viewerId);
    var activeMembers = state.Family.Members.Where(member => !member.InvitationPending).ToArray();
    var visibleBalanceMemberIds = new HashSet<FamilyMemberId> { viewerId };
    var visibleGoals = state.Economy.GoalsFor(viewerId);
    var allVouchers = state.Economy.SavingsVouchers.ToDictionary(voucher => voucher.Id);
    var balances = state.Economy.Wallets
      .Where(wallet => state.StoredBalanceMemberIds.Contains(wallet.Id))
      .Where(wallet => visibleBalanceMemberIds.Contains(wallet.Id))
      .ToDictionary(wallet => wallet.Id, wallet => wallet.Balance);
    var availableBalances = state.Economy.Wallets
      .Where(wallet => state.StoredBalanceMemberIds.Contains(wallet.Id))
      .Where(wallet => visibleBalanceMemberIds.Contains(wallet.Id))
      .ToDictionary(wallet => wallet.Id, wallet => wallet.Available);
    var goalProjections = visibleGoals.ToDictionary(
      goal => goal.Id,
      goal => ProjectGoal(state, goal, allVouchers.ContainsKey(goal.Id), viewer)
    );
    var voucherProjections = allVouchers.Values
      .Where(voucher => voucher.OwnerId == viewerId || voucher.Participants.Contains(viewerId))
      .ToDictionary(voucher => voucher.Id, voucher => ProjectVoucher(voucher, viewerId));
    var sharedSavedBalances = visibleGoals
      .Where(goal => goal.Owner.MemberId is not null)
      .Where(goal => goal.Visibility != GoalVisibility.Private)
      .Where(goal => activeMembers.Any(member => member.Id == goal.Owner.MemberId))
      .GroupBy(goal => goal.Owner.MemberId!)
      .ToDictionary(group => group.Key, group => group.Sum(goal => goal.Saved));

    return new SavingsReadModelProjection(
      balances,
      availableBalances,
      goalProjections,
      voucherProjections,
      sharedSavedBalances,
      visibleBalanceMemberIds
    );
  }

  private static SavingsGoalReadProjection ProjectGoal(
    SavingsReadModelState state,
    SavingGoal goal,
    bool hasVoucher,
    FamilyMember viewer
  )
  {
    var ownerId = goal.Owner.MemberId;
    var rate = ownerId is not null && state.LadiStages.TryGetValue(ownerId, out var stage)
      ? SavingsInterestPolicy.CalculateRate(stage, state.MaximumInterestRate)
      : new Percentage(0);
    var projectedInterestCredit = SavingsInterestPolicy.CalculateCredit(
      new LadirchenAmount(goal.Saved),
      new LadirchenAmount(goal.Target),
      rate
    );
    var remainingAmount = Math.Max(0, goal.Target - goal.Saved);
    var withdrawableBalance = !hasVoucher && goal.Owner.MemberId == viewer.Id
      ? goal.Withdrawable
      : 0;
    FamilyMemberId[] visibleToMemberIds = [viewer.Id];
    var commandLimits = new Dictionary<FamilyMemberId, SavingsGoalCommandLimits>
    {
      [viewer.Id] = ProjectCommandLimits(
        state.Economy,
        goal,
        viewer,
        hasVoucher,
        remainingAmount,
        withdrawableBalance,
        true
      ),
    };
    var cancellationRequest = goal.CancellationRequest is null
      ? null
      : new SavingsGoalCancellationRequestReadProjection(
        goal.CancellationRequest.RequestedByMemberId,
        goal.CancellationRequest.ConsentedMemberIds
          .OrderBy(memberId => memberId.Value, StringComparer.Ordinal)
          .ToArray(),
        goal.Participants
          .Where(memberId => !goal.CancellationRequest.ConsentedMemberIds.Contains(memberId))
          .OrderBy(memberId => memberId.Value, StringComparer.Ordinal)
          .ToArray()
      );

    return new SavingsGoalReadProjection(
      projectedInterestCredit,
      remainingAmount,
      withdrawableBalance,
      visibleToMemberIds,
      commandLimits,
      cancellationRequest
    );
  }

  private static SavingsGoalCommandLimits ProjectCommandLimits(
    FamilyEconomy economy,
    SavingGoal goal,
    FamilyMember member,
    bool hasVoucher,
    decimal remainingAmount,
    decimal withdrawableBalance,
    bool isVisible
  )
  {
    var availableBalance = economy.WalletFor(member.Id).Available;
    var ownsGoal = goal.Owner.MemberId == member.Id;
    var isCancellationParticipant = goal.Participants.Contains(member.Id);
    var cancellationPending = goal.CancellationRequest is not null;
    var canDecideCancellation = cancellationPending
      && isCancellationParticipant
      && !goal.CancellationRequest!.ConsentedMemberIds.Contains(member.Id);
    var supportsAsGuardian = member.Role == MemberRole.Guardian && isVisible;
    var supportsAsChild = member.Role == MemberRole.Child
      && !ownsGoal
      && goal.Visibility == GoalVisibility.Family;

    return new SavingsGoalCommandLimits(
      !hasVoucher && ownsGoal && !goal.Owner.IsFamily,
      !hasVoucher && goal.Owner.IsFamily && isCancellationParticipant && !cancellationPending,
      !hasVoucher && canDecideCancellation,
      !hasVoucher && canDecideCancellation,
      !hasVoucher && !cancellationPending && ownsGoal && withdrawableBalance > 0,
      !hasVoucher && !cancellationPending && ownsGoal ? Math.Min(availableBalance, remainingAmount) : 0,
      !hasVoucher && !cancellationPending && supportsAsGuardian
        ? remainingAmount
        : !hasVoucher && !cancellationPending && supportsAsChild
          ? Math.Min(availableBalance, remainingAmount)
          : 0
    );
  }

  private static SavingsVoucherReadProjection ProjectVoucher(
    GoalSavingsVoucher voucher,
    FamilyMemberId viewerId
  )
  {
    FamilyMemberId[] visible = [viewerId];
    var consentable = voucher.RedemptionCount == 0 && !voucher.Redeemable
      && voucher.Participants.Contains(viewerId)
      && !voucher.Consents.Contains(viewerId)
      ? new[] { viewerId }
      : [];
    var redeemable = voucher.RedemptionCount == 0 && voucher.Redeemable
      && (voucher.OwnerId == viewerId || voucher.Participants.Contains(viewerId))
      ? new[] { viewerId }
      : [];

    return new SavingsVoucherReadProjection(visible, consentable, redeemable);
  }
}
