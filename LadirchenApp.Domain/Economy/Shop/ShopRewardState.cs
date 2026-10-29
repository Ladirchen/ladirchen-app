using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

internal sealed record ShopRewardState(
  ShopRewardId Id,
  string Title,
  ShopRewardCategory Category,
  LadirchenAmount Price,
  int Quantity,
  bool IsVisible,
  AvailabilityWindow Window,
  ShopRewardStatus Status,
  bool Deleted
);
