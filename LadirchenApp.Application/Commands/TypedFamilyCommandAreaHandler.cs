using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal abstract class TypedFamilyCommandAreaHandler : ITypedFamilyCommandAreaHandler
{
  private readonly string _commandPrefix;

  protected TypedFamilyCommandAreaHandler(
    ITypedAggregateUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    string commandPrefix
  )
  {
    UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    TimeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    _commandPrefix = commandPrefix;
  }

  protected ITypedAggregateUnitOfWork UnitOfWork { get; }
  protected TimeProvider TimeProvider { get; }

  public bool CanHandle(FamilyCommand command)
  {
    ArgumentNullException.ThrowIfNull(command);

    return command.CommandType.StartsWith(_commandPrefix, StringComparison.Ordinal);
  }

  public abstract Task<FamilyCommandResult> HandleAsync(
    FamilyCommand command,
    LoadedAggregate<Family> family,
    CancellationToken cancellationToken
  );

  protected static FamilyAggregateChange Change(
    FamilyId familyId,
    string aggregateType,
    long revision,
    AggregateAudit audit
  )
  {
    ArgumentNullException.ThrowIfNull(familyId);
    ArgumentException.ThrowIfNullOrWhiteSpace(aggregateType);
    ArgumentOutOfRangeException.ThrowIfNegative(revision);
    ArgumentNullException.ThrowIfNull(audit);

    return new FamilyAggregateChange(
      familyId,
      aggregateType,
      revision,
      audit.UpdatedBy,
      audit.UpdatedAt
    );
  }

  protected static JsonElement RequiredObject(JsonElement value)
  {
    if (value.ValueKind != JsonValueKind.Object)
    {
      throw new FamilyCommandException("A JSON object is required.");
    }

    return value;
  }

  protected static JsonElement RequiredObject(JsonElement value, string property)
  {
    var result = RequiredProperty(value, property);

    if (result.ValueKind != JsonValueKind.Object)
    {
      throw new FamilyCommandException($"The {property} object is required.");
    }

    return result;
  }

  protected static IEnumerable<JsonElement> RequiredArray(JsonElement value, string property)
  {
    var result = RequiredProperty(value, property);

    if (result.ValueKind != JsonValueKind.Array)
    {
      throw new FamilyCommandException($"The {property} collection is required.");
    }

    return result.EnumerateArray();
  }

  protected static string RequiredString(JsonElement value, string property)
  {
    var result = OptionalString(value, property);

    if (string.IsNullOrWhiteSpace(result))
    {
      throw new FamilyCommandException($"The {property} value is required.");
    }

    return result;
  }

  protected static string RequiredString(JsonElement value)
  {
    if (value.ValueKind != JsonValueKind.String)
    {
      throw new FamilyCommandException("A string value is required.");
    }

    var result = value.GetString();

    if (string.IsNullOrWhiteSpace(result))
    {
      throw new FamilyCommandException("A non-empty string value is required.");
    }

    return result;
  }

  protected static string? OptionalString(JsonElement value, string property)
  {
    if (!value.TryGetProperty(property, out var result) || result.ValueKind == JsonValueKind.Null)
    {
      return null;
    }

    if (result.ValueKind != JsonValueKind.String)
    {
      throw new FamilyCommandException($"The {property} value must be a string.");
    }

    return result.GetString();
  }

  protected static decimal RequiredDecimal(JsonElement value, string property)
  {
    var result = RequiredProperty(value, property);

    if (result.ValueKind != JsonValueKind.Number || !result.TryGetDecimal(out var number))
    {
      throw new FamilyCommandException($"The {property} value must be a decimal number.");
    }

    return number;
  }

  protected static int RequiredInt(JsonElement value, string property)
  {
    var result = RequiredProperty(value, property);

    if (result.ValueKind != JsonValueKind.Number || !result.TryGetInt32(out var number))
    {
      throw new FamilyCommandException($"The {property} value must be an integer.");
    }

    return number;
  }

  protected static bool RequiredBool(JsonElement value, string property)
  {
    var result = RequiredProperty(value, property);

    if (result.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
    {
      throw new FamilyCommandException($"The {property} value must be a boolean.");
    }

    return result.GetBoolean();
  }

  protected static bool? OptionalBool(JsonElement value, string property)
  {
    if (!value.TryGetProperty(property, out var result) || result.ValueKind == JsonValueKind.Null)
    {
      return null;
    }

    if (result.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
    {
      throw new FamilyCommandException($"The {property} value must be a boolean.");
    }

    return result.GetBoolean();
  }

  protected static decimal Round(decimal value)
  {
    return decimal.Round(value, 0, MidpointRounding.AwayFromZero);
  }

  protected static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new FamilyCommandException(message);
    }
  }

  private static JsonElement RequiredProperty(JsonElement value, string property)
  {
    RequiredObject(value);

    return value.TryGetProperty(property, out var result)
      ? result
      : throw new FamilyCommandException($"The {property} value is required.");
  }
}
