using System;
using System.Collections.Generic;

namespace LadirchenApp.Prototypes;

public abstract class AggregateRoot<TId> : Entity<TId>
  where TId : notnull
{
  private readonly List<IDomainEvent> _domainEvents = new();

  protected AggregateRoot(TId id)
    : base(id)
  {
    DomainEvents = _domainEvents.AsReadOnly();
  }

  public IReadOnlyCollection<IDomainEvent> DomainEvents
  {
    get;
  }

  protected void AddDomainEvent(IDomainEvent domainEvent)
  {
    ArgumentNullException.ThrowIfNull(domainEvent);
    _domainEvents.Add(domainEvent);
  }

  public void ClearDomainEvents()
  {
    _domainEvents.Clear();
  }
}
