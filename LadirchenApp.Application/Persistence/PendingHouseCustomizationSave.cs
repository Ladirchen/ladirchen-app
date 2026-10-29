using System;

using LadirchenApp.Domain.House.Shop;

namespace LadirchenApp.Application.Persistence;

public sealed record PendingHouseCustomizationSave
{
  public PendingHouseCustomizationSave(
    FamilyHouseShop root,
    long expectedRevision
  )
  {
    ArgumentOutOfRangeException.ThrowIfNegative(expectedRevision);
    Root = root ?? throw new ArgumentNullException(nameof(root));
    ExpectedRevision = expectedRevision;
  }

  public FamilyHouseShop Root { get; }
  public long ExpectedRevision { get; }
}
