using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

internal static partial class FamilyEconomyPersistenceMapper
{
  private static IReadOnlyList<ShopRewardState> ToShopRewardStates(
    FamilyEconomyRootEntity entity
  )
  {
    return entity.ShopRewards
      .OrderBy(item => item.RewardId, StringComparer.Ordinal)
      .Select(item => new ShopRewardState(
        new ShopRewardId(item.RewardId),
        item.Title,
        (ShopRewardCategory)item.Category,
        new LadirchenAmount(item.Price),
        item.Quantity,
        item.IsVisible,
        new AvailabilityWindow(item.AvailableFrom, item.AvailableUntil),
        (ShopRewardStatus)item.Status,
        item.Deleted
      ))
      .ToArray();
  }

  private static IReadOnlyList<RedemptionRequestState> ToRedemptionRequestStates(
    FamilyEconomyRootEntity entity
  )
  {
    return entity.RedemptionRequests
      .OrderBy(item => item.RequestedAt)
      .ThenBy(item => item.RequestId, StringComparer.Ordinal)
      .Select(item => new RedemptionRequestState(
        new RedemptionRequestId(item.RequestId),
        new ShopRewardId(item.RewardId),
        new FamilyMemberId(item.RequesterMemberId),
        new LadirchenAmount(item.Price),
        item.RequestedAt,
        (RedemptionStatus)item.Status,
        item.DecidedAt
      ))
      .ToArray();
  }

  private static void AddRewardShop(
    FamilyEconomyRootEntity entity,
    FamilyEconomyState state
  )
  {
    foreach (var reward in state.ShopRewards)
    {
      entity.ShopRewards.Add(new EconomyShopRewardEntity
      {
        FamilyId = state.Id.Value,
        RewardId = reward.Id.Value,
        Title = reward.Title,
        Category = (int)reward.Category,
        Price = reward.Price.Value,
        Quantity = reward.Quantity,
        IsVisible = reward.IsVisible,
        AvailableFrom = reward.Window.From,
        AvailableUntil = reward.Window.Until,
        Status = (int)reward.Status,
        Deleted = reward.Deleted,
      });
    }

    foreach (var request in state.RedemptionRequests)
    {
      entity.RedemptionRequests.Add(new EconomyRedemptionRequestEntity
      {
        FamilyId = state.Id.Value,
        RequestId = request.Id.Value,
        RewardId = request.RewardId.Value,
        RequesterMemberId = request.RequesterId.Value,
        Price = request.Price.Value,
        RequestedAt = request.RequestedAt,
        Status = (int)request.Status,
        DecidedAt = request.DecidedAt,
      });
    }
  }
}
