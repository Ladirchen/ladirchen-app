using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
  private readonly IFamilyProgressionRepository _progressionRepository;

  public ContributionsFamilyCommandHandler(
    IFamilyEconomyRepository economyRepository,
    IFamilyProgressionRepository progressionRepository,
    ITypedAggregateUnitOfWork unitOfWork,
    TimeProvider timeProvider
  )
    : base(unitOfWork, timeProvider, "contributions.")
  {
    _economyRepository = economyRepository ?? throw new ArgumentNullException(nameof(economyRepository));
    _progressionRepository = progressionRepository ?? throw new ArgumentNullException(nameof(progressionRepository));
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
    var loadedProgression = await _progressionRepository.LoadAsync(family.Root, cancellationToken)
      ?? throw new FamilyCommandException("The family progression has not been initialized.");
    var payload = RequiredObject(command.Payload);
    var now = TimeProvider.GetUtcNow();
    var operationId = OperationId(command);
    ContributionPresentationState? addedContribution = null;
    IReadOnlyList<ContributionPresentationState> addedContributions = [];
    ContributionPresentationState? updatedContribution = null;
    ContributionPresentationState? updatedFocus = null;
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
        RequireNotFocus(loadedEconomy.Presentation, ContributionId(payload));
        economy.SubmitContribution(command.ActorId, ContributionId(payload), now);
        break;
      case "contributions.claim":
        RequireNotFocus(loadedEconomy.Presentation, ContributionId(payload));
        economy.ClaimContribution(command.ActorId, ContributionId(payload), now);
        break;
      case "contributions.assign":
        economy.AssignContribution(
          command.ActorId,
          ContributionId(payload),
          CreateOptionalMemberId(OptionalString(payload, "memberId")),
          now
        );
        AddAssignmentNotification(
          family.Root,
          economy,
          payload,
          notifications,
          IsVisibleToChildren(loadedEconomy.Presentation, ContributionId(payload))
        );
        break;
      case "contributions.approve":
        var approvedContribution = economy.Contributions.Single(item => item.Id == ContributionId(payload));
        RequireNotFocus(loadedEconomy.Presentation, approvedContribution.Id);
        var stars = RequiredInt(payload, "stars");
        var reward = economy.ApproveContribution(
          command.ActorId,
          ContributionId(payload),
          new StarRating(stars),
          operationId,
          now
        );
        notifications.Add(RewardNotification(approvedContribution, reward.Total));
        updatedFocus = SettleFocusBonus(
          economy,
          loadedEconomy.Presentation,
          command,
          command.ActorId,
          approvedContribution,
          stars,
          now,
          notifications
        );
        addedContribution = CreateNextRecurringContribution(
          economy,
          loadedEconomy.Presentation,
          approvedContribution.CreatorId,
          approvedContribution.Id,
          family.Root.TimeZone.LocalDate(now),
          now
        );
        break;
      case "contributions.return":
        RequireNotFocus(loadedEconomy.Presentation, ContributionId(payload));
        economy.ReturnContribution(command.ActorId, ContributionId(payload), now);
        break;
      case "contributions.add-focus":
        family.Root.RequireAdministrator(command.ActorId);
        addedContribution = AddFocus(economy, command.ActorId, payload, now);
        break;
      case "contributions.convert-to-focus":
        family.Root.RequireAdministrator(command.ActorId);
        updatedContribution = ConvertToFocus(
          family.Root,
          economy,
          loadedEconomy.Presentation,
          command.ActorId,
          payload,
          now
        );
        break;
      case "contributions.add-focus-preparations":
        family.Root.RequireAdministrator(command.ActorId);
        addedContributions = AddFocusPreparations(
          economy,
          loadedEconomy.Presentation,
          command.ActorId,
          payload,
          now
        );
        break;
      case "contributions.add":
        RequireAdministratorForHiddenContribution(family.Root, command.ActorId, payload);
        addedContribution = AddContribution(economy, command.ActorId, payload, now);
        AddAssignmentNotification(
          family.Root,
          economy,
          payload,
          notifications,
          IsVisibleToChildren(payload)
        );
        break;
      case "contributions.update":
        RequireAdministratorForHiddenContribution(family.Root, command.ActorId, payload);
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
      case "contributions.reactivate":
        addedContribution = ReactivateContribution(
          economy,
          loadedEconomy.Presentation,
          command.ActorId,
          payload,
          now
        );
        AddAssignmentNotification(
          family.Root,
          economy,
          payload,
          notifications,
          IsVisibleToChildren(
            loadedEconomy.Presentation,
            new ContributionId(RequiredString(payload, "sourceId"))
          )
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
      addedContributions,
      updatedContribution,
      updatedFocus,
      celebratedContributionId
    );
    var period = IsoWeekPeriod.For(family.Root.TimeZone.LocalDate(now));
    var progressionEvaluation = FamilyProgressionEvaluator.Evaluate(
      family.Root,
      economy,
      loadedProgression.Root,
      period
    );
    loadedProgression.Root.RecordDailyEnergy(
      family.Root.TimeZone.LocalDate(now),
      progressionEvaluation.FamilyEnergy
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
        familyProgression: new PendingFamilyProgressionSave(
          loadedProgression.Root,
          loadedProgression.Revision
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
    var changedProgression = saved.FamilyProgression
      ?? throw new InvalidOperationException("The family progression was not saved.");

    return new FamilyCommandResult(
    [
      Change(command.FamilyId, "contributions", changedEconomy.Revision, changedEconomy.Audit),
      Change(
        command.FamilyId,
        "family-progression",
        changedProgression.Revision,
        changedProgression.Audit
      ),
      Change(command.FamilyId, "savings", changedEconomy.Revision, changedEconomy.Audit),
      Change(command.FamilyId, "reward-shop", changedEconomy.Revision, changedEconomy.Audit),
      Change(command.FamilyId, "home-customization", changedEconomy.Revision, changedEconomy.Audit),
    ], notifications);
  }

  private static void AddAssignmentNotification(
    Family family,
    FamilyEconomy economy,
    JsonElement payload,
    ICollection<FamilyUserNotification> notifications,
    bool isVisibleToChildren = true
  )
  {
    var contribution = economy.Contributions.Single(item => item.Id == ContributionId(payload));

    if (contribution.AssigneeId is null)
    {
      return;
    }

    var recipient = family.ActiveMember(contribution.AssigneeId);

    if (!isVisibleToChildren && recipient.Role == MemberRole.Child)
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
      false,
      ContributionCategory(payload),
      ContributionDueDate(payload),
      ContributionRecurrence(payload),
      IsVisibleToChildren: IsVisibleToChildren(payload)
    );
  }

  private static ContributionPresentationState AddFocus(
    FamilyEconomy economy,
    FamilyMemberId actorId,
    JsonElement payload,
    DateTimeOffset now
  )
  {
    var id = ContributionId(payload);
    var period = RequiredString(payload, "period");
    Require(period is "date" or "weekly" or "monthly", "The focus period is invalid.");
    var startDate = RequiredDate(payload, "startDate");
    var endDate = RequiredDate(payload, "endDate");
    Require(endDate >= startDate, "The focus end date must not be before its start date.");
    var completionBonus = Round(RequiredDecimal(payload, "completionBonus"));
    Require(
      completionBonus >= 0 && completionBonus <= SpecialGiftPolicy.MaximumAmount,
      "The focus completion bonus is invalid."
    );
    economy.AddContribution(
      actorId,
      id,
      new ContributionDefinition(
        RequiredString(payload, "title"),
        ContributionKind.Extra,
        new LadirchenAmount(0),
        new Percentage(0),
        new FamilyMemberId(RequiredString(payload, "assigneeId"))
      ),
      now
    );

    return FocusPresentation(id, payload, period, startDate, endDate, completionBonus);
  }

  private static ContributionPresentationState ConvertToFocus(
    Family family,
    FamilyEconomy economy,
    FamilyEconomyPresentationState presentation,
    FamilyMemberId actorId,
    JsonElement payload,
    DateTimeOffset now
  )
  {
    var id = ContributionId(payload);
    var contribution = economy.Contributions.SingleOrDefault(item => item.Id == id)
      ?? throw new FamilyCommandException("The contribution does not exist.");
    var contributionPresentation = presentation.Contributions.SingleOrDefault(
      item => item.ContributionId == id
    ) ?? throw new FamilyCommandException("Presentation data for the contribution is missing.");
    Require(!contributionPresentation.IsFocus, "The contribution is already a focus goal.");
    Require(
      contribution.Status == ContributionStatus.Available,
      "Only an available contribution can be converted to a focus goal."
    );
    Require(
      economy.Promotions.All(item => item.ContributionId != id),
      "A contribution with a promotion cannot be converted to a focus goal."
    );
    Require(contribution.AssigneeId is not null, "An assigned contribution is required.");
    var assigneeId = new FamilyMemberId(RequiredString(payload, "assigneeId"));
    Require(
      family.ActiveMember(assigneeId).Role == MemberRole.Child,
      "A focus goal must be assigned to a child."
    );
    var period = RequiredString(payload, "period");
    Require(period is "date" or "weekly" or "monthly", "The focus period is invalid.");
    var startDate = RequiredDate(payload, "startDate");
    var endDate = RequiredDate(payload, "endDate");
    Require(endDate >= startDate, "The focus end date must not be before its start date.");
    var completionBonus = Round(RequiredDecimal(payload, "completionBonus"));
    Require(
      completionBonus >= 0 && completionBonus <= SpecialGiftPolicy.MaximumAmount,
      "The focus completion bonus is invalid."
    );
    economy.UpdateContribution(
      actorId,
      id,
      new ContributionDefinition(
        RequiredString(payload, "title"),
        ContributionKind.Extra,
        new LadirchenAmount(0),
        new Percentage(0),
        assigneeId
      ),
      now
    );

    return FocusPresentation(id, payload, period, startDate, endDate, completionBonus);
  }

  private static ContributionPresentationState FocusPresentation(
    ContributionId id,
    JsonElement payload,
    string period,
    DateOnly startDate,
    DateOnly endDate,
    decimal completionBonus
  )
  {
    return new ContributionPresentationState(
      id,
      RequiredString(payload, "description"),
      RequiredString(payload, "icon"),
      false,
      "Fokus",
      endDate,
      "none",
      true,
      period,
      startDate,
      endDate,
      completionBonus
    );
  }

  private static IReadOnlyList<ContributionPresentationState> AddFocusPreparations(
    FamilyEconomy economy,
    FamilyEconomyPresentationState presentation,
    FamilyMemberId actorId,
    JsonElement payload,
    DateTimeOffset now
  )
  {
    var focusId = new ContributionId(RequiredString(payload, "focusId"));
    var focusPresentation = presentation.Contributions.SingleOrDefault(
      item => item.ContributionId == focusId && item.IsFocus
    ) ?? throw new FamilyCommandException("The focus does not exist.");
    var focus = economy.Contributions.Single(item => item.Id == focusId);
    var minutes = RequiredInt(payload, "minutes");
    Require(minutes >= 1 && minutes <= 1440, "Preparation minutes must be between 1 and 1440.");
    var reward = Round(RequiredDecimal(payload, "reward"));
    Require(reward >= 0, "The preparation reward cannot be negative.");
    var ids = RequiredArray(payload, "ids").Select(RequiredString).ToArray();
    var dates = RequiredArray(payload, "dates").Select(RequiredString).ToArray();
    Require(ids.Length > 0 && ids.Length == dates.Length, "Preparation IDs and dates must match.");
    Require(ids.Distinct(StringComparer.Ordinal).Count() == ids.Length, "Preparation IDs must be unique.");
    var results = new List<ContributionPresentationState>();

    for (var index = 0; index < ids.Length; index += 1)
    {
      var dueDate = ParseDate(dates[index], "The preparation date is invalid.");
      Require(
        dueDate >= focusPresentation.FocusStartDate && dueDate <= focusPresentation.FocusEndDate,
        "A preparation date must be within the focus period."
      );
      var id = new ContributionId(ids[index]);
      economy.AddContribution(
        actorId,
        id,
        new ContributionDefinition(
          $"Vorbereitung: {focus.Title}",
          ContributionKind.School,
          new LadirchenAmount(reward),
          new Percentage(0),
          focus.AssigneeId
        ),
        now
      );
      results.Add(new ContributionPresentationState(
        id,
        $"{minutes} Minuten Vorbereitung für {focus.Title}",
        "🎯",
        false,
        "Vorbereitung",
        dueDate,
        "none",
        false,
        null,
        null,
        null,
        0,
        focusId.Value,
        minutes
      ));
    }

    return results;
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
      false,
      ContributionCategory(payload),
      ContributionDueDate(payload),
      ContributionRecurrence(payload),
      IsVisibleToChildren: IsVisibleToChildren(payload)
    );
  }

  private static bool IsVisibleToChildren(JsonElement payload)
  {
    return OptionalBool(payload, "isVisibleToChildren") ?? true;
  }

  private static bool IsVisibleToChildren(
    FamilyEconomyPresentationState presentation,
    ContributionId contributionId
  )
  {
    return presentation.Contributions.Single(item => item.ContributionId == contributionId)
      .IsVisibleToChildren;
  }

  private static void RequireAdministratorForHiddenContribution(
    Family family,
    FamilyMemberId actorId,
    JsonElement payload
  )
  {
    if (!IsVisibleToChildren(payload))
    {
      family.RequireAdministrator(actorId);
    }
  }

  private static ContributionPresentationState? CreateNextRecurringContribution(
    FamilyEconomy economy,
    FamilyEconomyPresentationState presentation,
    FamilyMemberId? actorId,
    ContributionId sourceId,
    DateOnly localDate,
    DateTimeOffset now
  )
  {
    var sourcePresentation = presentation.Contributions.SingleOrDefault(item => item.ContributionId == sourceId)
      ?? throw new FamilyCommandException("Presentation data for the source contribution is missing.");

    if (sourcePresentation.Recurrence == "none" || sourcePresentation.DueDate is null)
    {
      return null;
    }

    var creatorId = actorId
      ?? throw new InvalidOperationException("A recurring contribution requires a creator.");

    var nextDueDate = sourcePresentation.DueDate.Value;
    do
    {
      nextDueDate = sourcePresentation.Recurrence switch
      {
        "daily" => nextDueDate.AddDays(1),
        "weekly" => nextDueDate.AddDays(7),
        _ => throw new FamilyCommandException("The contribution recurrence is invalid."),
      };
    }
    while (nextDueDate <= localDate);

    var identity = $"{sourceId.Value}|{nextDueDate:yyyy-MM-dd}";
    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(identity));
    var nextId = new ContributionId($"repeat-{Convert.ToHexString(hash.AsSpan(0, 16)).ToLowerInvariant()}");
    economy.ReactivateContribution(creatorId, sourceId, nextId, now);

    return sourcePresentation with
    {
      ContributionId = nextId,
      DueDate = nextDueDate,
      RewardCelebrated = false,
    };
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

    return sourcePresentation with
    {
      ContributionId = copyId,
      RewardCelebrated = false
    };
  }

  private static ContributionPresentationState ReactivateContribution(
    FamilyEconomy economy,
    FamilyEconomyPresentationState presentation,
    FamilyMemberId actorId,
    JsonElement payload,
    DateTimeOffset now
  )
  {
    var sourceId = new ContributionId(RequiredString(payload, "sourceId"));
    var reactivatedId = ContributionId(payload);
    economy.ReactivateContribution(actorId, sourceId, reactivatedId, now);
    var sourcePresentation = presentation.Contributions.SingleOrDefault(item => item.ContributionId == sourceId)
      ?? throw new FamilyCommandException("Presentation data for the source contribution is missing.");

    return sourcePresentation with
    {
      ContributionId = reactivatedId,
      RewardCelebrated = false
    };
  }

  private static ContributionDefinition ContributionDefinition(JsonElement payload)
  {
    var kind = RequiredString(payload, "kind") switch
    {
      "basic" => ContributionKind.Basic,
      "extra" => ContributionKind.Extra,
      "school" => ContributionKind.School,
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

  private static string? ContributionCategory(JsonElement payload)
  {
    var category = OptionalString(payload, "category")?.Trim();

    if (string.IsNullOrEmpty(category))
    {
      return null;
    }

    Require(category.Length <= 80, "The contribution category may contain at most 80 characters.");

    return category;
  }

  private static DateOnly? ContributionDueDate(JsonElement payload)
  {
    var value = OptionalString(payload, "dueDate");

    if (string.IsNullOrWhiteSpace(value))
    {
      return null;
    }

    if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dueDate))
    {
      throw new FamilyCommandException("The contribution due date is invalid.");
    }

    return dueDate;
  }

  private static string ContributionRecurrence(JsonElement payload)
  {
    var recurrence = OptionalString(payload, "recurrence") ?? "none";
    Require(
      recurrence is "none" or "daily" or "weekly",
      "The contribution recurrence is invalid."
    );
    Require(
      recurrence == "none" || ContributionDueDate(payload) is not null,
      "A recurring contribution requires a due date."
    );

    return recurrence;
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

  private static ContributionPresentationState? SettleFocusBonus(
    FamilyEconomy economy,
    FamilyEconomyPresentationState presentation,
    FamilyCommand command,
    FamilyMemberId actorId,
    Contribution approvedContribution,
    int stars,
    DateTimeOffset now,
    ICollection<FamilyUserNotification> notifications
  )
  {
    var approvedPresentation = presentation.Contributions.Single(
      item => item.ContributionId == approvedContribution.Id
    );

    if (approvedPresentation.FocusId is null)
    {
      return null;
    }

    var focusId = new ContributionId(approvedPresentation.FocusId);
    var focusPresentation = presentation.Contributions.SingleOrDefault(
      item => item.ContributionId == focusId && item.IsFocus
    ) ?? throw new FamilyCommandException("The linked focus does not exist.");

    if (focusPresentation.FocusBonusPaid || focusPresentation.FocusCompletionBonus <= 0 || stars != 5)
    {
      return null;
    }

    var preparationIds = presentation.Contributions
      .Where(item => item.FocusId == focusId.Value)
      .Select(item => item.ContributionId)
      .ToHashSet();
    var allCompleted = preparationIds.Count > 0 && economy.Contributions
      .Where(item => preparationIds.Contains(item.Id))
      .All(item => item.Status == ContributionStatus.Approved);

    if (!allCompleted)
    {
      return null;
    }

    var focus = economy.Contributions.Single(item => item.Id == focusId);
    var childId = focus.AssigneeId
      ?? throw new FamilyCommandException("The focus does not have a child.");
    economy.GrantFocusCompletionBonus(
      actorId,
      childId,
      new LadirchenAmount(focusPresentation.FocusCompletionBonus),
      $"5-Sterne-Abschlussbonus: {focus.Title}",
      new TransactionId($"{command.CommandId:N}-focus"),
      now
    );
    notifications.Add(new FamilyUserNotification(
      childId,
      "Fokusziel erreicht",
      $"Du hast den Fokus „{focus.Title}“ mit 5 Sternen abgeschlossen und {focusPresentation.FocusCompletionBonus:0.##} Ladirchen Bonus erhalten.",
      "Focus goal reached",
      $"You completed “{focus.Title}” with five stars and received a {focusPresentation.FocusCompletionBonus:0.##} Ladirchen bonus.",
      "/#/contributions",
      $"focus-bonus-{focusId.Value}"
    ));

    return focusPresentation with
    {
      FocusBonusPaid = true
    };
  }

  private static void RequireNotFocus(
    FamilyEconomyPresentationState presentation,
    ContributionId contributionId
  )
  {
    var item = presentation.Contributions.SingleOrDefault(
      candidate => candidate.ContributionId == contributionId
    );
    Require(item?.IsFocus != true, "A focus is not a completable contribution.");
  }

  private static FamilyEconomyPresentationState UpdatePresentation(
    FamilyEconomyPresentationState presentation,
    FamilyEconomy economy,
    ContributionPresentationState? addedContribution,
    IReadOnlyList<ContributionPresentationState> addedContributions,
    ContributionPresentationState? updatedContribution,
    ContributionPresentationState? updatedFocus,
    ContributionId? celebratedContributionId
  )
  {
    var liveIds = economy.Contributions.Select(item => item.Id).ToHashSet();
    var contributions = presentation.Contributions
      .Where(item => liveIds.Contains(item.ContributionId))
      .Select(item => item.ContributionId == celebratedContributionId
        ? item with
        {
          RewardCelebrated = true
        }
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

    foreach (var item in addedContributions)
    {
      Require(
        contributions.All(existing => existing.ContributionId != item.ContributionId),
        "Presentation data for the contribution already exists."
      );
      contributions.Add(item);
    }

    if (updatedContribution is not null)
    {
      var index = contributions.FindIndex(item => item.ContributionId == updatedContribution.ContributionId);
      Require(index >= 0, "Presentation data for the contribution is missing.");
      contributions[index] = updatedContribution;
    }

    if (updatedFocus is not null)
    {
      var index = contributions.FindIndex(item => item.ContributionId == updatedFocus.ContributionId);
      Require(index >= 0, "Presentation data for the focus is missing.");
      contributions[index] = updatedFocus;
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

  private static DateOnly RequiredDate(JsonElement payload, string propertyName)
  {
    return ParseDate(RequiredString(payload, propertyName), $"The {propertyName} is invalid.");
  }

  private static DateOnly ParseDate(string value, string errorMessage)
  {
    if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
    {
      throw new FamilyCommandException(errorMessage);
    }

    return date;
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
