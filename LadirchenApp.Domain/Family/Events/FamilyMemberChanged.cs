using System;

using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Shared;

public sealed record FamilyMemberChanged(FamilyId FamilyId, FamilyMemberId MemberId, string Change,
  DateTimeOffset At) : BusinessEvent(At.ToUniversalTime());