using System;

using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Shared;

public sealed record FamilySettingsChanged(FamilyId FamilyId, DateTimeOffset At) : BusinessEvent(At.ToUniversalTime());