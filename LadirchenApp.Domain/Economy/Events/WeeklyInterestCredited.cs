using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed record WeeklyInterestCredited(FamilyId FamilyId, IsoWeekPeriod Period, decimal Total,
  DateTimeOffset At) : BusinessEvent(At.ToUniversalTime());