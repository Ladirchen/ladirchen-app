using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record ShopRewardDefinition(string Title, ShopRewardCategory Category, LadirchenAmount Price,
  int Quantity, bool IsVisible, AvailabilityWindow Window);