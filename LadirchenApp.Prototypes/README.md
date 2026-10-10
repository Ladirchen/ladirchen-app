# DDD Primitives

These types provide domain modeling foundations without persistence or messaging
dependencies. They use the `LadirchenApp.Prototypes` namespace.

## Value Objects

Derive an immutable class from `ValueObject` and expose its equality components in
a stable order. Equality requires the same concrete runtime type and equal
components. Hash codes use the same components and type.

```csharp
using System;
using System.Collections.Generic;
using LadirchenApp.Prototypes;

public sealed class DisplayName : ValueObject
{
  public DisplayName(string value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(value);
    Value = value;
  }

  public string Value { get; }

  protected override IEnumerable<object?> GetEqualityComponents()
  {
    yield return Value;
  }
}
```

Components use their own equality semantics. Collections are not compared deeply
automatically: expose their elements deliberately when sequence equality is
required. Keep all equality components immutable, including nested values, to
preserve hash-based collection lookups. Derive classes, not records, from
`ValueObject`.

## Entities and Aggregates

`Entity<TId>` has a get-only, caller-assigned ID. Equality depends only on that ID
and the concrete runtime type, never on other state. Null IDs are rejected;
default value-type IDs are accepted. Derived types can impose stricter rules.
IDs must themselves have stable equality and hashing. Exact-type equality does
not accommodate ORM-generated proxy subclasses.

`AggregateRoot<TId>` adds a pending domain-event buffer:

```csharp
using System;
using System.Linq;
using LadirchenApp.Prototypes;

public sealed record NameChanged(Guid MemberId, string Name) : DomainEvent;

public sealed class Member : AggregateRoot<Guid>
{
  public Member(Guid id, DisplayName name) : base(id)
  {
    ArgumentNullException.ThrowIfNull(name);
    Name = name;
  }

  public DisplayName Name { get; private set; }

  public void Rename(DisplayName name)
  {
    ArgumentNullException.ThrowIfNull(name);

    if (Name == name)
    {
      return;
    }

    Name = name;
    AddDomainEvent(new NameChanged(Id, name.Value));
  }
}
```

Strongly typed IDs also work, for example a `readonly record struct MemberId(Guid
Value)` used with `Entity<MemberId>` or `AggregateRoot<MemberId>`. No automatic
entity ID generation is performed.

## Event Lifecycle

Each newly constructed `DomainEvent` has a generated event ID and a UTC occurrence
timestamp. Derived records carry domain-specific data; keep that data immutable.
A record `with` clone preserves event metadata, so construct a new event for a
distinct notification. `IDomainEvent` also allows custom event implementations.

`DomainEvents` is a live read-only view, not a snapshot. Reading does not clear it.
Appending preserves insertion order and allows repeated events. Take a snapshot
when retaining events beyond clearing:

```csharp
var pendingEvents = member.DomainEvents.ToArray();
```

The application layer owns dispatch and persistence. Call
`member.ClearDomainEvents()` only after events have been successfully handled or
durably captured. There is no automatic dispatch, retry, event sourcing, replay,
versioning, or thread synchronization. Coordinate aggregate access and event
handling in the application layer.
