using System;
using System.Linq;

using LadirchenApp.Domain.Avatar;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Family.Setup;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

internal static class FamilyPersistenceMapper
{
  public static Family ToDomain(FamilyRootEntity entity)
  {
    ArgumentNullException.ThrowIfNull(entity);
    var state = new FamilyState(
      new FamilyId(entity.FamilyId),
      new IanaTimeZone(entity.TimeZone),
      (FamilyCurrency)entity.Currency,
      new ExchangeRate(entity.LadirchenPerCurrencyUnit),
      OptionalMemberId(entity.ActiveChildId),
      entity.OnboardingCompleted,
      entity.Members
        .OrderBy(item => item.MemberId, StringComparer.Ordinal)
        .Select(ToMemberState),
      entity.Pets
        .OrderBy(item => item.PetId, StringComparer.Ordinal)
        .Select(ToPetState)
    );

    return Family.Reconstitute(state);
  }

  public static FamilyRootEntity CreateEntity(Family family)
  {
    ArgumentNullException.ThrowIfNull(family);
    var entity = new FamilyRootEntity();
    Apply(entity, family);

    return entity;
  }

  public static void Apply(FamilyRootEntity entity, Family family)
  {
    ArgumentNullException.ThrowIfNull(entity);
    ArgumentNullException.ThrowIfNull(family);
    var state = family.CaptureState();
    entity.FamilyId = state.Id.Value;
    entity.TimeZone = state.TimeZone.Value;
    entity.Currency = (int)state.Currency;
    entity.LadirchenPerCurrencyUnit = state.ExchangeRate.LadirchenPerCurrencyUnit;
    entity.ActiveChildId = state.ActiveChildId?.Value;
    entity.OnboardingCompleted = state.OnboardingCompleted;
    ApplyMembers(entity, state);
    ApplyPets(entity, state);
  }

  private static void ApplyPets(FamilyRootEntity entity, FamilyState state)
  {
    var existingPets = entity.Pets.ToDictionary(item => item.PetId, StringComparer.Ordinal);

    foreach (var pet in state.Pets)
    {
      if (existingPets.Remove(pet.Id.Value, out var existingPet))
      {
        ApplyPet(existingPet, pet);
      }
      else
      {
        entity.Pets.Add(ToPetEntity(state.Id.Value, pet));
      }
    }

    foreach (var removedPet in existingPets.Values)
    {
      _ = entity.Pets.Remove(removedPet);
    }
  }

  private static void ApplyMembers(FamilyRootEntity entity, FamilyState state)
  {
    var existingMembers = entity.Members.ToDictionary(item => item.MemberId, StringComparer.Ordinal);

    foreach (var member in state.Members)
    {
      if (existingMembers.Remove(member.Id.Value, out var existingMember))
      {
        ApplyMember(existingMember, member);
      }
      else
      {
        entity.Members.Add(ToMemberEntity(state.Id.Value, member));
      }
    }

    foreach (var removedMember in existingMembers.Values)
    {
      _ = entity.Members.Remove(removedMember);
    }
  }

  private static FamilyMemberState ToMemberState(FamilyMemberEntity entity)
  {
    return new FamilyMemberState(
      new FamilyMemberId(entity.MemberId),
      entity.Name,
      (MemberRole)entity.Role,
      (GuardianAccess)entity.GuardianAccess,
      entity.InvitationPending,
      entity.Nickname,
      entity.EmailAddress,
      entity.ParticipatesInWeeklyGoal,
      entity.Avatar,
      entity.Color,
      ToAppearance(entity)
    );
  }

  private static FamilyMemberEntity ToMemberEntity(Guid familyId, FamilyMemberState state)
  {
    var entity = new FamilyMemberEntity
    {
      FamilyId = familyId,
      MemberId = state.Id.Value,
    };
    ApplyMember(entity, state);

    return entity;
  }

  private static void ApplyMember(FamilyMemberEntity entity, FamilyMemberState state)
  {
    var appearance = state.Appearance;
    entity.Name = state.Name;
    entity.Role = (int)state.Role;
    entity.GuardianAccess = (int)state.Access;
    entity.InvitationPending = state.InvitationPending;
    entity.Nickname = state.Nickname;
    entity.EmailAddress = state.EmailAddress;
    entity.ParticipatesInWeeklyGoal = state.ParticipatesInWeeklyGoal;
    entity.Avatar = state.Avatar;
    entity.Color = state.Color;
    entity.AppearanceAge = appearance?.Age;
    entity.AppearanceSkinTone = appearance?.SkinTone;
    entity.AppearanceFaceShape = appearance?.FaceShape;
    entity.AppearanceFace = appearance?.Face;
    entity.AppearanceHair = appearance?.Hair;
    entity.AppearanceHairColor = appearance?.HairColor;
    entity.AppearanceOutfit = appearance?.Outfit;
    entity.AppearanceOutfitColor = appearance?.OutfitColor;
    entity.AppearanceAccessory = appearance?.Accessory;
    entity.AppearanceFunAccessory = appearance?.FunAccessory;
    entity.AppearanceSeasonalAccessory = appearance?.SeasonalAccessory;
  }

  private static FamilyPetState ToPetState(FamilyPetEntity entity)
  {
    return new FamilyPetState(
      new FamilyPetId(entity.PetId),
      entity.Name,
      (FamilyPetKind)entity.Kind,
      entity.Avatar,
      entity.Color
    );
  }

  private static FamilyPetEntity ToPetEntity(Guid familyId, FamilyPetState state)
  {
    var entity = new FamilyPetEntity
    {
      FamilyId = familyId,
      PetId = state.Id.Value,
    };
    ApplyPet(entity, state);

    return entity;
  }

  private static void ApplyPet(FamilyPetEntity entity, FamilyPetState state)
  {
    entity.Name = state.Name;
    entity.Kind = (int)state.Kind;
    entity.Avatar = state.Avatar;
    entity.Color = state.Color;
  }

  private static AvatarAppearance? ToAppearance(FamilyMemberEntity entity)
  {
    var values = new[]
    {
      entity.AppearanceAge,
      entity.AppearanceSkinTone,
      entity.AppearanceFaceShape,
      entity.AppearanceFace,
      entity.AppearanceHair,
      entity.AppearanceHairColor,
      entity.AppearanceOutfit,
      entity.AppearanceOutfitColor,
      entity.AppearanceAccessory,
      entity.AppearanceFunAccessory,
      entity.AppearanceSeasonalAccessory,
    };

    if (values.All(item => item is null))
    {
      return null;
    }

    if (values.Any(item => item is null))
    {
      throw new InvalidOperationException("A persisted avatar appearance must be complete.");
    }

    return new AvatarAppearance(
      entity.AppearanceAge!,
      entity.AppearanceSkinTone!,
      entity.AppearanceFaceShape!,
      entity.AppearanceFace!,
      entity.AppearanceHair!,
      entity.AppearanceHairColor!,
      entity.AppearanceOutfit!,
      entity.AppearanceOutfitColor!,
      entity.AppearanceAccessory!,
      entity.AppearanceFunAccessory!,
      entity.AppearanceSeasonalAccessory!
    );
  }

  private static FamilyMemberId? OptionalMemberId(string? value)
  {
    return value is null ? null : new FamilyMemberId(value);
  }
}
