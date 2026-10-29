namespace LadirchenApp.Domain.House.Shop;

public sealed record HouseLayoutEntity(
  string Id,
  string EntityType,
  string EntityId,
  HousePlacement Placement,
  bool Active
);
