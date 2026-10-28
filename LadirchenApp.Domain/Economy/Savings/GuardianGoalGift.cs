using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed record GuardianGoalGift(FamilyMemberId OwnerId, FamilyMemberId GuardianId, decimal Amount);
