using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Avatar;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Family.Setup;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal sealed class ProfileFamilyCommandHandler : TypedFamilyCommandAreaHandler
{
  private readonly IFamilyEconomyRepository _economyRepository;
  private readonly IHouseCustomizationRepository _houseRepository;
  private readonly IFamilyProgressionRepository _progressionRepository;

  public ProfileFamilyCommandHandler(
    IFamilyEconomyRepository economyRepository,
    IFamilyProgressionRepository progressionRepository,
    IHouseCustomizationRepository houseRepository,
    ITypedAggregateUnitOfWork unitOfWork,
    TimeProvider timeProvider
  )
    : base(unitOfWork, timeProvider, "profile.")
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
    var payload = RequiredObject(command.Payload);
    var refreshProgression = false;
    var invalidatedReadModels = new HashSet<string>(StringComparer.Ordinal)
    {
      "family-profile",
    };
    var now = TimeProvider.GetUtcNow();

    if (command.CommandType == "profile.complete-setup")
    {
      return await CompleteSetupAsync(
        command,
        loadedFamily,
        payload,
        now,
        cancellationToken
      );
    }

    switch (command.CommandType)
    {
      case "profile.set-weekly-participation":
        family.SetGuardianWeeklyGoalParticipation(
          command.ActorId,
          new FamilyMemberId(RequiredString(payload, "memberId")),
          RequiredBool(payload, "participates"),
          now
        );
        refreshProgression = true;
        invalidatedReadModels.Add("family-progression");
        invalidatedReadModels.Add("savings");
        invalidatedReadModels.Add("home-customization");
        break;
      case "profile.save-own-appearance":
        family.SaveOwnAppearance(
          command.ActorId,
          ReadAvatarAppearance(RequiredObject(payload, "appearance")),
          now
        );
        break;
      case "profile.set-own-nickname":
        family.SaveOwnNickname(
          command.ActorId,
          OptionalString(payload, "nickname") ?? string.Empty,
          now
        );
        break;
      case "profile.invite-guardian":
        family.InviteGuardian(
          command.ActorId,
          new FamilyMemberId(RequiredString(payload, "id")),
          RequiredString(payload, "name"),
          RequiredString(payload, "email"),
          ParseGuardianAccess(RequiredString(payload, "guardianAccess")),
          now
        );
        break;
      case "profile.set-guardian-access":
        family.ChangeGuardianAccess(
          command.ActorId,
          new FamilyMemberId(RequiredString(payload, "memberId")),
          ParseGuardianAccess(RequiredString(payload, "guardianAccess")),
          now
        );
        invalidatedReadModels.Add("savings");
        invalidatedReadModels.Add("reward-shop");
        break;
      default:
        throw new FamilyCommandException("The family-profile command is not supported.");
    }

    LoadedAggregate<FamilyHouse>? loadedProgression = null;

    if (refreshProgression)
    {
      loadedProgression = await _progressionRepository.LoadAsync(family, cancellationToken)
        ?? throw new FamilyCommandException(
          "The family-progression aggregate has not been initialized."
        );
    }

    var result = await UnitOfWork.SaveAsync(
      new TypedAggregateSaveBatch(
        family.Id,
        command.ActorId,
        family: new PendingFamilySave(family, loadedFamily.Revision),
        familyProgression: loadedProgression is null
          ? null
          : new PendingFamilyProgressionSave(
            loadedProgression.Root,
            loadedProgression.Revision
          )
      ),
      cancellationToken
    );
    var changes = new List<FamilyAggregateChange>();
    var savedFamily = result.Family
      ?? throw new InvalidOperationException("The family-profile command did not save the family aggregate.");

    foreach (var readModel in invalidatedReadModels)
    {
      var source = readModel is "family-progression" or "savings" or "home-customization"
        ? result.FamilyProgression
        : null;
      changes.Add(
        Change(
          family.Id,
          readModel,
          source?.Revision ?? savedFamily.Revision,
          source?.Audit ?? savedFamily.Audit
        )
      );
    }

    return new FamilyCommandResult(changes);
  }

  private async Task<FamilyCommandResult> CompleteSetupAsync(
    FamilyCommand command,
    LoadedAggregate<Family> loadedFamily,
    JsonElement payload,
    DateTimeOffset now,
    CancellationToken cancellationToken
  )
  {
    var family = loadedFamily.Root;
    var loadedEconomy = await _economyRepository.LoadAsync(family, cancellationToken)
      ?? throw new FamilyCommandException(
        "The family-economy aggregate has not been initialized."
      );
    var loadedProgression = await _progressionRepository.LoadAsync(family, cancellationToken)
      ?? throw new FamilyCommandException(
        "The family-progression aggregate has not been initialized."
      );
    var loadedHouse = await _houseRepository.LoadAsync(
      family,
      loadedProgression.Root.StoredLevel,
      cancellationToken
    ) ?? throw new FamilyCommandException(
      "The house-customization aggregate has not been initialized."
    );

    family.CompleteSetup(
      command.ActorId,
      ReadMembers(payload),
      ReadPets(payload),
      now
    );
    var economy = loadedEconomy.Root.SynchronizeMembers(family);
    loadedProgression.Root.SynchronizeParticipants(family);
    loadedHouse.Root.SynchronizeFamilyEntities(family);
    var result = await UnitOfWork.SaveAsync(
      new TypedAggregateSaveBatch(
        family.Id,
        command.ActorId,
        family: new PendingFamilySave(family, loadedFamily.Revision),
        familyEconomy: new PendingFamilyEconomySave(
          economy,
          loadedEconomy.Presentation,
          loadedEconomy.Revision
        ),
        familyProgression: new PendingFamilyProgressionSave(
          loadedProgression.Root,
          loadedProgression.Revision
        ),
        houseCustomization: new PendingHouseCustomizationSave(
          loadedHouse.Root,
          loadedHouse.Revision
        )
      ),
      cancellationToken
    );
    var savedFamily = result.Family
      ?? throw new InvalidOperationException("The setup command did not save the family aggregate.");
    var savedEconomy = result.FamilyEconomy
      ?? throw new InvalidOperationException("The setup command did not save the family economy.");
    var savedProgression = result.FamilyProgression
      ?? throw new InvalidOperationException("The setup command did not save family progression.");
    var savedHouse = result.HouseCustomization
      ?? throw new InvalidOperationException("The setup command did not save house customization.");

    return new FamilyCommandResult(
    [
      Change(family.Id, "family-profile", savedFamily.Revision, savedFamily.Audit),
      Change(family.Id, "contributions", savedEconomy.Revision, savedEconomy.Audit),
      Change(family.Id, "family-progression", savedProgression.Revision, savedProgression.Audit),
      Change(family.Id, "savings", savedEconomy.Revision, savedEconomy.Audit),
      Change(family.Id, "reward-shop", savedEconomy.Revision, savedEconomy.Audit),
      Change(family.Id, "home-customization", savedHouse.Revision, savedHouse.Audit),
    ]);
  }

  private static IReadOnlyList<FamilyMemberState> ReadMembers(JsonElement payload)
  {
    return RequiredArray(payload, "members")
      .Select(ReadMember)
      .ToArray();
  }

  private static FamilyMemberState ReadMember(JsonElement value)
  {
    RequiredObject(value);
    var role = ParseRole(RequiredString(value, "role"));
    var access = role == MemberRole.Guardian
      ? ParseGuardianAccess(RequiredString(value, "guardianAccess"))
      : GuardianAccess.Supporter;
    var appearance = value.TryGetProperty("appearance", out var appearanceElement)
      && appearanceElement.ValueKind != JsonValueKind.Null
        ? ReadAvatarAppearance(appearanceElement)
        : null;

    return new FamilyMemberState(
      new FamilyMemberId(RequiredString(value, "id")),
      RequiredString(value, "name"),
      role,
      access,
      OptionalBool(value, "invitationPending") ?? false,
      OptionalString(value, "nickname"),
      OptionalString(value, "email"),
      OptionalBool(value, "participatesInWeeklyGoal") ?? role == MemberRole.Child,
      RequiredString(value, "avatar"),
      RequiredString(value, "color"),
      appearance
    );
  }

  private static IReadOnlyList<FamilyPetState> ReadPets(JsonElement payload)
  {
    return RequiredArray(payload, "pets")
      .Select(value =>
      {
        RequiredObject(value);

        return new FamilyPetState(
          new FamilyPetId(RequiredString(value, "id")),
          RequiredString(value, "name"),
          ParsePetKind(RequiredString(value, "kind")),
          RequiredString(value, "avatar"),
          RequiredString(value, "color")
        );
      })
      .ToArray();
  }

  private static AvatarAppearance ReadAvatarAppearance(JsonElement value)
  {
    RequiredObject(value);

    return new AvatarAppearance(
      RequiredString(value, "age"),
      RequiredString(value, "skinToneId"),
      RequiredString(value, "faceShape"),
      RequiredString(value, "face"),
      RequiredString(value, "hair"),
      RequiredString(value, "hairColorId"),
      RequiredString(value, "outfit"),
      RequiredString(value, "outfitColorId"),
      RequiredString(value, "accessoryId"),
      RequiredString(value, "funAccessoryId"),
      RequiredString(value, "seasonalAccessoryId")
    );
  }

  private static GuardianAccess ParseGuardianAccess(string value)
  {
    return value switch
    {
      "admin" => GuardianAccess.Admin,
      "supporter" => GuardianAccess.Supporter,
      _ => throw new FamilyCommandException("The guardian access level is invalid."),
    };
  }

  private static MemberRole ParseRole(string value)
  {
    return value switch
    {
      "child" => MemberRole.Child,
      "guardian" => MemberRole.Guardian,
      _ => throw new FamilyCommandException("The family member role is invalid."),
    };
  }

  private static FamilyPetKind ParsePetKind(string value)
  {
    return value switch
    {
      "cat" => FamilyPetKind.Cat,
      "dog" => FamilyPetKind.Dog,
      "rabbit" => FamilyPetKind.Rabbit,
      "bird" => FamilyPetKind.Bird,
      "other" => FamilyPetKind.Other,
      _ => throw new FamilyCommandException("The family pet kind is invalid."),
    };
  }
}
