using System;

using LadirchenApp.Domain.Family;

namespace LadirchenApp.Application.Persistence;

public sealed record PendingFamilySave
{
  public PendingFamilySave(Family root, long expectedRevision)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(expectedRevision);
    Root = root ?? throw new ArgumentNullException(nameof(root));
    ExpectedRevision = expectedRevision;
  }

  public Family Root { get; }
  public long ExpectedRevision { get; }
}
