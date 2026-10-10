using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record SavingGoalDefinition(GoalOwner Owner, string Title, LadirchenAmount Target, GoalVisibility Visibility);