namespace LadirchenApp.Application.Queries;

public sealed record SavingsInterestSimulation(
  string LadiStageId,
  int LadiStagePosition,
  decimal InterestRate,
  decimal ProjectedCredit
);
