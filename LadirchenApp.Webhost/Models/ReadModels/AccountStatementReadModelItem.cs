using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record AccountStatementReadModelItem(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string MemberId,
  [property: JsonRequired] string OccurredAtUtc,
  [property: JsonRequired] decimal Amount,
  [property: JsonRequired] decimal BalanceChange,
  [property: JsonRequired] decimal ResultingBalance,
  [property: JsonRequired] string Direction,
  [property: JsonRequired] string TransactionType,
  string? SourceTitle,
  string? CounterpartyMemberId
);
