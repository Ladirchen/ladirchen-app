using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed record MemberProgressionReadProjection(
  IReadOnlyList<DateOnly> ApprovedDates,
  PersonalLadiEvaluation Evaluation,
  int EnergyContribution,
  Percentage SavingsInterestRate
);
