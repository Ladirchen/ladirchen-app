using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal sealed record FamilyProgressionEvaluation(
  IReadOnlyList<MemberProgressionEvaluation> Members,
  int FamilyEnergy,
  int HouseVisualLevel,
  Percentage MaximumSavingsInterestRate
);
