using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Family.Permissions;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

internal static class SavingsReadModelProjector
{
  public static SavingsReadModelProjection Project(SavingsReadModelState state)
  {
    ArgumentNullException.ThrowIfNull(state);
    var activeMembers = state.Family.Members.Where(member => !member.InvitationPending).ToArray();
    var visibleGoalsByMember = activeMembers.ToDictionary(
      member => member.Id,
      member => state.Economy.GoalsFor(member.Id).Select(goal => goal.Id).ToHashSet()
    );
    var vouchers = state.Economy.SavingsVouchers.ToDictionary(voucher => voucher.Id);
    var availableBalances = state.Economy.Wallets
      .Where(wallet => state.StoredBalanceMemberIds.Contains(wallet.Id))
      .ToDictionary(wallet => wallet.Id, wallet => wallet.Available);
    var goalProjections = state.Economy.SavingsGoals.ToDictionary(
      goal => goal.Id,
      goal => ProjectGoal(state, goal, vouchers.ContainsKey(goal.Id), activeMembers, visibleGoalsByMember)
    );
    var voucherProjections = vouchers.ToDictionary(
      item => item.Key,
      item => ProjectVoucher(item.Value)
    );
    var sharedSavedBalances = state.Economy.SavingsGoals
      .Where(goal => goal.Owner.MemberId is not null)
      .Where(goal => goal.Visibility != GoalVisibility.Private)
      .Where(goal => activeMembers.Any(member => member.Id == goal.Owner.MemberId))
      .GroupBy(goal => goal.Owner.MemberId!)
      .ToDictionary(group => group.Key, group => group.Sum(goal => goal.Saved));

    return new SavingsReadModelProjection(
      availableBalances,
      goalProjections,
      voucherProjections,
      sharedSavedBalances
    );
  }

  private static SavingsGoalReadProjection ProjectGoal(
    SavingsReadModelState state,
    SavingGoal goal,
    bool hasVoucher,
    IReadOnlyList<FamilyMember> activeMembers,
    IReadOnlyDictionary<FamilyMemberId, HashSet<SavingGoalId>> visibleGoalsByMember
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
    var withdrawableBalance = hasVoucher ? 0 : goal.Withdrawable;
    var visibleToMemberIds = activeMembers
      .Where(member => visibleGoalsByMember[member.Id].Contains(goal.Id))
      .Select(member => member.Id)
      .ToArray();
    var commandLimits = activeMembers.ToDictionary(
      member => member.Id,
      member => ProjectCommandLimits(
        state.Economy,
        goal,
        member,
        hasVoucher,
        remainingAmount,
        withdrawableBalance,
        visibleGoalsByMember[member.Id].Contains(goal.Id)
      )
    );

    return new SavingsGoalReadProjection(
      projectedInterestCredit,
      remainingAmount,
      withdrawableBalance,
      visibleToMemberIds,
      commandLimits
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
    var permissions = new FamilyPermissionPolicy().Resolve(member);
    var canCancelFamilyGoal = goal.Owner.IsFamily
      && (member.Role == MemberRole.Child || permissions.ManageFamily);
    var supportsAsGuardian = member.Role == MemberRole.Guardian && isVisible;
    var supportsAsChild = member.Role == MemberRole.Child
      && !ownsGoal
      && goal.Visibility == GoalVisibility.Family;

    return new SavingsGoalCommandLimits(
      !hasVoucher && (ownsGoal || canCancelFamilyGoal),
      !hasVoucher && ownsGoal && withdrawableBalance > 0,
      !hasVoucher && ownsGoal ? Math.Min(availableBalance, remainingAmount) : 0,
      !hasVoucher && supportsAsGuardian
        ? remainingAmount
        : !hasVoucher && supportsAsChild
          ? Math.Min(availableBalance, remainingAmount)
          : 0
    );
  }

  private static SavingsVoucherReadProjection ProjectVoucher(GoalSavingsVoucher voucher)
  {
    var visible = voucher.Participants
      .Append(voucher.OwnerId)
      .Where(memberId => memberId is not null)
      .Select(memberId => memberId!)
      .Distinct()
      .ToArray();
    var consentable = voucher.RedemptionCount == 0 && !voucher.Redeemable
      ? voucher.Participants.Where(memberId => !voucher.Consents.Contains(memberId)).ToArray()
      : [];
    var redeemable = voucher.RedemptionCount == 0 && voucher.Redeemable
      ? visible
      : [];

    return new SavingsVoucherReadProjection(visible, consentable, redeemable);
  }
}
