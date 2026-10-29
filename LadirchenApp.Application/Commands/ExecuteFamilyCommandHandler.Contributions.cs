using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

public sealed partial class ExecuteFamilyCommandHandler
{
  private async Task<FamilyCommandResult> ExecuteContributionsAsync(
    FamilyCommand command,
    FamilyAggregateSnapshot profile,
    JsonObject profileState,
    JsonObject payload,
    CancellationToken cancellationToken
  )
  {
    var family = FamilyProfileSnapshotMapper.Rehydrate(command.FamilyId, profileState);
    _ = family.ActiveMember(command.ActorId);
    var contributions = await RequireSnapshotAsync(command.FamilyId, "contributions", cancellationToken);
    var contributionState = RequireObject(contributions.State);
    FamilyAggregateSnapshot? savings = null;
    JsonObject? savingsState = null;

    if (command.CommandType == "contributions.approve")
    {
      savings = await RequireSnapshotAsync(command.FamilyId, "savings", cancellationToken);
      savingsState = RequireObject(savings.State);
    }

    var economy = FamilyEconomySnapshotMapper.RehydrateContributions(
      family,
      contributionState,
      savingsState
    );
    var now = _timeProvider.GetUtcNow();
    var operationId = new TransactionId(command.CommandId.ToString("N", CultureInfo.InvariantCulture));
    ContributionPresentation? addedContribution = null;
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
        economy.SubmitContribution(
          command.ActorId,
          new ContributionId(RequiredString(payload, "id")),
          now
        );
        break;
      case "contributions.claim":
        economy.ClaimContribution(
          command.ActorId,
          new ContributionId(RequiredString(payload, "id")),
          now
        );
        break;
      case "contributions.assign":
        economy.AssignContribution(
          command.ActorId,
          new ContributionId(RequiredString(payload, "id")),
          CreateOptionalMemberId(StringValue(payload, "memberId")),
          now
        );
        break;
      case "contributions.approve":
        _ = economy.ApproveContribution(
          command.ActorId,
          new ContributionId(RequiredString(payload, "id")),
          new StarRating(RequiredInt(payload, "stars")),
          operationId,
          now
        );
        break;
      case "contributions.return":
        economy.ReturnContribution(
          command.ActorId,
          new ContributionId(RequiredString(payload, "id")),
          now
        );
        break;
      case "contributions.add":
        addedContribution = AddContribution(economy, command.ActorId, payload, now);
        break;
      case "contributions.delete":
        economy.DeleteContribution(
          command.ActorId,
          new ContributionId(RequiredString(payload, "id")),
          now
        );
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
          new ContributionId(RequiredString(payload, "id")),
          RequireArray(payload, "partnerIds")
            .Select(RequiredString)
            .Select(value => new FamilyMemberId(value)),
          now
        );
        break;
      case "contributions.add-promotion":
        AddPromotion(economy, command.ActorId, payload, now);
        break;
      case "contributions.acknowledge-reward":
        celebratedContributionId = ValidateRewardAcknowledgement(economy, command.ActorId, payload);
        break;
      default:
        throw new FamilyCommandException("The family command is not supported.");
    }

    var updatedContributionState = FamilyEconomySnapshotMapper.WriteContributions(
      contributionState,
      economy,
      addedContribution
    );
    if (celebratedContributionId is not null)
    {
      FamilyEconomySnapshotMapper.MarkContributionRewardCelebrated(
        updatedContributionState,
        celebratedContributionId
      );
    }

    var pendingSaves = new List<PendingFamilyAggregateSave>
    {
      CreateSave(contributions, updatedContributionState, command.ActorId),
    };

    if (savings is not null && savingsState is not null)
    {
      var updatedSavingsState = FamilyEconomySnapshotMapper.WriteWalletBalances(savingsState, economy);
      pendingSaves.Add(CreateSave(savings, updatedSavingsState, command.ActorId));
    }

    pendingSaves.Add(
      await CreateProgressionSaveAsync(
        command.FamilyId,
        profileState,
        updatedContributionState,
        command.ActorId,
        cancellationToken
      )
    );
    var guards = new[]
    {
      new FamilyAggregateRevisionGuard(profile.FamilyId, profile.AggregateType, profile.Revision),
    };
    var changed = await _repository.SaveBatchAsync(pendingSaves, guards, cancellationToken);

    return new FamilyCommandResult(changed);
  }

  private static ContributionPresentation AddContribution(
    FamilyEconomy economy,
    FamilyMemberId actorId,
    JsonObject payload,
    DateTimeOffset now
  )
  {
    var id = new ContributionId(RequiredString(payload, "id"));
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
        CreateOptionalMemberId(StringValue(payload, "assigneeId"))
      ),
      now
    );

    return new ContributionPresentation(
      id,
      RequiredString(payload, "description"),
      RequiredString(payload, "icon")
    );
  }

  private static void AddPromotion(
    FamilyEconomy economy,
    FamilyMemberId actorId,
    JsonObject payload,
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
    JsonObject payload
  )
  {
    var id = new ContributionId(RequiredString(payload, "id"));
    var contribution = economy.Contributions.SingleOrDefault(item => item.Id == id)
      ?? throw new FamilyCommandException("The contribution does not exist.");
    Require(
      contribution.AssigneeId == actorId && contribution.Status == ContributionStatus.Approved,
      "Only the recipient may acknowledge an approved reward."
    );

    return id;
  }

  private static FamilyMemberId? CreateOptionalMemberId(string? value)
  {
    return string.IsNullOrWhiteSpace(value) ? null : new FamilyMemberId(value);
  }
}
