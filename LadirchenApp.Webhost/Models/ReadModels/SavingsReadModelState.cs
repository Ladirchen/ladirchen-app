using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record SavingsReadModelState(
  [property: JsonRequired] IReadOnlyDictionary<string, decimal> AvailableBalances,
  [property: JsonRequired] IReadOnlyDictionary<string, decimal> Balances,
  [property: JsonRequired] string FamilyCurrencyCode,
  [property: JsonRequired] IReadOnlyList<SavingGoalReadModel> Goals,
  [property: JsonRequired] decimal LadirchenPerCurrencyUnit,
  [property: JsonRequired] IReadOnlyList<GuardianGiftReadModel> PendingGuardianGifts,
  [property: JsonRequired] IReadOnlyList<string> SettledInterestPeriods,
  [property: JsonRequired] IReadOnlyDictionary<string, decimal> SharedSavedBalances,
  [property: JsonRequired] IReadOnlyList<SavingsVoucherReadModel> Vouchers
);
