using System.Collections.Generic;

using LadirchenApp.Application.Aggregates;

namespace LadirchenApp.Application.Commands;

public sealed record FamilyCommandResult(IReadOnlyList<FamilyAggregateSnapshot> ChangedAggregates);
