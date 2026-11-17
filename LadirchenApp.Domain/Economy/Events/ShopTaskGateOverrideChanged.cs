using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record ShopTaskGateOverrideChanged(
  FamilyId FamilyId,
  FamilyMemberId ChildId,
  bool Enabled,
  DateTimeOffset At
) : BusinessEvent(At.ToUniversalTime());