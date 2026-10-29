using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Avatar;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal sealed class ProfileFamilyCommandHandler : FamilyCommandAreaHandler
{
  private readonly FamilyProgressionApplicationService _progression;

  public ProfileFamilyCommandHandler(
    IFamilyAggregateSnapshotRepository repository,
    TimeProvider timeProvider,
    FamilyProgressionApplicationService progression
  )
    : base(repository, timeProvider, "profile.")
  {
    _progression = progression;
  }

  public override async Task<FamilyCommandResult> HandleAsync(
    FamilyCommand command,
    FamilyAggregateSnapshot profile,
    JsonObject profileState,
    JsonObject payload,
    CancellationToken cancellationToken
  )
  {
    var refreshProgression = false;
    var now = _timeProvider.GetUtcNow();

    var family = FamilyProfileSnapshotMapper.Rehydrate(command.FamilyId, profileState);

    switch (command.CommandType)
    {
      case "profile.set-weekly-participation":
        SetWeeklyParticipation(family, command.ActorId, payload, now);
        refreshProgression = true;
        break;
      case "profile.save-own-appearance":
        SaveOwnAppearance(family, command.ActorId, profileState, payload);
        break;
      case "profile.set-own-nickname":
        SetOwnNickname(family, command.ActorId, payload, now);
        break;
      case "profile.invite-guardian":
        InviteGuardian(family, command.ActorId, payload, now);
        break;
      case "profile.set-guardian-access":
        SetGuardianAccess(family, command.ActorId, payload, now);
        break;
      case "profile.complete-setup":
        CompleteFamilySetup(family, profileState, command.ActorId, payload, now);
        refreshProgression = true;
        break;
      default:
        throw new FamilyCommandException("The family-profile command is not supported.");
    }

    FamilyProfileSnapshotMapper.WriteTo(profileState, family);

    var saves = new List<PendingFamilyAggregateSave> { CreateSave(profile, profileState, command.ActorId) };
    var revisionGuards = new List<FamilyAggregateRevisionGuard>();

    if (refreshProgression)
    {
      var contributions = await RequireSnapshotAsync(command.FamilyId, "contributions", cancellationToken);
      revisionGuards.Add(FamilyAggregateRevisionGuard.FromSnapshot(contributions));
      saves.Add(
        await _progression.CreateSaveAsync(
          command.FamilyId,
          profileState,
          RequireObject(contributions.State),
          command.ActorId,
          cancellationToken
        )
      );
    }

    var changed = await _repository.SaveBatchAsync(saves, revisionGuards, cancellationToken);

    return new FamilyCommandResult(changed);
  }

  private static void SetWeeklyParticipation(
    Family family,
    FamilyMemberId actorId,
    JsonObject payload,
    DateTimeOffset now
  )
  {
    family.SetGuardianWeeklyGoalParticipation(
      actorId,
      new FamilyMemberId(RequiredString(payload, "memberId")),
      RequiredBool(payload, "participates"),
      now
    );
  }

  private static void SaveOwnAppearance(
    Family family,
    FamilyMemberId actorId,
    JsonObject profileState,
    JsonObject payload
  )
  {
    var appearance = RequireObject(payload, "appearance");
    var member = family.ActiveMember(actorId);
    var profile = member.Role == MemberRole.Child ? AvatarProfile.Child : AvatarProfile.Guardian;
    var service = new AvatarAppearanceService(new AvatarCatalog());
    service.Save(
      actorId,
      actorId,
      actorId,
      profile,
      FamilyProfileSnapshotMapper.ReadAvatarAppearance(appearance)
    );
    var actor = FamilyProfileSnapshotMapper.MemberPresentation(profileState, actorId);
    actor["appearance"] = appearance.DeepClone();
  }

  private static void SetOwnNickname(
    Family family,
    FamilyMemberId actorId,
    JsonObject payload,
    DateTimeOffset now
  )
  {
    family.SaveOwnNickname(actorId, StringValue(payload, "nickname") ?? string.Empty, now);
  }

  private static void InviteGuardian(
    Family family,
    FamilyMemberId actorId,
    JsonObject payload,
    DateTimeOffset now
  )
  {
    family.InviteGuardian(
      actorId,
      new FamilyMemberId(RequiredString(payload, "id")),
      RequiredString(payload, "name"),
      RequiredString(payload, "email"),
      FamilyProfileSnapshotMapper.ParseGuardianAccess(RequiredString(payload, "guardianAccess")),
      now
    );
  }

  private static void SetGuardianAccess(
    Family family,
    FamilyMemberId actorId,
    JsonObject payload,
    DateTimeOffset now
  )
  {
    family.ChangeGuardianAccess(
      actorId,
      new FamilyMemberId(RequiredString(payload, "memberId")),
      FamilyProfileSnapshotMapper.ParseGuardianAccess(RequiredString(payload, "guardianAccess")),
      now
    );
  }

  private static void CompleteFamilySetup(
    Family family,
    JsonObject profile,
    FamilyMemberId actorId,
    JsonObject payload,
    DateTimeOffset now
  )
  {
    var members = RequireArray(payload, "members");
    var pets = RequireArray(payload, "pets");
    var memberStates = FamilyProfileSnapshotMapper.ReadMembers(members);
    var petStates = FamilyProfileSnapshotMapper.ReadPets(pets);
    var memberPresentations = FamilyProfileSnapshotMapper.CreateSetupMemberPresentations(
      profile,
      members
    );
    var petPresentations = FamilyProfileSnapshotMapper.CreateSetupPetPresentations(pets);

    family.CompleteSetup(actorId, memberStates, petStates, now);
    profile["members"] = memberPresentations;
    profile["pets"] = petPresentations;
  }
}
