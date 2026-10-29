using System;
using System.Linq;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

internal static class FamilyProgressionPersistenceMapper
{
  public static FamilyHouse ToDomain(FamilyProgressionRootEntity entity, Family family)
  {
    ArgumentNullException.ThrowIfNull(entity);
    ArgumentNullException.ThrowIfNull(family);
    var state = new FamilyHouseState(
      new FamilyId(entity.FamilyId),
      entity.StoredLevel,
      entity.CompletedWeeklyStreak,
      entity.CurrentWeekTarget,
      new Percentage(entity.MaximumSavingsInterestRate),
      entity.Participants
        .OrderBy(item => item.MemberId, StringComparer.Ordinal)
        .Select(item => new ParticipantProgressState(
          new FamilyMemberId(item.MemberId),
          item.StoredWeeklyStreak
        )),
      entity.ClosedWeeks
        .OrderBy(item => item.IsoYear)
        .ThenBy(item => item.IsoWeek)
        .Select(item => new ClosedFamilyWeekState(
          new IsoWeekPeriod(item.IsoYear, item.IsoWeek),
          (FamilyWeekOutcome)item.Outcome,
          item.EvaluatedAt,
          item.MemberEvaluations
            .OrderBy(evaluation => evaluation.MemberId, StringComparer.Ordinal)
            .Select(ToEvaluation)
        ))
    );

    return FamilyHouse.Reconstitute(family, state);
  }

  public static FamilyProgressionRootEntity CreateEntity(FamilyHouse progression)
  {
    ArgumentNullException.ThrowIfNull(progression);
    var entity = new FamilyProgressionRootEntity();
    Apply(entity, progression);

    return entity;
  }

  public static void Apply(FamilyProgressionRootEntity entity, FamilyHouse progression)
  {
    ArgumentNullException.ThrowIfNull(entity);
    ArgumentNullException.ThrowIfNull(progression);
    var state = progression.CaptureState();
    entity.FamilyId = state.Id.Value;
    entity.StoredLevel = state.StoredLevel;
    entity.CompletedWeeklyStreak = state.CompletedWeeklyStreak;
    entity.CurrentWeekTarget = state.CurrentWeekTarget;
    entity.MaximumSavingsInterestRate = state.MaximumSavingsInterestRate.Value;
    entity.Participants.Clear();
    entity.ClosedWeeks.Clear();

    foreach (var participant in state.Participants)
    {
      entity.Participants.Add(new FamilyProgressionParticipantEntity
      {
        FamilyId = state.Id.Value,
        MemberId = participant.MemberId.Value,
        StoredWeeklyStreak = participant.StoredWeeklyStreak,
      });
    }

    foreach (var week in state.ClosedWeeks)
    {
      var weekEntity = new ClosedFamilyWeekEntity
      {
        FamilyId = state.Id.Value,
        IsoYear = week.Period.Year,
        IsoWeek = week.Period.Week,
        Outcome = (int)week.Outcome,
        EvaluatedAt = week.EvaluatedAt,
      };

      foreach (var evaluation in week.MemberEvaluations)
      {
        weekEntity.MemberEvaluations.Add(new ClosedFamilyWeekEvaluationEntity
        {
          FamilyId = state.Id.Value,
          IsoYear = week.Period.Year,
          IsoWeek = week.Period.Week,
          MemberId = evaluation.MemberId.Value,
          StreakDays = evaluation.Participation.StreakDays,
          CompletionPercent = evaluation.Participation.EffectiveCompletionPercent,
          AverageBasicRating = evaluation.Participation.AverageBasicRating,
          UncappedParticipationValue = evaluation.Value.Uncapped,
          CappedParticipationValue = evaluation.Value.Capped,
          LadiStageId = evaluation.Stage.Id,
          LadiStagePosition = evaluation.Stage.Position,
        });
      }

      entity.ClosedWeeks.Add(weekEntity);
    }
  }

  private static PersonalLadiEvaluation ToEvaluation(ClosedFamilyWeekEvaluationEntity entity)
  {
    return new PersonalLadiEvaluation(
      new FamilyMemberId(entity.MemberId),
      entity.IsoYear,
      entity.IsoWeek,
      new PersonalParticipation(
        entity.StreakDays,
        entity.CompletionPercent,
        entity.AverageBasicRating
      ),
      new ParticipationValue(
        entity.UncappedParticipationValue,
        entity.CappedParticipationValue
      ),
      new LadiStage(entity.LadiStageId, entity.LadiStagePosition)
    );
  }
}
