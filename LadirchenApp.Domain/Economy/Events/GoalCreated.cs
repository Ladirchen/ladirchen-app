using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record GoalCreated(FamilyId FamilyId, SavingGoalId GoalId, GoalOwner Owner, decimal StarterBonus,
  DateTimeOffset At) : BusinessEvent(At.ToUniversalTime());