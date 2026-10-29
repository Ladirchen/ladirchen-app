using System;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal static partial class FamilyEconomySnapshotMapper
{
  private static void RestoreEconomyHistory(FamilyEconomy economy, JsonObject? savingsState)
  {
    if (savingsState is null)
    {
      return;
    }

    foreach (var node in OptionalArray(savingsState, "appliedOperationIds"))
    {
      economy.RestoreAppliedOperation(new TransactionId(RequiredString(node)));
    }

    foreach (var item in OptionalArray(savingsState, "ledger").OfType<JsonObject>())
    {
      economy.RestoreLedgerEntry(
        new LedgerEntry(
          new TransactionId(RequiredString(item, "operationId")),
          new FamilyMemberId(RequiredString(item, "memberId")),
          OptionalString(item, "goalId") is { } goalId ? new SavingGoalId(goalId) : null,
          ParseLedgerKind(RequiredString(item, "kind")),
          RequiredDecimal(item, "amount"),
          ParseLedgerSource(RequiredString(item, "sourceType"), RequiredString(item, "sourceId")),
          RequiredString(item, "reason"),
          OptionalInstant(item, "occurredAtUtc")
            ?? throw new FamilyCommandException("A stored ledger entry requires its occurrence time."),
          OptionalString(item, "initiatorId") is { } initiatorId ? new FamilyMemberId(initiatorId) : null
        )
        {
          EntryId = ParseEntryId(RequiredString(item, "entryId")),
        }
      );
    }
  }

  private static void WriteEconomyHistory(JsonObject state, FamilyEconomy economy)
  {
    state["appliedOperationIds"] = IdentifierArray(economy.AppliedOperationIds);
    state["ledger"] = new JsonArray(
      economy.Ledger.Select(entry =>
      {
        var (sourceType, sourceId) = FormatLedgerSource(entry.SourceId);

        return new JsonObject
        {
          ["entryId"] = entry.EntryId.ToString("D", CultureInfo.InvariantCulture),
          ["operationId"] = entry.OperationId.Value,
          ["memberId"] = entry.MemberId.Value,
          ["goalId"] = entry.GoalId?.Value,
          ["kind"] = entry.Kind.ToString(),
          ["amount"] = entry.Amount,
          ["sourceType"] = sourceType,
          ["sourceId"] = sourceId,
          ["reason"] = entry.Reason,
          ["occurredAtUtc"] = entry.OccurredAtUtc.ToString("O", CultureInfo.InvariantCulture),
          ["initiatorId"] = entry.InitiatorId?.Value,
        };
      }).ToArray<JsonNode?>()
    );
  }

  private static Guid ParseEntryId(string value)
  {
    return Guid.TryParse(value, out var entryId) && entryId != Guid.Empty
      ? entryId
      : throw new FamilyCommandException("The stored ledger-entry ID is invalid.");
  }

  private static LedgerKind ParseLedgerKind(string value)
  {
    return Enum.TryParse<LedgerKind>(value, false, out var kind)
      ? kind
      : throw new FamilyCommandException("The stored ledger kind is invalid.");
  }

  private static DomainIdentifier ParseLedgerSource(string sourceType, string sourceId)
  {
    return sourceType switch
    {
      "contribution" => new ContributionId(sourceId),
      "family-member" => new FamilyMemberId(sourceId),
      "house-content" => new HouseContentId(sourceId),
      "redemption-request" => new RedemptionRequestId(sourceId),
      "saving-goal" => new SavingGoalId(sourceId),
      _ => throw new FamilyCommandException("The stored ledger-source type is invalid."),
    };
  }

  private static (string Type, string Id) FormatLedgerSource(DomainIdentifier source)
  {
    return source switch
    {
      ContributionId id => ("contribution", id.Value),
      FamilyMemberId id => ("family-member", id.Value),
      HouseContentId id => ("house-content", id.Value),
      RedemptionRequestId id => ("redemption-request", id.Value),
      SavingGoalId id => ("saving-goal", id.Value),
      _ => throw new FamilyCommandException("The ledger-source type cannot be persisted."),
    };
  }
}
