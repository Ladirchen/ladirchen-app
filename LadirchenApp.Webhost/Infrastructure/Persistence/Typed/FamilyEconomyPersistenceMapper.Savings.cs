using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

internal static partial class FamilyEconomyPersistenceMapper
{
  private static IReadOnlyList<SavingGoalState> ToSavingGoalStates(
    FamilyEconomyRootEntity entity
  )
  {
    return entity.SavingGoals
      .OrderBy(item => item.GoalId, StringComparer.Ordinal)
      .Select(item => new SavingGoalState(
        new SavingGoalId(item.GoalId),
        OptionalMemberId(item.OwnerMemberId),
        item.Title,
        item.Target,
        (GoalVisibility)item.Visibility,
        item.CreatedAt,
        item.Participants
          .OrderBy(participant => participant.MemberId, StringComparer.Ordinal)
          .Select(participant => new FamilyMemberId(participant.MemberId))
          .ToArray(),
        item.Saved,
        item.StarterBonus,
        item.InterestEarned,
        item.RefundableContributions
          .OrderBy(refund => refund.MemberId, StringComparer.Ordinal)
          .Select(refund => new RefundableContributionState(
            new FamilyMemberId(refund.MemberId),
            refund.Amount
          ))
          .ToArray(),
        ToCancellationState(item.CancellationRequest),
        item.Cheered,
        item.Cancelled,
        ToInterestPeriod(item),
        item.LastInterestRate is null ? null : new Percentage(item.LastInterestRate.Value)
      ))
      .ToArray();
  }

  private static IReadOnlyList<SavingsVoucherState> ToSavingsVoucherStates(
    FamilyEconomyRootEntity entity
  )
  {
    return entity.SavingsVouchers
      .OrderBy(item => item.GoalId, StringComparer.Ordinal)
      .Select(item => new SavingsVoucherState(
        new SavingGoalId(item.GoalId),
        item.Amount,
        OptionalMemberId(item.OwnerMemberId),
        item.Participants
          .OrderBy(participant => participant.MemberId, StringComparer.Ordinal)
          .Select(participant => new FamilyMemberId(participant.MemberId))
          .ToArray(),
        item.Consents
          .OrderBy(consent => consent.MemberId, StringComparer.Ordinal)
          .Select(consent => new FamilyMemberId(consent.MemberId))
          .ToArray(),
        item.RedeemedAt
      ))
      .ToArray();
  }

  private static IReadOnlyList<GuardianGoalGiftState> ToGuardianGiftStates(
    FamilyEconomyRootEntity entity
  )
  {
    return entity.GuardianGifts
      .OrderBy(item => item.StorageId)
      .Select(item => new GuardianGoalGiftState(
        item.GiftId is null ? null : new GuardianGiftId(item.GiftId),
        new FamilyMemberId(item.OwnerMemberId),
        OptionalMemberId(item.GuardianMemberId),
        item.Amount,
        OptionalSavingGoalId(item.GoalId),
        (GiftDestination)item.Destination
      ))
      .ToArray();
  }

  private static void AddSavings(
    FamilyEconomyRootEntity entity,
    FamilyEconomyState state
  )
  {
    foreach (var goal in state.SavingGoals)
    {
      var goalEntity = new EconomySavingGoalEntity
      {
        FamilyId = state.Id.Value,
        GoalId = goal.Id.Value,
        OwnerMemberId = goal.OwnerId?.Value,
        Title = goal.Title,
        Target = goal.Target,
        Saved = goal.Saved,
        StarterBonus = goal.StarterBonus,
        InterestEarned = goal.InterestEarned,
        Visibility = (int)goal.Visibility,
        Cheered = goal.Cheered,
        Cancelled = goal.Cancelled,
        CreatedAt = goal.CreatedAt,
        LastInterestYear = goal.LastInterestPeriod?.Year,
        LastInterestWeek = goal.LastInterestPeriod?.Week,
        LastInterestRate = goal.LastInterestRate?.Value,
      };

      foreach (var participantId in goal.ParticipantIds)
      {
        goalEntity.Participants.Add(new EconomySavingGoalParticipantEntity
        {
          FamilyId = state.Id.Value,
          GoalId = goal.Id.Value,
          MemberId = participantId.Value,
        });
      }

      foreach (var refund in goal.RefundableContributions)
      {
        goalEntity.RefundableContributions.Add(new EconomySavingGoalRefundEntity
        {
          FamilyId = state.Id.Value,
          GoalId = goal.Id.Value,
          MemberId = refund.MemberId.Value,
          Amount = refund.Amount,
        });
      }

      if (goal.CancellationRequest is not null)
      {
        var cancellation = new EconomyGoalCancellationEntity
        {
          FamilyId = state.Id.Value,
          GoalId = goal.Id.Value,
          RequestedByMemberId = goal.CancellationRequest.RequestedByMemberId.Value,
        };

        foreach (var memberId in goal.CancellationRequest.ConsentedMemberIds)
        {
          cancellation.Consents.Add(new EconomyGoalCancellationConsentEntity
          {
            FamilyId = state.Id.Value,
            GoalId = goal.Id.Value,
            MemberId = memberId.Value,
          });
        }

        goalEntity.CancellationRequest = cancellation;
      }

      entity.SavingGoals.Add(goalEntity);
    }

    foreach (var voucher in state.SavingsVouchers)
    {
      var voucherEntity = new EconomySavingsVoucherEntity
      {
        FamilyId = state.Id.Value,
        GoalId = voucher.GoalId.Value,
        Amount = voucher.Amount,
        OwnerMemberId = voucher.OwnerId?.Value,
        RedeemedAt = voucher.RedeemedAt,
      };

      foreach (var memberId in voucher.ParticipantIds)
      {
        voucherEntity.Participants.Add(new EconomySavingsVoucherParticipantEntity
        {
          FamilyId = state.Id.Value,
          GoalId = voucher.GoalId.Value,
          MemberId = memberId.Value,
        });
      }

      foreach (var memberId in voucher.ConsentedMemberIds)
      {
        voucherEntity.Consents.Add(new EconomySavingsVoucherConsentEntity
        {
          FamilyId = state.Id.Value,
          GoalId = voucher.GoalId.Value,
          MemberId = memberId.Value,
        });
      }

      entity.SavingsVouchers.Add(voucherEntity);
    }

    foreach (var gift in state.GuardianGifts)
    {
      entity.GuardianGifts.Add(new EconomyGuardianGiftEntity
      {
        FamilyId = state.Id.Value,
        StorageId = Guid.NewGuid(),
        GiftId = gift.Id?.Value,
        OwnerMemberId = gift.OwnerId.Value,
        GuardianMemberId = gift.GuardianId?.Value,
        Amount = gift.Amount,
        GoalId = gift.GoalId?.Value,
        Destination = (int)gift.Destination,
      });
    }

    foreach (var period in state.SettledInterestPeriods)
    {
      entity.SettledInterestPeriods.Add(new EconomySettledInterestPeriodEntity
      {
        FamilyId = state.Id.Value,
        IsoYear = period.Year,
        IsoWeek = period.Week,
      });
    }
  }

  private static GoalCancellationRequestState? ToCancellationState(
    EconomyGoalCancellationEntity? entity
  )
  {
    if (entity is null)
    {
      return null;
    }

    return new GoalCancellationRequestState(
      new FamilyMemberId(entity.RequestedByMemberId),
      entity.Consents
        .OrderBy(item => item.MemberId, StringComparer.Ordinal)
        .Select(item => new FamilyMemberId(item.MemberId))
        .ToArray()
    );
  }

  private static IsoWeekPeriod? ToInterestPeriod(EconomySavingGoalEntity entity)
  {
    if (entity.LastInterestYear is null && entity.LastInterestWeek is null)
    {
      return null;
    }

    if (entity.LastInterestYear is null || entity.LastInterestWeek is null)
    {
      throw new InvalidOperationException("A persisted savings interest period must be complete.");
    }

    return new IsoWeekPeriod(entity.LastInterestYear.Value, entity.LastInterestWeek.Value);
  }
}
