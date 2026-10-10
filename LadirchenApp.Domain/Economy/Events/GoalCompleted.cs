using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record GoalCompleted(FamilyId FamilyId, SavingGoalId GoalId, DateTimeOffset At)
  : BusinessEvent(At.ToUniversalTime());