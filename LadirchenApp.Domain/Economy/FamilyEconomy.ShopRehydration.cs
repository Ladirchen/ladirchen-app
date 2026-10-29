using System;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed partial class FamilyEconomy
{
  internal void RestoreShopReward(
    ShopRewardId id,
    ShopRewardDefinition definition,
    ShopRewardStatus status
  )
  {
    ArgumentNullException.ThrowIfNull(id);
    ArgumentNullException.ThrowIfNull(definition);
    Rules.Require(
      _shopRewards.All(item => item.Id != id),
      DomainError.Duplicate,
      "This reward identity was already restored."
    );
    var reward = new ShopReward(id, definition);
    reward.Restore(status, false);
    _shopRewards.Add(reward);
  }

  internal void RestorePendingShopRewardRequest(
    RedemptionRequestId id,
    ShopRewardId rewardId,
    FamilyMemberId requesterId,
    LadirchenAmount price,
    DateTimeOffset requestedAt
  )
  {
    ArgumentNullException.ThrowIfNull(id);
    ArgumentNullException.ThrowIfNull(rewardId);
    ArgumentNullException.ThrowIfNull(requesterId);
    ArgumentNullException.ThrowIfNull(price);
    RequireChild(requesterId);
    Rules.Require(
      _requests.All(item => item.Id != id),
      DomainError.Duplicate,
      "This request identity was already restored."
    );
    Rules.Require(
      _requests.All(item => item.RewardId != rewardId || item.Status != RedemptionStatus.Pending),
      DomainError.Duplicate,
      "A reward can have only one pending request."
    );
    var reward = FindShopReward(rewardId);
    Rules.Require(
      reward.Status == ShopRewardStatus.Requested,
      DomainError.InvalidState,
      "A restored pending request requires a requested reward."
    );
    Rules.Require(
      reward.Price.Value == price.Value,
      DomainError.InvalidState,
      "A restored request must retain the reward price."
    );
    _requests.Add(new RedemptionRequest(id, rewardId, requesterId, price, requestedAt));
  }
}
