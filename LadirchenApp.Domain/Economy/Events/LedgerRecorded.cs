using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record LedgerRecorded(FamilyId FamilyId, LedgerEntry Entry) : BusinessEvent(Entry.OccurredAtUtc);