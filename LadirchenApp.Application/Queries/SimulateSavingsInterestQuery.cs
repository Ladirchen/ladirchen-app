using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Queries;

public sealed record SimulateSavingsInterestQuery(
  FamilyId FamilyId,
  FamilyMemberId MemberId,
  int StreakDays,
  decimal CompletionPercent,
  decimal AverageRating
);
