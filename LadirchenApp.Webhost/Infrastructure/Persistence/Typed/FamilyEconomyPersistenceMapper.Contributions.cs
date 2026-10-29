using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

internal static partial class FamilyEconomyPersistenceMapper
{
  private static IReadOnlyList<ContributionState> ToContributionStates(
    FamilyEconomyRootEntity entity
  )
  {
    return entity.Contributions
      .OrderBy(item => item.ContributionId, StringComparer.Ordinal)
      .Select(item => new ContributionState(
        new ContributionId(item.ContributionId),
        item.Title,
        (ContributionKind)item.Kind,
        new LadirchenAmount(item.Reward),
        new Percentage(item.EnergyPercent),
        OptionalMemberId(item.AssigneeId),
        (ContributionStatus)item.Status,
        item.Rating is null ? null : new StarRating(item.Rating.Value),
        ToRewardBreakdown(item),
        item.ApprovedAt,
        item.InvitedPartners
          .OrderBy(partner => partner.MemberId, StringComparer.Ordinal)
          .Select(partner => new FamilyMemberId(partner.MemberId))
          .ToArray()
      ))
      .ToArray();
  }

  private static IReadOnlyList<PromotionState> ToPromotionStates(FamilyEconomyRootEntity entity)
  {
    return entity.Promotions
      .OrderBy(item => item.PromotionId, StringComparer.Ordinal)
      .Select(item => new PromotionState(
        new PromotionId(item.PromotionId),
        new ContributionId(item.ContributionId),
        new RewardMultiplier(item.Multiplier),
        new LadirchenAmount(item.TeamworkBonus),
        item.DeadlineLocalDateTime is null
          ? new PromotionDeadline(item.DeadlineLocalTime)
          : new PromotionDeadline(item.DeadlineLocalDateTime.Value),
        item.Active
      ))
      .ToArray();
  }

  private static void AddContributions(
    FamilyEconomyRootEntity entity,
    FamilyEconomyState state
  )
  {
    foreach (var usedId in state.UsedContributionIds)
    {
      entity.UsedContributionIds.Add(new EconomyUsedContributionIdEntity
      {
        FamilyId = state.Id.Value,
        ContributionId = usedId.Value,
      });
    }

    foreach (var contribution in state.Contributions)
    {
      var contributionEntity = new EconomyContributionEntity
      {
        FamilyId = state.Id.Value,
        ContributionId = contribution.Id.Value,
        Title = contribution.Title,
        Kind = (int)contribution.Kind,
        Reward = contribution.Reward.Value,
        EnergyPercent = contribution.Energy.Value,
        AssigneeId = contribution.AssigneeId?.Value,
        Status = (int)contribution.Status,
        Rating = contribution.Rating?.Value,
        EarnedBaseReward = contribution.EarnedReward?.BaseReward,
        EarnedRatingBonus = contribution.EarnedReward?.RatingBonus,
        EarnedPromotionMultiplier = contribution.EarnedReward?.Multiplier,
        ApprovedAt = contribution.ApprovedAt,
      };

      foreach (var partnerId in contribution.InvitedPartnerIds)
      {
        contributionEntity.InvitedPartners.Add(new EconomyContributionPartnerEntity
        {
          FamilyId = state.Id.Value,
          ContributionId = contribution.Id.Value,
          MemberId = partnerId.Value,
        });
      }

      entity.Contributions.Add(contributionEntity);
    }

    foreach (var promotion in state.Promotions)
    {
      entity.Promotions.Add(new EconomyPromotionEntity
      {
        FamilyId = state.Id.Value,
        PromotionId = promotion.Id.Value,
        ContributionId = promotion.ContributionId.Value,
        Multiplier = promotion.Multiplier.Value,
        TeamworkBonus = promotion.TeamworkBonus.Value,
        DeadlineLocalTime = promotion.Deadline.Time,
        DeadlineLocalDateTime = promotion.Deadline.LocalDeadline,
        Active = promotion.Active,
      });
    }
  }

  private static ContributionRewardBreakdown? ToRewardBreakdown(
    EconomyContributionEntity entity
  )
  {
    if (
      entity.EarnedBaseReward is null
      && entity.EarnedRatingBonus is null
      && entity.EarnedPromotionMultiplier is null
    )
    {
      return null;
    }

    if (
      entity.EarnedBaseReward is null
      || entity.EarnedRatingBonus is null
      || entity.EarnedPromotionMultiplier is null
    )
    {
      throw new InvalidOperationException("A persisted contribution reward breakdown must be complete.");
    }

    return new ContributionRewardBreakdown(
      entity.EarnedBaseReward.Value,
      entity.EarnedRatingBonus.Value,
      entity.EarnedPromotionMultiplier.Value
    );
  }
}
