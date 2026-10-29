using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;

namespace LadirchenApp.Application.Aggregates;

internal sealed record ContributionReadModelState(Family Family, FamilyEconomy Economy);