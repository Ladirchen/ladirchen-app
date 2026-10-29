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

internal sealed class ContributionsFamilyCommandHandler : TypedFamilyCommandAreaHandler
{
  private readonly IFamilyEconomyRepository _economyRepository;

  public ContributionsFamilyCommandHandler(
    IFamilyEconomyRepository economyRepository,
    ITypedAggregateUnitOfWork unitOfWork,
    TimeProvider timeProvider
  )
    : base(unitOfWork, timeProvider, "contributions.")
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
    _ = family.Root.ActiveMember(command.ActorId);
    var loadedEconomy = await _economyRepository.LoadAsync(family.Root, cancellationToken)
      ?? throw new FamilyCommandException("The family economy has not been initialized.");
    var economy = loadedEconomy.Root;
    var payload = RequiredObject(command.Payload);
    var now = TimeProvider.GetUtcNow();
    var operationId = OperationId(command);
    ContributionPresentationState? addedContribution = null;
    ContributionId? celebratedContributionId = null;

    switch (command.CommandType)
    {
      case "contributions.set-perfect-rating-bonus":
        economy.UpdateContributionSettings(
          command.ActorId,
          new ContributionSettings(new Percentage(Round(RequiredDecimal(payload, "percent")))),
          now
        );
        break;
      case "contributions.submit":
        economy.SubmitContribution(command.ActorId, ContributionId(payload), now);
        break;
      case "contributions.claim":
        economy.ClaimContribution(command.ActorId, ContributionId(payload), now);
        break;
      case "contributions.assign":
        economy.AssignContribution(
          command.ActorId,
          ContributionId(payload),
          CreateOptionalMemberId(OptionalString(payload, "memberId")),
          now
        );
        break;
      case "contributions.approve":
        _ = economy.ApproveContribution(
          command.ActorId,
          ContributionId(payload),
          new StarRating(RequiredInt(payload, "stars")),
          operationId,
          now
        );
        break;
      case "contributions.return":
        economy.ReturnContribution(command.ActorId, ContributionId(payload), now);
        break;
      case "contributions.add":
        addedContribution = AddContribution(economy, command.ActorId, payload, now);
        break;
      case "contributions.delete":
        economy.DeleteContribution(command.ActorId, ContributionId(payload), now);
        break;
      case "contributions.delete-promotion":
        economy.DeletePromotion(
          command.ActorId,
          new PromotionId(RequiredString(payload, "id")),
          now
        );
        break;
      case "contributions.set-partners":
        economy.SetContributionPartners(
          command.ActorId,
          ContributionId(payload),
          RequiredArray(payload, "partnerIds")
            .Select(RequiredString)
            .Select(value => new FamilyMemberId(value)),
          now
        );
        break;
      case "contributions.add-promotion":
        AddPromotion(economy, command.ActorId, payload, now);
        break;
      case "contributions.acknowledge-reward":
        celebratedContributionId = ValidateRewardAcknowledgement(
          economy,
          command.ActorId,
          payload
        );
        break;
      default:
        throw new FamilyCommandException("The family command is not supported.");
    }

    var presentation = UpdatePresentation(
      loadedEconomy.Presentation,
      economy,
      addedContribution,
      celebratedContributionId
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
      Change(command.FamilyId, "contributions", changedEconomy.Revision, changedEconomy.Audit),
      Change(command.FamilyId, "family-progression", changedEconomy.Revision, changedEconomy.Audit),
      Change(command.FamilyId, "savings", changedEconomy.Revision, changedEconomy.Audit),
      Change(command.FamilyId, "reward-shop", changedEconomy.Revision, changedEconomy.Audit),
      Change(command.FamilyId, "home-customization", changedEconomy.Revision, changedEconomy.Audit),
    ]);
  }

  private static ContributionPresentationState AddContribution(
    FamilyEconomy economy,
    FamilyMemberId actorId,
    JsonElement payload,
    DateTimeOffset now
  )
  {
    var id = ContributionId(payload);
    var kind = RequiredString(payload, "kind") switch
    {
      "basic" => ContributionKind.Basic,
      "extra" => ContributionKind.Extra,
      _ => throw new FamilyCommandException("The contribution kind is invalid."),
    };
    economy.AddContribution(
      actorId,
      id,
      new ContributionDefinition(
        RequiredString(payload, "title"),
        kind,
        new LadirchenAmount(RequiredDecimal(payload, "reward")),
        new Percentage(RequiredDecimal(payload, "energy")),
        CreateOptionalMemberId(OptionalString(payload, "assigneeId"))
      ),
      now
    );

    return new ContributionPresentationState(
      id,
      RequiredString(payload, "description"),
      RequiredString(payload, "icon"),
      false
    );
  }

  private static void AddPromotion(
    FamilyEconomy economy,
    FamilyMemberId actorId,
    JsonElement payload,
    DateTimeOffset now
  )
  {
    if (!PromotionDeadline.TryParse(RequiredString(payload, "deadline"), out var deadline))
    {
      throw new FamilyCommandException("The promotion deadline is invalid.");
    }

    economy.AddPromotion(
      actorId,
      new PromotionId(RequiredString(payload, "id")),
      new ContributionId(RequiredString(payload, "contributionId")),
      new RewardMultiplier(RequiredDecimal(payload, "multiplier")),
      new LadirchenAmount(RequiredDecimal(payload, "teamworkBonus")),
      deadline!,
      now
    );
  }

  private static ContributionId ValidateRewardAcknowledgement(
    FamilyEconomy economy,
    FamilyMemberId actorId,
    JsonElement payload
  )
  {
    var id = ContributionId(payload);
    var contribution = economy.Contributions.SingleOrDefault(item => item.Id == id)
      ?? throw new FamilyCommandException("The contribution does not exist.");
    Require(
      contribution.AssigneeId == actorId && contribution.Status == ContributionStatus.Approved,
      "Only the recipient may acknowledge an approved reward."
    );

    return id;
  }

  private static FamilyEconomyPresentationState UpdatePresentation(
    FamilyEconomyPresentationState presentation,
    FamilyEconomy economy,
    ContributionPresentationState? addedContribution,
    ContributionId? celebratedContributionId
  )
  {
    var liveIds = economy.Contributions.Select(item => item.Id).ToHashSet();
    var contributions = presentation.Contributions
      .Where(item => liveIds.Contains(item.ContributionId))
      .Select(item => item.ContributionId == celebratedContributionId
        ? item with { RewardCelebrated = true }
        : item)
      .ToList();

    if (addedContribution is not null)
    {
      Require(
        contributions.All(item => item.ContributionId != addedContribution.ContributionId),
        "Presentation data for the contribution already exists."
      );
      contributions.Add(addedContribution);
    }

    Require(
      liveIds.All(id => contributions.Any(item => item.ContributionId == id)),
      "Presentation data for a contribution is missing."
    );

    return new FamilyEconomyPresentationState(
      contributions,
      presentation.SavingGoals,
      presentation.ShopRewards,
      presentation.GuardianGifts
    );
  }

  private static ContributionId ContributionId(JsonElement payload)
  {
    return new ContributionId(RequiredString(payload, "id"));
  }

  private static TransactionId OperationId(FamilyCommand command)
  {
    return new TransactionId(command.CommandId.ToString("N", CultureInfo.InvariantCulture));
  }

  private static FamilyMemberId? CreateOptionalMemberId(string? value)
  {
    return string.IsNullOrWhiteSpace(value) ? null : new FamilyMemberId(value);
  }
}
