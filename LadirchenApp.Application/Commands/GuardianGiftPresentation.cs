using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal sealed record GuardianGiftPresentation(
  GuardianGiftId Id,
  string SenderName,
  string GoalTitle
);
