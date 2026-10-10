using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record ShopRewardDecided(FamilyId FamilyId, RedemptionRequestId RequestId, FamilyMemberId ActorId,
  RedemptionStatus Decision, decimal Price, DateTimeOffset At) : BusinessEvent(At.ToUniversalTime());