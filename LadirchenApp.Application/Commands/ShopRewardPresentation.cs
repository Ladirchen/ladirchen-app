using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal sealed record ShopRewardPresentation(
  ShopRewardId Id,
  string Description,
  string Icon,
  string Conditions
);