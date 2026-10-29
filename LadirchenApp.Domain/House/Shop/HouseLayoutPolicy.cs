using System;
using System.Collections.Generic;
using System.Linq;

namespace LadirchenApp.Domain.House.Shop;

public sealed class HouseLayoutPolicy
{
  private const decimal CharacterHorizontalRadius = 4m;
  private const decimal CharacterVerticalRadius = 6m;
  private static readonly IReadOnlyDictionary<string, HouseCollisionBounds> CollisionBounds =
    new Dictionary<string, HouseCollisionBounds>(StringComparer.Ordinal)
    {
      ["art-desk"] = new(11m, 9m),
      ["bedside-table"] = new(8m, 9m),
      ["bookshelf"] = new(8m, 12m),
      ["bunk-bed"] = new(14m, 12m),
      ["candy-cloud-bushes"] = new(8m, 9m),
      ["candy-striped-fence"] = new(8m, 9m),
      ["cat-tree"] = new(8m, 9m),
      ["cozy-sofa"] = new(14m, 10m),
      ["double-bed"] = new(15m, 13m),
      ["family-dining-table"] = new(9m, 8m),
      ["floor-lamp"] = new(8m, 9m),
      ["garden-chair"] = new(8m, 9m),
      ["garden-pool"] = new(8m, 9m),
      ["garden-sunshade"] = new(8m, 9m),
      ["garden-table"] = new(8m, 9m),
      ["garden-trampoline"] = new(8m, 9m),
      ["halloween-pumpkin-arch"] = new(8m, 9m),
      ["hammock"] = new(8m, 9m),
      ["kitchen-counter"] = new(12m, 7m),
      ["plant-corner"] = new(8m, 9m),
      ["play-table"] = new(9m, 8m),
      ["retro-fridge"] = new(6m, 13m),
      ["storage-cabinet"] = new(8m, 9m),
      ["telescope"] = new(8m, 9m),
      ["wardrobe"] = new(8m, 13m),
    };

  public HousePlacement Normalize(string entityType, string entityId, string area, decimal x, decimal y, decimal scale)
  {
    if (entityType == "ladi" && area == "living-room" && x is >= 6m and <= 31m && y is >= 12m and <= 47m)
    {
      return new HousePlacement(area, 18m, 30m, scale);
    }

    var isFloorEntity = entityType is "member" or "pet" or "ladi";
    var horizontalInset = isFloorEntity ? 7m : 3m;
    var minimumY = isFloorEntity ? 62m : MinimumFurnitureY(entityId);
    var maximumY = isFloorEntity ? 94m : MaximumFurnitureY(entityId);

    return new HousePlacement(
      area,
      decimal.Round(Math.Clamp(x, horizontalInset, 100m - horizontalInset), 2, MidpointRounding.AwayFromZero),
      decimal.Round(Math.Clamp(y, minimumY, maximumY), 2, MidpointRounding.AwayFromZero),
      scale
    );
  }

  public bool CollidesWithFurniture(HouseLayoutEntity moved, IEnumerable<HouseLayoutEntity> entities)
  {
    ArgumentNullException.ThrowIfNull(moved);
    ArgumentNullException.ThrowIfNull(entities);

    if (moved.EntityType is not ("member" or "pet" or "ladi") || IsLadiPerch(moved))
    {
      return false;
    }

    return entities
      .Where(entity => entity.Id != moved.Id)
      .Where(entity => entity.Active && entity.EntityType == "furniture")
      .Where(entity => entity.Placement.Area == moved.Placement.Area)
      .Any(entity => Collides(moved.Placement, entity));
  }

  private static bool Collides(HousePlacement character, HouseLayoutEntity furniture)
  {
    if (!CollisionBounds.TryGetValue(furniture.EntityId, out var bounds))
    {
      return false;
    }

    var horizontalLimit = bounds.HorizontalRadius * furniture.Placement.Scale + CharacterHorizontalRadius;
    var verticalLimit = bounds.VerticalRadius * furniture.Placement.Scale + CharacterVerticalRadius;

    return Math.Abs(character.X - furniture.Placement.X) < horizontalLimit
      && Math.Abs(character.Y - furniture.Placement.Y) < verticalLimit;
  }

  private static bool IsLadiPerch(HouseLayoutEntity entity)
  {
    return entity.EntityType == "ladi"
      && entity.Placement.Area == "living-room"
      && entity.Placement.X == 18m
      && entity.Placement.Y == 30m;
  }

  private static decimal MinimumFurnitureY(string entityId)
  {
    return entityId switch
    {
      "halloween-bat-garland" or "wall-art" => 12m,
      "garden-lights" => 58m,
      _ => 52m,
    };
  }

  private static decimal MaximumFurnitureY(string entityId)
  {
    return entityId is "halloween-bat-garland" or "wall-art" ? 46m : 94m;
  }
}
