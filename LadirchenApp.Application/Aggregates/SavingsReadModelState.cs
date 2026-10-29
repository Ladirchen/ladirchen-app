using System.Collections.Generic;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

internal sealed record SavingsReadModelState(
  Family Family,
  FamilyEconomy Economy,
  Percentage MaximumInterestRate,
  IReadOnlyDictionary<FamilyMemberId, LadiStage> LadiStages,
  IReadOnlySet<FamilyMemberId> StoredBalanceMemberIds
);
