using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed record GoalInterestCredited(FamilyId FamilyId, SavingGoalId GoalId, IsoWeekPeriod Period,
  Percentage Rate, decimal Amount, DateTimeOffset At) : BusinessEvent(At.ToUniversalTime());