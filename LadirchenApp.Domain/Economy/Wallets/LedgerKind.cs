using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public enum LedgerKind
{
  Credit, Debit, Reserve, Release, Capture, GoalCredit, GoalDebit, BonusForfeited
}