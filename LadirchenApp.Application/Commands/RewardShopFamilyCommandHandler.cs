using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal sealed class RewardShopFamilyCommandHandler : TypedFamilyCommandAreaHandler
{
  private readonly IFamilyEconomyRepository _economyRepository;

  public RewardShopFamilyCommandHandler(
    IFamilyEconomyRepository economyRepository,
    ITypedAggregateUnitOfWork unitOfWork,
    TimeProvider timeProvider
  )
    : base(unitOfWork, timeProvider, "reward-shop.")
  {
    _economyRepository = economyRepository ?? throw new ArgumentNullException(nameof(economyRepository));
  }

  public override async Task<FamilyCommandResult> HandleAsync(
    FamilyCommand command,
    LoadedAggregate<Family> family,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(command);
    ArgumentNullException.ThrowIfNull(family);
    var loadedEconomy = await _economyRepository.LoadAsync(family.Root, cancellationToken)
      ?? throw new FamilyCommandException("The family economy has not been initialized.");
    var economy = loadedEconomy.Root;
    var payload = RequiredObject(command.Payload);
    var now = TimeProvider.GetUtcNow();
    var operationId = OperationId(command);
    ShopRewardPresentationState? addedReward = null;

    switch (command.CommandType)
    {
      case "reward-shop.add":
        addedReward = AddShopReward(economy, command.ActorId, payload, now);
        break;
      case "reward-shop.set-visibility":
        economy.SetShopRewardVisibility(
          command.ActorId,
          RewardId(payload),
          RequiredBool(payload, "isVisible"),
          now
        );
        break;
      case "reward-shop.delete":
        economy.DeleteShopReward(command.ActorId, RewardId(payload), operationId, now);
        break;
      case "reward-shop.request":
        economy.RequestShopReward(
          command.ActorId,
          RewardId(payload),
          new RedemptionRequestId(command.CommandId.ToString("N", CultureInfo.InvariantCulture)),
          operationId,
          now
        );
        break;
      case "reward-shop.cancel":
        economy.CancelShopRewardRequest(
          command.ActorId,
          PendingRequestId(economy, RewardId(payload)),
          operationId,
          now
        );
        break;
      case "reward-shop.decide":
        DecideShopReward(economy, command.ActorId, payload, operationId, now);
        break;
      default:
        throw new FamilyCommandException("The reward-shop command is not supported.");
    }

    var presentation = UpdatePresentation(
      loadedEconomy.Presentation,
      economy,
      addedReward
    );
    var saved = await UnitOfWork.SaveAsync(
      new TypedAggregateSaveBatch(
        command.FamilyId,
        command.ActorId,
        familyEconomy: new PendingFamilyEconomySave(
          economy,
          presentation,
          loadedEconomy.Revision
        ),
        revisionGuards:
        [
          new TypedAggregateRevisionGuard(TypedAggregateKind.Family, family.Revision),
        ]
      ),
      cancellationToken
    );
    var changedEconomy = saved.FamilyEconomy
      ?? throw new InvalidOperationException("The family economy was not saved.");

    return new FamilyCommandResult(
    [
      Change(command.FamilyId, "reward-shop", changedEconomy.Revision, changedEconomy.Audit),
      Change(command.FamilyId, "savings", changedEconomy.Revision, changedEconomy.Audit),
      Change(command.FamilyId, "home-customization", changedEconomy.Revision, changedEconomy.Audit),
    ]);
  }

  private static ShopRewardPresentationState AddShopReward(
    FamilyEconomy economy,
    FamilyMemberId actorId,
    JsonElement payload,
    DateTimeOffset now
  )
  {
    var id = RewardId(payload);
    var icon = RequiredString(payload, "icon").Trim();
    var title = RequiredString(payload, "title").Trim();
    var description = RequiredString(payload, "description").Trim();
    var conditions = RequiredString(payload, "conditions").Trim();
    var price = RequiredDecimal(payload, "price");
    var quantity = RequiredInt(payload, "quantity");
    var from = OptionalCalendarDate(payload, "availableFrom");
    var until = OptionalCalendarDate(payload, "availableUntil");
    var catalogPolicy = new FamilyRewardCatalogPolicy();

    if (!catalogPolicy.IsValidNewReward(
      icon,
      title,
      description,
      conditions,
      price,
      quantity,
      from,
      until,
      from is not null,
      until is not null
    ))
    {
      throw new FamilyCommandException("The shop reward data is invalid.");
    }

    economy.AddShopReward(
      actorId,
      id,
      new ShopRewardDefinition(
        title,
        ParseShopRewardCategory(RequiredString(payload, "category")),
        new LadirchenAmount(price),
        quantity,
        OptionalBool(payload, "isVisible") ?? true,
        new AvailabilityWindow(from, until)
      ),
      now
    );

    return new ShopRewardPresentationState(id, description, icon, conditions);
  }

  private static void DecideShopReward(
    FamilyEconomy economy,
    FamilyMemberId actorId,
    JsonElement payload,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    var requestId = PendingRequestId(economy, RewardId(payload));

    if (RequiredBool(payload, "approved"))
    {
      economy.ApproveShopReward(actorId, requestId, operationId, now);
    }
    else
    {
      economy.RejectShopReward(actorId, requestId, operationId, now);
    }
  }

  private static FamilyEconomyPresentationState UpdatePresentation(
    FamilyEconomyPresentationState presentation,
    FamilyEconomy economy,
    ShopRewardPresentationState? addedReward
  )
  {
    var liveRewardIds = economy.ShopRewards.Select(item => item.Id).ToHashSet();
    var rewards = presentation.ShopRewards
      .Where(item => liveRewardIds.Contains(item.RewardId))
      .ToList();

    if (addedReward is not null)
    {
      Require(
        rewards.All(item => item.RewardId != addedReward.RewardId),
        "Presentation data for the shop reward already exists."
      );
      rewards.Add(addedReward);
    }

    Require(
      liveRewardIds.All(id => rewards.Any(item => item.RewardId == id)),
      "Presentation data for a shop reward is missing."
    );

    return new FamilyEconomyPresentationState(
      presentation.Contributions,
      presentation.SavingGoals,
      rewards,
      presentation.GuardianGifts
    );
  }

  private static RedemptionRequestId PendingRequestId(
    FamilyEconomy economy,
    ShopRewardId rewardId
  )
  {
    return economy.RedemptionRequests
      .SingleOrDefault(item =>
        item.RewardId == rewardId && item.Status == RedemptionStatus.Pending
      )
      ?.Id
      ?? throw new FamilyCommandException("A pending reward request is required.");
  }

  private static ShopRewardId RewardId(JsonElement payload)
  {
    return new ShopRewardId(RequiredString(payload, "id"));
  }

  private static TransactionId OperationId(FamilyCommand command)
  {
    return new TransactionId(command.CommandId.ToString("N", CultureInfo.InvariantCulture));
  }

  private static ShopRewardCategory ParseShopRewardCategory(string value)
  {
    return value.Trim().ToLowerInvariant() switch
    {
      "time" => ShopRewardCategory.Time,
      "activity" => ShopRewardCategory.Activity,
      "allowance" => ShopRewardCategory.Allowance,
      "gift" => ShopRewardCategory.Gift,
      "privilege" => ShopRewardCategory.Privilege,
      "custom" => ShopRewardCategory.Custom,
      _ => throw new FamilyCommandException("The shop reward category is invalid."),
    };
  }

  private static DateOnly? OptionalCalendarDate(JsonElement source, string property)
  {
    var value = OptionalString(source, property);

    if (string.IsNullOrWhiteSpace(value))
    {
      return null;
    }

    if (!DateOnly.TryParseExact(
      value,
      "yyyy-MM-dd",
      CultureInfo.InvariantCulture,
      DateTimeStyles.None,
      out var result
    ))
    {
      throw new FamilyCommandException("The shop date is invalid.");
    }

    return result;
  }
}
