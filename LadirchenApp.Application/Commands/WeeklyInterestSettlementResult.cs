namespace LadirchenApp.Application.Commands;

public sealed record WeeklyInterestSettlementResult(
  int AttemptedPeriods,
  int SettledPeriods,
  int DeferredPeriods,
  int FailedFamilies
);
