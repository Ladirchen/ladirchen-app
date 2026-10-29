using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Avatar;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

public sealed partial class ExecuteFamilyCommandHandler
{
  private readonly IFamilyAggregateSnapshotRepository _repository;
  private readonly IFamilyAggregateChangePublisher _publisher;
  private readonly TimeProvider _timeProvider;

  public ExecuteFamilyCommandHandler(
    IFamilyAggregateSnapshotRepository repository,
    IFamilyAggregateChangePublisher publisher,
    TimeProvider timeProvider
  )
  {
    _repository = repository;
    _publisher = publisher;
    _timeProvider = timeProvider;
  }

  public async Task<FamilyCommandResult> HandleAsync(FamilyCommand command, CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(command);

    return await ExecuteAsync(command, cancellationToken);
  }

  private async Task<FamilyCommandResult> ExecuteAsync(
    FamilyCommand command,
    CancellationToken cancellationToken
  )
  {
    var profile = await RequireSnapshotAsync(command.FamilyId, "family-profile", cancellationToken);
    var profileState = RequireObject(profile.State);
    var members = RequireArray(profileState, "members");
    var payload = RequireObject(command.Payload);
    var actor = RequireMember(members, command.ActorId.Value.ToString());

    if (command.CommandType.StartsWith("profile.", StringComparison.Ordinal))
    {
      return await ExecuteProfileAsync(command, profile, profileState, members, actor, payload, cancellationToken);
    }

    if (command.CommandType.StartsWith("home.", StringComparison.Ordinal))
    {
      return await ExecuteHomeAsync(command, profileState, actor, payload, cancellationToken);
    }

    if (command.CommandType.StartsWith("savings.", StringComparison.Ordinal))
    {
      return await ExecuteSavingsAsync(command, profileState, members, actor, payload, cancellationToken);
    }

    if (command.CommandType.StartsWith("reward-shop.", StringComparison.Ordinal))
    {
      return await ExecuteRewardShopAsync(
        command,
        profileState,
        members,
        actor,
        payload,
        cancellationToken
      );
    }

    if (command.CommandType.StartsWith("progression.", StringComparison.Ordinal))
    {
      return await ExecuteProgressionAsync(command, profile, profileState, members, actor, cancellationToken);
    }

    if (command.CommandType.StartsWith("contributions.", StringComparison.Ordinal))
    {
      return await ExecuteContributionsAsync(
        command,
        profileState,
        members,
        actor,
        payload,
        cancellationToken
      );
    }

    throw new FamilyCommandException("The family command is not supported.");
  }

  private async Task<FamilyAggregateSnapshot> RequireSnapshotAsync(
    FamilyId familyId,
    string aggregateType,
    CancellationToken cancellationToken
  )
  {
    return await _repository.GetAsync(familyId, aggregateType, cancellationToken)
      ?? throw new FamilyCommandException($"The {aggregateType} aggregate has not been initialized.");
  }

  private static PendingFamilyAggregateSave CreateSave(
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

    return new PendingFamilyAggregateSave(command, current.SchemaVersion);
  }

  private static decimal Round(decimal value)
  {
    return decimal.Round(value, 0, MidpointRounding.AwayFromZero);
  }

  private static JsonObject RequireMember(JsonArray members, string id)
  {
    return FindById(members, id) ?? throw new FamilyCommandException("The family member does not exist.");
  }

  private static JsonObject? FindById(JsonArray items, string id)
  {
    return items.OfType<JsonObject>().FirstOrDefault(item => StringValue(item, "id") == id);
  }

  private static void RequireStatus(JsonObject contribution, string status)
  {
    Require(StringValue(contribution, "status") == status, $"The contribution must be {status}.");
  }

  private static void RequireAdministrator(JsonObject actor)
  {
    Require(IsAdministrator(actor), "Only a family administrator may perform this command.");
  }

  private static bool IsAdministrator(JsonObject member)
  {
    return FamilyMemberPermissionResolver.Resolve(member).ManageFamily;
  }

  private static JsonObject RequireObject(JsonElement element)
  {
    return JsonNode.Parse(element.GetRawText()) as JsonObject
      ?? throw new FamilyCommandException("A JSON object is required.");
  }

  private static JsonObject RequireObject(JsonObject parent, string property)
  {
    return parent[property] as JsonObject
      ?? throw new FamilyCommandException($"The {property} object is required.");
  }

  private static JsonArray RequireArray(JsonObject parent, string property)
  {
    return parent[property] as JsonArray
      ?? throw new FamilyCommandException($"The {property} collection is required.");
  }

  private static string RequiredString(JsonObject value, string property)
  {
    var result = StringValue(value, property);
    Require(!string.IsNullOrWhiteSpace(result), $"The {property} value is required.");

    return result!;
  }

  private static string RequiredString(JsonNode? value)
  {
    var result = value?.GetValue<string>();
    Require(!string.IsNullOrWhiteSpace(result), "A non-empty string value is required.");

    return result!;
  }

  private static string? StringValue(JsonObject value, string property)
  {
    return value[property]?.GetValue<string>();
  }

  private static int RequiredInt(JsonObject value, string property)
  {
    return value[property]?.GetValue<int>()
      ?? throw new FamilyCommandException($"The {property} value is required.");
  }

  private static decimal RequiredDecimal(JsonObject value, string property)
  {
    return DecimalValue(value, property)
      ?? throw new FamilyCommandException($"The {property} value is required.");
  }

  private static decimal? DecimalValue(JsonObject value, string property)
  {
    return value[property]?.GetValue<decimal>();
  }

  private static bool BoolValue(JsonObject value, string property)
  {
    return value[property]?.GetValue<bool>() ?? false;
  }

  private static bool BoolValueOrDefault(JsonObject value, string property, bool defaultValue)
  {
    return value[property]?.GetValue<bool>() ?? defaultValue;
  }

  private static bool RequiredBool(JsonObject value, string property)
  {
    return value[property]?.GetValue<bool>()
      ?? throw new FamilyCommandException($"The {property} value is required.");
  }

  private static void CopyOptionalString(JsonObject source, JsonObject target, string property)
  {
    var value = StringValue(source, property);

    if (!string.IsNullOrWhiteSpace(value))
    {
      target[property] = value;
    }
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new FamilyCommandException(message);
    }
  }
}
