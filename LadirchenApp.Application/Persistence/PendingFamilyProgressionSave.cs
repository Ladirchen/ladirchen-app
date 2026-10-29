using System;

using LadirchenApp.Domain.House;

namespace LadirchenApp.Application.Persistence;

public sealed record PendingFamilyProgressionSave
{
  public PendingFamilyProgressionSave(
    FamilyHouse root,
    long expectedRevision
  )
  {
    ArgumentOutOfRangeException.ThrowIfNegative(expectedRevision);
    Root = root ?? throw new ArgumentNullException(nameof(root));
    ExpectedRevision = expectedRevision;
  }

  public FamilyHouse Root { get; }
  public long ExpectedRevision { get; }
}
