using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed record FamilyProgressionReadModelProjection(
  long Revision,
  bool CanCloseWeekSuccessfully,
  int CompletedWeeklyStreak,
  int CurrentWeekTarget,
  int FamilyEnergy,
  string HouseAtmosphere,
  int HouseLevel,
  int HouseVisualLevel,
  Percentage MaximumSavingsInterestRate,
  int MinimumSuccessfulWeekEnergy,
  IReadOnlyDictionary<FamilyMemberId, MemberProgressionReadProjection> MemberProgressions
) : IFamilyReadModelProjection
{
  public string ReadModelType
  {
    get
    {
      return "family-progression";
    }
  }
}
