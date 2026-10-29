using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class EconomyShopRewardEntity
{
  public Guid FamilyId { get; set; }
  public string RewardId { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public int Category { get; set; }
  public decimal Price { get; set; }
  public int Quantity { get; set; }
  public bool IsVisible { get; set; }
  public DateOnly? AvailableFrom { get; set; }
  public DateOnly? AvailableUntil { get; set; }
  public int Status { get; set; }
  public bool Deleted { get; set; }
  public FamilyEconomyRootEntity Economy { get; set; } = null!;
}
