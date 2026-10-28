using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed record GetFamilyAggregateQuery(FamilyId FamilyId, string AggregateType);
