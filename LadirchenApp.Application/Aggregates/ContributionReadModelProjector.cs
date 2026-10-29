using System;
using System.Linq;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

internal static class ContributionReadModelProjector
{
  public static ContributionReadModelProjection Project(
    ContributionReadModelState state,
    DateTimeOffset now
  )
  {
    ArgumentNullException.ThrowIfNull(state);
    var activeMembers = state.Family.Members.Where(member => !member.InvitationPending).ToArray();
    var promotionAvailability = state.Economy.Promotions.ToDictionary(
      promotion => promotion.Id,
      promotion => promotion.Active && promotion.Deadline.IsOpen(now, state.Family.TimeZone)
    );
    var contributions = state.Economy.Contributions.ToDictionary(
      contribution => contribution.Id,
      contribution =>
      {
        var promotion = state.Economy.Promotions.FirstOrDefault(item =>
          item.ContributionId == contribution.Id && promotionAvailability[item.Id]
        );
        var projectedReward = ContributionRewardPolicy.Calculate(
          contribution.Reward,
          new StarRating(1),
          state.Economy.ContributionSettings.PerfectRatingBonus,
          promotion?.Multiplier,
          promotion?.TeamworkBonus
        ).BaseReward;
        var partnerableMemberIds = contribution.Kind == ContributionKind.Extra
          && contribution.Status == ContributionStatus.Available
            ? activeMembers
              .Where(member => member.Id != contribution.AssigneeId)
              .Select(member => member.Id)
              .ToArray()
            : [];

        return new ContributionReadProjection(projectedReward, partnerableMemberIds);
      }
    );
    var approvedContributions = state.Economy.Contributions
      .Where(contribution => contribution.Status == ContributionStatus.Approved)
      .Where(contribution => contribution.AssigneeId is not null)
      .Where(contribution => contribution.ApprovedAt is not null)
      .ToArray();
    var localToday = state.Family.TimeZone.LocalDate(now);
    var todayEarnedByMember = approvedContributions
      .Where(contribution => state.Family.TimeZone.LocalDate(contribution.ApprovedAt!.Value) == localToday)
      .GroupBy(contribution => contribution.AssigneeId!)
      .ToDictionary(
        group => group.Key,
        group => group.Sum(contribution => contribution.EarnedReward?.Total ?? contribution.Reward.Value)
      );
    var currentWeek = IsoWeekPeriod.For(localToday);
    var currentWeekContributions = approvedContributions
      .Where(contribution => currentWeek.Contains(state.Family.TimeZone.LocalDate(contribution.ApprovedAt!.Value)))
      .ToArray();
    var currentWeekEarnedByMember = currentWeekContributions
      .GroupBy(contribution => contribution.AssigneeId!)
      .ToDictionary(
        group => group.Key,
        group => group.Sum(contribution => contribution.EarnedReward?.Total ?? contribution.Reward.Value)
      );
    var currentWeekApprovedContributionCountByMember = currentWeekContributions
      .GroupBy(contribution => contribution.AssigneeId!)
      .ToDictionary(group => group.Key, group => group.Count());

    return new ContributionReadModelProjection(
      state.Economy.ContributionSettings.PerfectRatingBonus.Value,
      contributions,
      promotionAvailability,
      todayEarnedByMember,
      currentWeekEarnedByMember,
      currentWeekApprovedContributionCountByMember
    );
  }
}
