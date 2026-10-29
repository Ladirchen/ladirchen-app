using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal sealed record MemberProgressionEvaluation(
  PersonalLadiEvaluation LadiEvaluation,
  IReadOnlyList<DateOnly> ApprovedDates,
  int EnergyContribution,
  Percentage SavingsInterestRate
);
