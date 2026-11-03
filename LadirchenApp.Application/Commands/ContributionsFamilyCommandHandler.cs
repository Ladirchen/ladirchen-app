using System;
using System.Collections.Generic;
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
    ContributionPresentationState? updatedContribution = null;
    ContributionId? celebratedContributionId = null;
    var notifications = new List<FamilyUserNotification>();

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
        AddAssignmentNotification(economy, payload, notifications);
        break;
      case "contributions.approve":
        var approvedContribution = economy.Contributions.Single(item => item.Id == ContributionId(payload));
        var reward = economy.ApproveContribution(
          command.ActorId,
          ContributionId(payload),
          new StarRating(RequiredInt(payload, "stars")),
          operationId,
          now
        );
        notifications.Add(RewardNotification(approvedContribution, reward.Total));
        break;
      case "contributions.return":
        economy.ReturnContribution(command.ActorId, ContributionId(payload), now);
        break;
      case "contributions.add":
        addedContribution = AddContribution(economy, command.ActorId, payload, now);
        AddAssignmentNotification(economy, payload, notifications);
        break;
      case "contributions.update":
        updatedContribution = UpdateContribution(economy, command.ActorId, payload, now);
        break;
      case "contributions.copy":
        addedContribution = CopyContribution(
          economy,
          loadedEconomy.Presentation,
          command.ActorId,
          payload,
          now
        );
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
      updatedContribution,
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
    ], notifications);
  }

  private static void AddAssignmentNotification(
    FamilyEconomy economy,
    JsonElement payload,
    ICollection<FamilyUserNotification> notifications
  )
  {
    var contribution = economy.Contributions.Single(item => item.Id == ContributionId(payload));

    if (contribution.AssigneeId is null)
    {
      return;
    }

    notifications.Add(new FamilyUserNotification(
      contribution.AssigneeId,
      "Neue Aufgabe",
      $"Dir wurde die Aufgabe „{contribution.Title}“ zugewiesen.",
      "New task",
      $"The task “{contribution.Title}” was assigned to you.",
      "/#/contributions",
      $"contribution-{contribution.Id.Value}"
    ));
  }

  private static FamilyUserNotification RewardNotification(
    Contribution contribution,
    decimal amount
  )
  {
    var recipientId = contribution.AssigneeId
      ?? throw new InvalidOperationException("An approved contribution requires an assignee.");

    return new FamilyUserNotification(
      recipientId,
      "Ladirchen erhalten",
      $"Du hast {amount:0.##} Ladirchen für „{contribution.Title}“ erhalten.",
      "Ladirchen received",
      $"You received {amount:0.##} Ladirchen for “{contribution.Title}”.",
      "/#/contributions",
      $"contribution-reward-{contribution.Id.Value}"
    );
  }

  private static ContributionPresentationState AddContribution(
    FamilyEconomy economy,
    FamilyMemberId actorId,
    JsonElement payload,
    DateTimeOffset now
  )
  {
    var id = ContributionId(payload);
    economy.AddContribution(actorId, id, ContributionDefinition(payload), now);

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

  private static ContributionPresentationState UpdateContribution(
    FamilyEconomy economy,
    FamilyMemberId actorId,
    JsonElement payload,
    DateTimeOffset now
  )
  {
    var id = ContributionId(payload);
    economy.UpdateContribution(actorId, id, ContributionDefinition(payload), now);

    return new ContributionPresentationState(
      id,
      RequiredString(payload, "description"),
      RequiredString(payload, "icon"),
      false
    );
  }

  private static ContributionPresentationState CopyContribution(
    FamilyEconomy economy,
    FamilyEconomyPresentationState presentation,
    FamilyMemberId actorId,
    JsonElement payload,
    DateTimeOffset now
  )
  {
    var sourceId = new ContributionId(RequiredString(payload, "sourceId"));
    var copyId = ContributionId(payload);
    economy.CopyContribution(actorId, sourceId, copyId, now);
    var sourcePresentation = presentation.Contributions.SingleOrDefault(item => item.ContributionId == sourceId)
      ?? throw new FamilyCommandException("Presentation data for the source contribution is missing.");

    return sourcePresentation with { ContributionId = copyId, RewardCelebrated = false };
  }

  private static ContributionDefinition ContributionDefinition(JsonElement payload)
  {
    var kind = RequiredString(payload, "kind") switch
    {
      "basic" => ContributionKind.Basic,
      "extra" => ContributionKind.Extra,
      _ => throw new FamilyCommandException("The contribution kind is invalid."),
    };

    return new ContributionDefinition(
      RequiredString(payload, "title"),
      kind,
      new LadirchenAmount(RequiredDecimal(payload, "reward")),
      new Percentage(RequiredDecimal(payload, "energy")),
      CreateOptionalMemberId(OptionalString(payload, "assigneeId"))
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
    ContributionPresentationState? updatedContribution,
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

    if (updatedContribution is not null)
    {
      var index = contributions.FindIndex(item => item.ContributionId == updatedContribution.ContributionId);
      Require(index >= 0, "Presentation data for the contribution is missing.");
      contributions[index] = updatedContribution;
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
