using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

internal sealed record HouseLayoutEntityState
{
  public HouseLayoutEntityState(
    string id,
    string entityType,
    string entityId,
    HousePlacement placement
  )
  {
    Id = Rules.Text(id);
    EntityType = Rules.Text(entityType);
    Rules.Require(
      entityType is "furniture" or "ladi" or "member" or "pet",
      DomainError.InvalidValue,
      "The house entity type is invalid."
    );
    EntityId = Rules.Text(entityId);
    ArgumentNullException.ThrowIfNull(placement);
    Rules.Require(placement.Scale > 0, DomainError.InvalidValue, "A house-placement scale must be positive.");
    Placement = new HousePlacement(
      Rules.Text(placement.Area),
      placement.X,
      placement.Y,
      placement.Scale
    );
  }

  public string Id { get; }
  public string EntityType { get; }
  public string EntityId { get; }
  public HousePlacement Placement { get; }
}
