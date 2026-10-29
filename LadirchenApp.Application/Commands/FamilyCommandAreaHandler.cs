using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal abstract class FamilyCommandAreaHandler : IFamilyCommandAreaHandler
{
  private readonly string _commandPrefix;
  protected readonly IFamilyAggregateSnapshotRepository _repository;
  protected readonly TimeProvider _timeProvider;

  protected FamilyCommandAreaHandler(
    IFamilyAggregateSnapshotRepository repository,
    TimeProvider timeProvider,
    string commandPrefix
  )
  {
    _repository = repository;
    _timeProvider = timeProvider;
    _commandPrefix = commandPrefix;
  }

  public bool CanHandle(FamilyCommand command)
  {
    return command.CommandType.StartsWith(_commandPrefix, StringComparison.Ordinal);
  }

  public abstract Task<FamilyCommandResult> HandleAsync(
    FamilyCommand command,
    FamilyAggregateSnapshot profile,
    JsonObject profileState,
    JsonObject payload,
    CancellationToken cancellationToken
  );

  protected async Task<FamilyAggregateSnapshot> RequireSnapshotAsync(
    FamilyId familyId,
    string aggregateType,
    CancellationToken cancellationToken
  )
  {
    return await _repository.GetAsync(familyId, aggregateType, cancellationToken)
      ?? throw new FamilyCommandException($"The {aggregateType} aggregate has not been initialized.");
  }

  internal static PendingFamilyAggregateSave CreateSave(
    FamilyAggregateSnapshot current,
    JsonObject state,
    FamilyMemberId actorId
  )
  {
    using var document = JsonDocument.Parse(state.ToJsonString());
    var command = new SaveFamilyAggregateCommand(
      current.FamilyId,
      current.AggregateType,
      current.Revision,
      document.RootElement.Clone(),
      actorId
    );

    return new PendingFamilyAggregateSave(
      command,
      FamilyAggregateSchema.GetVersion(current.AggregateType)
    );
  }

  internal static decimal Round(decimal value)
  {
    return decimal.Round(value, 0, MidpointRounding.AwayFromZero);
  }

  internal static JsonObject RequireObject(JsonElement element)
  {
    return JsonNode.Parse(element.GetRawText()) as JsonObject
      ?? throw new FamilyCommandException("A JSON object is required.");
  }

  internal static JsonObject RequireObject(JsonObject parent, string property)
  {
    return parent[property] as JsonObject
      ?? throw new FamilyCommandException($"The {property} object is required.");
  }

  internal static JsonArray RequireArray(JsonObject parent, string property)
  {
    return parent[property] as JsonArray
      ?? throw new FamilyCommandException($"The {property} collection is required.");
  }

  internal static string RequiredString(JsonObject value, string property)
  {
    var result = StringValue(value, property);
    Require(!string.IsNullOrWhiteSpace(result), $"The {property} value is required.");

    return result!;
  }

  internal static string RequiredString(JsonNode? value)
  {
    var result = value?.GetValue<string>();
    Require(!string.IsNullOrWhiteSpace(result), "A non-empty string value is required.");

    return result!;
  }

  internal static string? StringValue(JsonObject value, string property)
  {
    return value[property]?.GetValue<string>();
  }

  internal static int RequiredInt(JsonObject value, string property)
  {
    return value[property]?.GetValue<int>()
      ?? throw new FamilyCommandException($"The {property} value is required.");
  }

  internal static decimal RequiredDecimal(JsonObject value, string property)
  {
    return DecimalValue(value, property)
      ?? throw new FamilyCommandException($"The {property} value is required.");
  }

  internal static decimal? DecimalValue(JsonObject value, string property)
  {
    return value[property]?.GetValue<decimal>();
  }

  internal static bool BoolValueOrDefault(JsonObject value, string property, bool defaultValue)
  {
    return value[property]?.GetValue<bool>() ?? defaultValue;
  }

  internal static bool RequiredBool(JsonObject value, string property)
  {
    return value[property]?.GetValue<bool>()
      ?? throw new FamilyCommandException($"The {property} value is required.");
  }

  internal static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new FamilyCommandException(message);
    }
  }
}
