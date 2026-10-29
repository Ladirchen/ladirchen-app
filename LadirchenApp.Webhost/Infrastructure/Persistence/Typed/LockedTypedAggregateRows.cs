using System;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

internal sealed class LockedTypedAggregateRows
{
  public FamilyRootEntity? Family { get; set; }
  public FamilyEconomyRootEntity? FamilyEconomy { get; set; }
  public FamilyProgressionRootEntity? FamilyProgression { get; set; }
  public HouseCustomizationRootEntity? HouseCustomization { get; set; }

  public long Revision(TypedAggregateKind kind)
  {
    return kind switch
    {
      TypedAggregateKind.Family => Family?.Revision ?? 0,
      TypedAggregateKind.FamilyEconomy => FamilyEconomy?.Revision ?? 0,
      TypedAggregateKind.FamilyProgression => FamilyProgression?.Revision ?? 0,
      TypedAggregateKind.HouseCustomization => HouseCustomization?.Revision ?? 0,
      _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
  }
}
