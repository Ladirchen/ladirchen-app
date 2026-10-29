using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Persistence;

public sealed record GuardianGiftPresentationState(
  GuardianGiftId GiftId,
  string SenderName,
  string GoalTitle
);
