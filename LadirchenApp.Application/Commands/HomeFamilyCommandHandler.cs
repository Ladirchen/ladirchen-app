using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal sealed class HomeFamilyCommandHandler : TypedFamilyCommandAreaHandler
{
  private readonly IFamilyEconomyRepository _economyRepository;
  private readonly IFamilyProgressionRepository _progressionRepository;
  private readonly IHouseCustomizationRepository _houseRepository;

  public HomeFamilyCommandHandler(
    IFamilyEconomyRepository economyRepository,
    IFamilyProgressionRepository progressionRepository,
    IHouseCustomizationRepository houseRepository,
    ITypedAggregateUnitOfWork unitOfWork,
    TimeProvider timeProvider
  )
    : base(unitOfWork, timeProvider, "home.")
  {
    _economyRepository = economyRepository
      ?? throw new ArgumentNullException(nameof(economyRepository));
    _progressionRepository = progressionRepository
      ?? throw new ArgumentNullException(nameof(progressionRepository));
    _houseRepository = houseRepository
      ?? throw new ArgumentNullException(nameof(houseRepository));
  }

  public override async Task<FamilyCommandResult> HandleAsync(
    FamilyCommand command,
    LoadedAggregate<Family> loadedFamily,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(command);
    ArgumentNullException.ThrowIfNull(loadedFamily);
    var family = loadedFamily.Root;
    var progression = await _progressionRepository.LoadAsync(family, cancellationToken)
      ?? throw new FamilyCommandException(
        "The family-progression aggregate has not been initialized."
      );
    var loadedHouse = await _houseRepository.LoadAsync(
      family,
      progression.Root.StoredLevel,
      cancellationToken
    ) ?? throw new FamilyCommandException(
      "The home-customization aggregate has not been initialized."
    );
    var isPurchase = IsHomePurchase(command.CommandType);
    LoadedFamilyEconomy? loadedEconomy = null;
    var availableBalance = 0m;

    if (isPurchase)
    {
      loadedEconomy = await _economyRepository.LoadAsync(family, cancellationToken)
        ?? throw new FamilyCommandException(
          "The family-economy aggregate has not been initialized."
        );
      availableBalance = loadedEconomy.Root.WalletFor(command.ActorId).Available;
    }

    var purchase = ExecuteHomeCommand(
      loadedHouse.Root,
      family,
      command,
      RequiredObject(command.Payload),
      availableBalance
    );

    if (isPurchase)
    {
      if (purchase is null || loadedEconomy is null)
      {
        throw new InvalidOperationException("A house purchase must return its purchased content.");
      }

      loadedEconomy.Root.PurchaseHouseContent(
        command.ActorId,
        new LadirchenAmount(purchase.Price),
        new HouseContentId($"{command.CommandType}:{purchase.ContentId}"),
        new TransactionId(command.CommandId.ToString("N", CultureInfo.InvariantCulture)),
        TimeProvider.GetUtcNow()
      );
    }

    var result = await UnitOfWork.SaveAsync(
      new TypedAggregateSaveBatch(
        family.Id,
        command.ActorId,
        familyEconomy: loadedEconomy is null
          ? null
          : new PendingFamilyEconomySave(
            loadedEconomy.Root,
            loadedEconomy.Presentation,
            loadedEconomy.Revision
          ),
        houseCustomization: new PendingHouseCustomizationSave(
          loadedHouse.Root,
          loadedHouse.Revision
        ),
        revisionGuards:
        [
          new TypedAggregateRevisionGuard(
            TypedAggregateKind.Family,
            loadedFamily.Revision
          ),
          new TypedAggregateRevisionGuard(
            TypedAggregateKind.FamilyProgression,
            progression.Revision
          ),
        ]
      ),
      cancellationToken
    );
    var savedHouse = result.HouseCustomization
      ?? throw new InvalidOperationException("The house customization was not saved.");
    var changes = new List<FamilyAggregateChange>
    {
      Change(
        family.Id,
        "home-customization",
        savedHouse.Revision,
        savedHouse.Audit
      ),
    };

    if (result.FamilyEconomy is not null)
    {
      changes.Add(
        Change(
          family.Id,
          "savings",
          result.FamilyEconomy.Revision,
          result.FamilyEconomy.Audit
        )
      );
      changes.Add(
        Change(
          family.Id,
          "reward-shop",
          result.FamilyEconomy.Revision,
          result.FamilyEconomy.Audit
        )
      );
    }

    return new FamilyCommandResult(changes);
  }

  private static HousePurchase? ExecuteHomeCommand(
    FamilyHouseShop house,
    Family family,
    FamilyCommand command,
    JsonElement payload,
    decimal availableBalance
  )
  {
    var actorIsChild = family.ActiveMember(command.ActorId).Role == MemberRole.Child;

    switch (command.CommandType)
    {
      case "home.purchase-accessory":
        return RequirePurchase(
          house.PurchaseAccessoryContent(
            RequiredString(payload, "id"),
            actorIsChild,
            availableBalance
          ),
          "The house accessory cannot be purchased."
        );
      case "home.purchase-furniture-set":
        return RequirePurchase(
          house.PurchaseFurnitureSetContent(
            RequiredString(payload, "id"),
            actorIsChild,
            availableBalance
          ),
          "The furniture set cannot be purchased."
        );
      case "home.toggle-accessory":
        Require(
          house.ToggleAccessory(RequiredString(payload, "id")),
          "The house accessory cannot be toggled."
        );
        break;
      case "home.store-accessory":
        Require(
          house.StoreAccessory(RequiredString(payload, "id")),
          "The house accessory cannot be stored."
        );
        break;
      case "home.place-accessory":
        Require(
          house.PlaceAccessory(
            RequiredString(payload, "id"),
            RequiredString(payload, "zoneId")
          ),
          "The house accessory cannot be placed."
        );
        break;
      case "home.purchase-theme":
        return RequirePurchase(
          house.PurchaseThemeContent(
            RequiredString(payload, "id"),
            actorIsChild,
            availableBalance
          ),
          "The house theme cannot be purchased."
        );
      case "home.select-theme":
        Require(
          house.SelectTheme(RequiredString(payload, "id")),
          "The house theme cannot be selected."
        );
        break;
      case "home.purchase-room-design":
        return RequirePurchase(
          house.PurchaseRoomDesignContent(
            RequiredString(payload, "id"),
            actorIsChild,
            availableBalance
          ),
          "The room design cannot be purchased."
        );
      case "home.select-room-design":
        Require(
          house.SelectRoomDesign(RequiredString(payload, "id")),
          "The room design cannot be selected."
        );
        break;
      case "home.move-entity":
        Require(
          house.ArrangeEntity(
            RequiredString(payload, "placementId"),
            RequiredString(payload, "zoneId"),
            RequiredDecimal(payload, "x"),
            RequiredDecimal(payload, "y")
          ) is not null,
          "The house entity cannot be moved."
        );
        break;
      case "home.reset-entity":
        Require(
          house.ResetEntity(RequiredString(payload, "placementId")) is not null,
          "The house entity cannot be reset."
        );
        break;
      default:
        throw new FamilyCommandException("The home-customization command is not supported.");
    }

    return null;
  }

  private static HousePurchase RequirePurchase(HousePurchase? purchase, string message)
  {
    return purchase ?? throw new FamilyCommandException(message);
  }

  private static bool IsHomePurchase(string commandType)
  {
    return commandType is
      "home.purchase-accessory"
      or "home.purchase-furniture-set"
      or "home.purchase-theme"
      or "home.purchase-room-design";
  }
}
