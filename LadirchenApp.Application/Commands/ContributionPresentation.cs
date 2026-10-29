using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal sealed record ContributionPresentation(
  ContributionId Id,
  string Description,
  string Icon
);
