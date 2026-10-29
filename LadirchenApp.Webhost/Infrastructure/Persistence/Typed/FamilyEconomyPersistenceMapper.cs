using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

internal static partial class FamilyEconomyPersistenceMapper
{
  public static FamilyEconomy ToDomain(FamilyEconomyRootEntity entity, Family family)
  {
    ArgumentNullException.ThrowIfNull(entity);
    ArgumentNullException.ThrowIfNull(family);
    // Reservations are authoritative pending requests, not a second persisted wallet balance.
    var pendingReservations = entity.RedemptionRequests
      .Where(item => (RedemptionStatus)item.Status == RedemptionStatus.Pending)
      .GroupBy(item => item.RequesterMemberId, StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.Sum(item => item.Price), StringComparer.Ordinal);
    var state = new FamilyEconomyState(
      new FamilyId(entity.FamilyId),
      new ContributionSettings(new Percentage(entity.PerfectRatingBonusPercent)),
      entity.Wallets
        .OrderBy(item => item.MemberId, StringComparer.Ordinal)
        .Select(item => new MemberWalletState(
          new FamilyMemberId(item.MemberId),
          item.Balance,
          pendingReservations.GetValueOrDefault(item.MemberId)
        ))
        .ToArray(),
      ToContributionStates(entity),
      entity.UsedContributionIds
        .OrderBy(item => item.ContributionId, StringComparer.Ordinal)
        .Select(item => new ContributionId(item.ContributionId))
        .ToArray(),
      ToPromotionStates(entity),
      entity.AppliedOperations
        .OrderBy(item => item.OperationId, StringComparer.Ordinal)
        .Select(item => new TransactionId(item.OperationId))
        .ToArray(),
      ToLedgerStates(entity),
      ToSavingGoalStates(entity),
      ToSavingsVoucherStates(entity),
      ToGuardianGiftStates(entity),
      entity.SettledInterestPeriods
        .OrderBy(item => item.IsoYear)
        .ThenBy(item => item.IsoWeek)
        .Select(item => new IsoWeekPeriod(item.IsoYear, item.IsoWeek))
        .ToArray(),
      ToShopRewardStates(entity),
      ToRedemptionRequestStates(entity)
    );

    return FamilyEconomy.Reconstitute(family, state);
  }

  public static FamilyEconomyPresentationState ToPresentation(FamilyEconomyRootEntity entity)
  {
    ArgumentNullException.ThrowIfNull(entity);

    return new FamilyEconomyPresentationState(
      entity.ContributionPresentations
        .OrderBy(item => item.ContributionId, StringComparer.Ordinal)
        .Select(item => new ContributionPresentationState(
          new ContributionId(item.ContributionId),
          item.Description,
          item.Icon,
          item.RewardCelebrated
        )),
      entity.SavingGoalPresentations
        .OrderBy(item => item.GoalId, StringComparer.Ordinal)
        .Select(item => new SavingGoalPresentationState(
          new SavingGoalId(item.GoalId),
          item.Icon
        )),
      entity.ShopRewardPresentations
        .OrderBy(item => item.RewardId, StringComparer.Ordinal)
        .Select(item => new ShopRewardPresentationState(
          new ShopRewardId(item.RewardId),
          item.Description,
          item.Icon,
          item.Conditions
        )),
      entity.GuardianGiftPresentations
        .OrderBy(item => item.GiftId, StringComparer.Ordinal)
        .Select(item => new GuardianGiftPresentationState(
          new GuardianGiftId(item.GiftId),
          item.SenderName,
          item.GoalTitle
        ))
    );
  }

  public static FamilyEconomyRootEntity CreateEntity(
    FamilyEconomy economy,
    FamilyEconomyPresentationState presentation
  )
  {
    ArgumentNullException.ThrowIfNull(economy);
    ArgumentNullException.ThrowIfNull(presentation);
    var entity = new FamilyEconomyRootEntity();
    Apply(entity, economy, presentation);

    return entity;
  }

  public static void Apply(
    FamilyEconomyRootEntity entity,
    FamilyEconomy economy,
    FamilyEconomyPresentationState presentation
  )
  {
    ArgumentNullException.ThrowIfNull(entity);
    ArgumentNullException.ThrowIfNull(economy);
    ArgumentNullException.ThrowIfNull(presentation);
    var state = economy.CaptureState();
    entity.FamilyId = state.Id.Value;
    entity.PerfectRatingBonusPercent = state.ContributionSettings.PerfectRatingBonus.Value;
    ClearCollections(entity);
    AddWalletsAndHistory(entity, state);
    AddContributions(entity, state);
    AddSavings(entity, state);
    AddRewardShop(entity, state);
    AddPresentations(entity, presentation);
  }

  private static IReadOnlyList<LedgerEntryState> ToLedgerStates(FamilyEconomyRootEntity entity)
  {
    return entity.LedgerEntries
      .OrderBy(item => item.OccurredAtUtc)
      .ThenBy(item => item.EntryId)
      .Select(item => new LedgerEntryState(
        item.EntryId,
        new TransactionId(item.OperationId),
        new FamilyMemberId(item.MemberId),
        OptionalSavingGoalId(item.GoalId),
        (LedgerKind)item.Kind,
        item.Amount,
        new LedgerSourceState((LedgerSourceKind)item.SourceKind, item.SourceId),
        item.Reason,
        item.OccurredAtUtc,
        OptionalMemberId(item.InitiatorMemberId)
      ))
      .ToArray();
  }

  private static void AddWalletsAndHistory(
    FamilyEconomyRootEntity entity,
    FamilyEconomyState state
  )
  {
    foreach (var wallet in state.Wallets)
    {
      entity.Wallets.Add(new FamilyEconomyWalletEntity
      {
        FamilyId = state.Id.Value,
        MemberId = wallet.MemberId.Value,
        Balance = wallet.Balance,
      });
    }

    foreach (var operationId in state.AppliedOperationIds)
    {
      entity.AppliedOperations.Add(new EconomyAppliedOperationEntity
      {
        FamilyId = state.Id.Value,
        OperationId = operationId.Value,
      });
    }

    foreach (var entry in state.Ledger)
    {
      entity.LedgerEntries.Add(new EconomyLedgerEntryEntity
      {
        FamilyId = state.Id.Value,
        EntryId = entry.EntryId,
        OperationId = entry.OperationId.Value,
        MemberId = entry.MemberId.Value,
        GoalId = entry.GoalId?.Value,
        Kind = (int)entry.Kind,
        Amount = entry.Amount,
        SourceKind = (int)entry.Source.Kind,
        SourceId = entry.Source.Id,
        Reason = entry.Reason,
        OccurredAtUtc = entry.OccurredAtUtc,
        InitiatorMemberId = entry.InitiatorId?.Value,
      });
    }
  }

  private static void AddPresentations(
    FamilyEconomyRootEntity entity,
    FamilyEconomyPresentationState presentation
  )
  {
    var familyId = entity.FamilyId;

    foreach (var item in presentation.Contributions)
    {
      entity.ContributionPresentations.Add(new ContributionPresentationEntity
      {
        FamilyId = familyId,
        ContributionId = item.ContributionId.Value,
        Description = item.Description,
        Icon = item.Icon,
        RewardCelebrated = item.RewardCelebrated,
      });
    }

    foreach (var item in presentation.SavingGoals)
    {
      entity.SavingGoalPresentations.Add(new SavingGoalPresentationEntity
      {
        FamilyId = familyId,
        GoalId = item.GoalId.Value,
        Icon = item.Icon,
      });
    }

    foreach (var item in presentation.ShopRewards)
    {
      entity.ShopRewardPresentations.Add(new ShopRewardPresentationEntity
      {
        FamilyId = familyId,
        RewardId = item.RewardId.Value,
        Description = item.Description,
        Icon = item.Icon,
        Conditions = item.Conditions,
      });
    }

    foreach (var item in presentation.GuardianGifts)
    {
      entity.GuardianGiftPresentations.Add(new GuardianGiftPresentationEntity
      {
        FamilyId = familyId,
        GiftId = item.GiftId.Value,
        SenderName = item.SenderName,
        GoalTitle = item.GoalTitle,
      });
    }
  }

  private static void ClearCollections(FamilyEconomyRootEntity entity)
  {
    entity.Wallets.Clear();
    entity.Contributions.Clear();
    entity.UsedContributionIds.Clear();
    entity.Promotions.Clear();
    entity.SavingGoals.Clear();
    entity.SavingsVouchers.Clear();
    entity.GuardianGifts.Clear();
    entity.SettledInterestPeriods.Clear();
    entity.ShopRewards.Clear();
    entity.RedemptionRequests.Clear();
    entity.AppliedOperations.Clear();
    entity.LedgerEntries.Clear();
    entity.ContributionPresentations.Clear();
    entity.SavingGoalPresentations.Clear();
    entity.ShopRewardPresentations.Clear();
    entity.GuardianGiftPresentations.Clear();
  }

  private static FamilyMemberId? OptionalMemberId(string? value)
  {
    return value is null ? null : new FamilyMemberId(value);
  }

  private static SavingGoalId? OptionalSavingGoalId(string? value)
  {
    return value is null ? null : new SavingGoalId(value);
  }
}
