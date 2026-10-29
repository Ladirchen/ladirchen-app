using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Models;

namespace LadirchenApp.Webhost.Infrastructure;

public sealed class InMemoryAuthenticationService
{
  private const int PasswordIterations = 100_000;
  private static readonly HashSet<string> InitiallyOwnedAccessories =
    ["flower-boxes", "round-rug", "cozy-sofa", "kitchen-counter", "retro-fridge", "family-dining-table"];
  private static readonly HashSet<string> InitiallyEquippedAccessories = new(InitiallyOwnedAccessories);
  private static readonly HashSet<string> InitiallyOwnedRoomDesigns =
    ["garden-ladi-hills", "living-ladi-classic", "kitchen-ladi-classic"];
  private readonly ConcurrentDictionary<string, Account> _accounts = new(StringComparer.Ordinal);
  private readonly IFamilyAggregateChangePublisher _publisher;
  private readonly IFamilyAggregateSnapshotRepository _repository;
  private readonly SemaphoreSlim _registrationLock = new(1, 1);

  public InMemoryAuthenticationService(
    IFamilyAggregateSnapshotRepository repository,
    IFamilyAggregateChangePublisher publisher
  )
  {
    _repository = repository;
    _publisher = publisher;
  }

  public bool UsernameExists(string username)
  {
    return _accounts.ContainsKey(NormalizeUsername(username));
  }

  public AuthenticationSessionResponse? Authenticate(AuthenticateAccountRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    var username = NormalizeUsername(request.Username);

    if (!_accounts.TryGetValue(username, out var account))
    {
      return null;
    }

    var actualHash = HashPassword(request.Password, account.Salt);

    return CryptographicOperations.FixedTimeEquals(actualHash, account.PasswordHash)
      ? new AuthenticationSessionResponse(account.FamilyId, account.FamilyName, account.MemberId)
      : null;
  }

  public async Task<AuthenticationSessionResponse> RegisterAsync(
    RegisterAccountRequest request,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(request);
    ValidateRegistration(request);
    var username = NormalizeUsername(request.Username);
    await _registrationLock.WaitAsync(cancellationToken);

    try
    {
      if (_accounts.ContainsKey(username))
      {
        throw new InvalidOperationException("The username already exists.");
      }

      var actorId = new FamilyMemberId(request.MemberId);
      var familyId = new FamilyId(request.FamilyId);
      var saves = CreateInitialAggregates(familyId, actorId, request.GuardianName.Trim());
      var snapshots = await _repository.SaveBatchAsync(saves, cancellationToken);
      var salt = RandomNumberGenerator.GetBytes(16);
      var account = new Account(
        request.FamilyId,
        request.FamilyName.Trim(),
        request.MemberId,
        HashPassword(request.Password, salt),
        salt
      );
      _accounts[username] = account;

      foreach (var snapshot in snapshots)
      {
        await _publisher.PublishAsync(snapshot, cancellationToken);
      }

      return new AuthenticationSessionResponse(account.FamilyId, account.FamilyName, account.MemberId);
    }
    finally
    {
      _registrationLock.Release();
    }
  }

  private static IReadOnlyList<PendingFamilyAggregateSave> CreateInitialAggregates(
    FamilyId familyId,
    FamilyMemberId actorId,
    string guardianName
  )
  {
    var profile = new JsonObject
    {
      ["members"] = new JsonArray
      {
        new JsonObject
        {
          ["id"] = actorId.Value,
          ["name"] = guardianName,
          ["avatar"] = "🧑",
          ["color"] = "#58aa82",
          ["role"] = "guardian",
          ["guardianAccess"] = "admin",
          ["participatesInWeeklyGoal"] = false,
          ["weeklyStreak"] = 0,
          ["appearance"] = DefaultGuardianAppearance(),
        },
      },
      ["onboardingCompleted"] = false,
      ["pets"] = new JsonArray(),
      ["subscriptionTier"] = "free",
      ["timeZone"] = "Europe/Zurich",
    };
    var contributions = new JsonObject
    {
      ["contributions"] = new JsonArray(),
      ["promotions"] = new JsonArray(),
    };
    var savings = new JsonObject
    {
      ["balances"] = new JsonObject(),
      ["familyCurrencyCode"] = "CHF",
      ["goals"] = new JsonArray(),
      ["ladirchenPerCurrencyUnit"] = 10,
      ["pendingGuardianGifts"] = new JsonArray(),
    };
    var progression = new JsonObject
    {
      ["completedWeeklyStreak"] = 0,
      ["currentWeekTarget"] = 7,
      ["familyEnergy"] = 60,
      ["houseLevel"] = 0,
      ["houseVisualLevel"] = 3,
      ["maximumSavingsInterestRate"] = 8,
      ["memberProgressions"] = new JsonObject(),
    };
    var rewardShop = new JsonObject { ["rewards"] = new JsonArray() };
    var home = CreateInitialHomeCustomization();

    return
    [
      CreateInitialSave(familyId, actorId, "family-profile", profile, 2),
      CreateInitialSave(familyId, actorId, "contributions", contributions, 1),
      CreateInitialSave(familyId, actorId, "savings", savings, 1),
      CreateInitialSave(familyId, actorId, "family-progression", progression, 3),
      CreateInitialSave(familyId, actorId, "reward-shop", rewardShop, 1),
      CreateInitialSave(familyId, actorId, "home-customization", home, 3),
    ];
  }

  private static JsonObject CreateInitialHomeCustomization()
  {
    var accessories = new JsonArray();
    var placements = new JsonArray();

    foreach (var item in HouseShopCatalog.Accessories)
    {
      accessories.Add(
        new JsonObject
        {
          ["id"] = item.Key,
          ["equipped"] = InitiallyEquippedAccessories.Contains(item.Key),
          ["owned"] = InitiallyOwnedAccessories.Contains(item.Key),
        }
      );
      placements.Add(
        new JsonObject
        {
          ["id"] = $"furniture-{item.Key}",
          ["entityId"] = item.Key,
          ["entityType"] = "furniture",
          ["zoneId"] = item.Value.Initial.Area,
          ["x"] = item.Value.Initial.X,
          ["y"] = item.Value.Initial.Y,
          ["scale"] = item.Value.Initial.Scale,
        }
      );
    }

    return new JsonObject
    {
      ["accessories"] = accessories,
      ["editions"] = new JsonArray(
        HouseShopCatalog.Themes.Select(item => new JsonObject { ["id"] = item.Key, ["owned"] = item.Key == "sunny-dollhouse" }).ToArray<JsonNode?>()
      ),
      ["placements"] = placements,
      ["roomDesigns"] = new JsonArray(
        HouseShopCatalog.RoomDesigns.Select(item => new JsonObject { ["id"] = item.Key, ["owned"] = InitiallyOwnedRoomDesigns.Contains(item.Key) }).ToArray<JsonNode?>()
      ),
      ["selectedEditionId"] = "sunny-dollhouse",
      ["selectedRoomDesigns"] = new JsonArray
      {
        new JsonObject { ["designId"] = "garden-ladi-hills", ["zoneId"] = "garden" },
        new JsonObject { ["designId"] = "living-ladi-classic", ["zoneId"] = "living-room" },
        new JsonObject { ["designId"] = "kitchen-ladi-classic", ["zoneId"] = "kitchen" },
      },
    };
  }

  private static PendingFamilyAggregateSave CreateInitialSave(
    FamilyId familyId,
    FamilyMemberId actorId,
    string aggregateType,
    JsonObject state,
    int schemaVersion
  )
  {
    using var document = JsonDocument.Parse(state.ToJsonString());
    var command = new SaveFamilyAggregateCommand(familyId, aggregateType, 0, document.RootElement.Clone(), actorId);

    return new PendingFamilyAggregateSave(command, schemaVersion);
  }

  private static JsonObject DefaultGuardianAppearance()
  {
    return new JsonObject
    {
      ["age"] = "adult",
      ["skinToneId"] = "skin-medium",
      ["faceShape"] = "soft",
      ["face"] = "happy",
      ["hair"] = "short",
      ["hairColorId"] = "hair-brown",
      ["outfit"] = "shirt",
      ["outfitColorId"] = "outfit-emerald",
      ["accessoryId"] = "none",
      ["funAccessoryId"] = "none",
      ["seasonalAccessoryId"] = "none",
    };
  }

  private static byte[] HashPassword(string password, byte[] salt)
  {
    return Rfc2898DeriveBytes.Pbkdf2(password, salt, PasswordIterations, HashAlgorithmName.SHA256, 32);
  }

  private static string NormalizeUsername(string username)
  {
    return username.Trim().ToLowerInvariant();
  }

  private static void ValidateRegistration(RegisterAccountRequest request)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(request.Username);
    ArgumentException.ThrowIfNullOrWhiteSpace(request.Password);
    ArgumentException.ThrowIfNullOrWhiteSpace(request.FamilyName);
    ArgumentException.ThrowIfNullOrWhiteSpace(request.GuardianName);

    if (request.Username.Trim().Length < 3 || request.Password.Length < 6)
    {
      throw new ArgumentException("The username or password does not meet the minimum length.");
    }

    if (request.FamilyName.Trim().Length < 2 || request.GuardianName.Trim().Length < 2)
    {
      throw new ArgumentException("The family and guardian names must contain at least two characters.");
    }
  }

  private sealed record Account(
    Guid FamilyId,
    string FamilyName,
    string MemberId,
    byte[] PasswordHash,
    byte[] Salt
  );
}
