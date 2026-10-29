using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Persistence;

public sealed record SavingGoalPresentationState(SavingGoalId GoalId, string Icon);
