using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Queries;

public sealed record WeeklyInterestSettlementPlan(
  FamilyMemberId AdministratorId,
  IReadOnlyList<IsoWeekPeriod> Periods
);
