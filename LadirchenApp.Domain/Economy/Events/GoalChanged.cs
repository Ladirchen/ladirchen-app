using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record GoalChanged(FamilyId FamilyId, SavingGoalId GoalId, string Change, decimal Amount,
  DateTimeOffset At) : BusinessEvent(At.ToUniversalTime());