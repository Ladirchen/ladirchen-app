using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed class RedemptionRequest : Entity<RedemptionRequestId>
{
  internal RedemptionRequest(RedemptionRequestId id, ShopRewardId rewardId, FamilyMemberId requesterId,
    LadirchenAmount price, DateTimeOffset now) : base(id)
  {
    RewardId = rewardId;
    RequesterId = requesterId;
    Price = price;
    RequestedAt = now.ToUniversalTime();
  }
  public ShopRewardId RewardId
  {
    get;
  }
  public FamilyMemberId RequesterId
  {
    get;
  }
  public LadirchenAmount Price
  {
    get;
  }
  public DateTimeOffset RequestedAt
  {
    get;
  }
  public RedemptionStatus Status
  {
    get; private set;
  }
  public DateTimeOffset? DecidedAt
  {
    get; private set;
  }
  internal void Decide(RedemptionStatus status, DateTimeOffset now)
  {
    Status = status;
    DecidedAt = now.ToUniversalTime();
  }

  internal void Restore(RedemptionStatus status, DateTimeOffset? decidedAt)
  {
    Rules.Defined(status);
    Rules.Require(
      status == RedemptionStatus.Pending ? decidedAt is null : decidedAt is not null,
      DomainError.InvalidState,
      "A request decision time must match its status."
    );
    Status = status;
    DecidedAt = decidedAt?.ToUniversalTime();
  }
}
