using System;

namespace LadirchenApp.Application.Persistence;

public sealed record LoadedAggregate<TAggregate>
{
  public LoadedAggregate(TAggregate root, long revision, AggregateAudit audit)
  {
    ArgumentNullException.ThrowIfNull(root);
    ArgumentOutOfRangeException.ThrowIfNegative(revision);
    Root = root;
    Revision = revision;
    Audit = audit ?? throw new ArgumentNullException(nameof(audit));
  }

  public TAggregate Root { get; }
  public long Revision { get; }
  public AggregateAudit Audit { get; }
}
