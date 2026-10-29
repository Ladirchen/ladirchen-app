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
    entity.Members.Clear();
    entity.Pets.Clear();

    foreach (var member in state.Members)
    {
      entity.Members.Add(ToMemberEntity(state.Id.Value, member));
    }

    foreach (var pet in state.Pets)
    {
      entity.Pets.Add(ToPetEntity(state.Id.Value, pet));
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
    var appearance = state.Appearance;

    return new FamilyMemberEntity
    {
      FamilyId = familyId,
      MemberId = state.Id.Value,
      Name = state.Name,
      Role = (int)state.Role,
      GuardianAccess = (int)state.Access,
      InvitationPending = state.InvitationPending,
      Nickname = state.Nickname,
      EmailAddress = state.EmailAddress,
      ParticipatesInWeeklyGoal = state.ParticipatesInWeeklyGoal,
      Avatar = state.Avatar,
      Color = state.Color,
      AppearanceAge = appearance?.Age,
      AppearanceSkinTone = appearance?.SkinTone,
      AppearanceFaceShape = appearance?.FaceShape,
      AppearanceFace = appearance?.Face,
      AppearanceHair = appearance?.Hair,
      AppearanceHairColor = appearance?.HairColor,
      AppearanceOutfit = appearance?.Outfit,
      AppearanceOutfitColor = appearance?.OutfitColor,
      AppearanceAccessory = appearance?.Accessory,
      AppearanceFunAccessory = appearance?.FunAccessory,
      AppearanceSeasonalAccessory = appearance?.SeasonalAccessory,
    };
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
    return new FamilyPetEntity
    {
      FamilyId = familyId,
      PetId = state.Id.Value,
      Name = state.Name,
      Kind = (int)state.Kind,
      Avatar = state.Avatar,
      Color = state.Color,
    };
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
