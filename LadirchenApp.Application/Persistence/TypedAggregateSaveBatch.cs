using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Persistence;

public sealed record TypedAggregateSaveBatch
{
  public TypedAggregateSaveBatch(
    FamilyId familyId,
    FamilyMemberId updatedBy,
    PendingFamilySave? family = null,
    PendingFamilyEconomySave? familyEconomy = null,
    PendingFamilyProgressionSave? familyProgression = null,
    PendingHouseCustomizationSave? houseCustomization = null,
    IEnumerable<TypedAggregateRevisionGuard>? revisionGuards = null
  )
  {
    FamilyId = familyId ?? throw new ArgumentNullException(nameof(familyId));
    UpdatedBy = updatedBy ?? throw new ArgumentNullException(nameof(updatedBy));
    Family = family;
    FamilyEconomy = familyEconomy;
    FamilyProgression = familyProgression;
    HouseCustomization = houseCustomization;
    RevisionGuards = (revisionGuards ?? []).ToArray();
    ValidateUniqueExpectations();
  }

  public FamilyId FamilyId { get; }
  public FamilyMemberId UpdatedBy { get; }
  public PendingFamilySave? Family { get; }
  public PendingFamilyEconomySave? FamilyEconomy { get; }
  public PendingFamilyProgressionSave? FamilyProgression { get; }
  public PendingHouseCustomizationSave? HouseCustomization { get; }
  public IReadOnlyList<TypedAggregateRevisionGuard> RevisionGuards { get; }

  private void ValidateUniqueExpectations()
  {
    var kinds = new List<TypedAggregateKind>();

    if (Family is not null)
    {
      kinds.Add(TypedAggregateKind.Family);
    }

    if (FamilyEconomy is not null)
    {
      kinds.Add(TypedAggregateKind.FamilyEconomy);
    }

    if (FamilyProgression is not null)
    {
      kinds.Add(TypedAggregateKind.FamilyProgression);
    }

    if (HouseCustomization is not null)
    {
      kinds.Add(TypedAggregateKind.HouseCustomization);
    }

    kinds.AddRange(RevisionGuards.Select(item => item.AggregateKind));

    if (kinds.Distinct().Count() != kinds.Count)
    {
      throw new ArgumentException(
        "An aggregate root can be written or revision-guarded only once per save batch.",
        nameof(RevisionGuards)
      );
    }
  }
}
