using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Avatar;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Family.Permissions;
using LadirchenApp.Domain.Family.Setup;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Models.ReadModels;

internal static class FamilyReadModelResponseMapper
{
  public static ContributionsReadModelResponse Map(
    ContributionsFamilyReadModelProjection source
  )
  {
    ArgumentNullException.ThrowIfNull(source);
    var presentations = source.Presentation.Contributions.ToDictionary(
      item => item.ContributionId
    );
    var contributions = source.Economy.Contributions.Select(contribution =>
    {
      var presentation = Required(
        presentations,
        contribution.Id,
        "The contribution presentation is missing."
      );
      var calculated = Required(
        source.CalculatedState.Contributions,
        contribution.Id,
        "The contribution calculation is missing."
      );
      var kind = FormatContributionKind(contribution.Kind);

      return new ContributionReadModelItem(
        contribution.Id.Value,
        null,
        contribution.Title,
        presentation.Description,
        presentation.Icon,
        string.Empty,
        "contributions.dynamic.familyArea",
        kind,
        FormatContributionStatus(contribution.Status),
        contribution.Reward.Value,
        calculated.ProjectedReward,
        contribution.Energy.Value,
        contribution.AssigneeId?.Value,
        string.Empty,
        kind == "basic" ? "contributions.dynamic.daily" : "contributions.dynamic.optional",
        kind == "basic" ? "sparkle" : null,
        contribution.Rating?.Value,
        Identifiers(contribution.InvitedPartners),
        Identifiers(calculated.PartnerableMemberIds),
        contribution.EarnedReward?.BaseReward,
        contribution.EarnedReward?.RatingBonus,
        contribution.EarnedReward?.Multiplier,
        FormatInstant(contribution.ApprovedAt),
        contribution.Status == ContributionStatus.Approved
          ? presentation.RewardCelebrated
          : null
      );
    }).ToArray();
    var promotions = source.Economy.Promotions.Select(promotion =>
      new ContributionPromotionReadModelItem(
        promotion.Id.Value,
        null,
        promotion.ContributionId.Value,
        string.Empty,
        "contributions.promotions.multiplierTitle",
        promotion.Multiplier.Value,
        promotion.Deadline.ToString(),
        promotion.TeamworkBonus.Value,
        promotion.Active,
        Required(
          source.CalculatedState.PromotionAvailability,
          promotion.Id,
          "The promotion calculation is missing."
        )
      )
    ).ToArray();

    return new ContributionsReadModelResponse(
      source.ReadModelType,
      source.Revision,
      new ContributionsReadModelState(
        contributions,
        IntegerMap(source.CalculatedState.CurrentWeekApprovedContributionCountByMember),
        DecimalMap(source.CalculatedState.CurrentWeekEarnedByMember),
        source.CalculatedState.PerfectRatingBonusPercent,
        promotions,
        DecimalMap(source.CalculatedState.TodayEarnedByMember)
      )
    );
  }

  public static FamilyProfileReadModelResponse Map(FamilyProfileReadModelProjection source)
  {
    ArgumentNullException.ThrowIfNull(source);
    var members = source.Members.Select(member =>
    {
      source.Permissions.TryGetValue(member.Id, out var permissions);

      return new FamilyMemberReadModel(
        member.Id.Value,
        member.Name,
        member.Nickname,
        member.Avatar ?? (member.Role == MemberRole.Child ? "🧒" : "🧑"),
        member.Color ?? (member.Role == MemberRole.Child ? "#58aa82" : "#7e8db8"),
        FormatMemberRole(member.Role),
        member.Role == MemberRole.Guardian ? FormatGuardianAccess(member.Access) : null,
        member.ParticipatesInWeeklyGoal,
        source.WeeklyStreaks.GetValueOrDefault(member.Id),
        source.EmailVisibleMemberIds.Contains(member.Id) ? member.EmailAddress : null,
        member.InvitationPending,
        MapAvatarAppearance(member.Appearance),
        permissions is null ? null : MapPermissions(permissions)
      );
    }).ToArray();
    var pets = source.Pets.Select(pet => new FamilyPetReadModel(
      pet.Id.Value,
      pet.Name,
      FormatPetKind(pet.Kind),
      pet.Avatar ?? "🐾",
      pet.Color ?? "#7e8db8"
    )).ToArray();

    return new FamilyProfileReadModelResponse(
      source.ReadModelType,
      source.Revision,
      new FamilyProfileReadModelState(
        MapAvatarCatalog(source.AvatarCatalog),
        members,
        source.OnboardingCompleted,
        pets,
        source.SubscriptionTier,
        source.TimeZone.Value
      )
    );
  }

  public static FamilyProgressionReadModelResponse Map(
    FamilyProgressionReadModelProjection source
  )
  {
    ArgumentNullException.ThrowIfNull(source);
    var members = source.MemberProgressions.ToDictionary(
      item => item.Key.Value,
      item => new MemberProgressionReadModel(
        item.Value.ApprovedDates
          .Select(date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
          .ToArray(),
        item.Value.Evaluation.Participation.AverageBasicRating,
        item.Value.Evaluation.Participation.EffectiveCompletionPercent,
        item.Value.EnergyContribution,
        item.Value.Evaluation.Stage.Id,
        item.Value.Evaluation.Stage.Position,
        item.Value.Evaluation.Value.Capped,
        item.Value.SavingsInterestRate.Value,
        item.Value.Evaluation.Participation.StreakDays
      ),
      StringComparer.Ordinal
    );

    return new FamilyProgressionReadModelResponse(
      source.ReadModelType,
      source.Revision,
      new FamilyProgressionReadModelState(
        source.CanCloseWeekSuccessfully,
        source.CompletedWeeklyStreak,
        source.CurrentWeekTarget,
        source.FamilyEnergy,
        source.HouseAtmosphere,
        source.HouseLevel,
        source.HouseVisualLevel,
        source.MaximumSavingsInterestRate.Value,
        source.MinimumSuccessfulWeekEnergy,
        members
      )
    );
  }

  public static HomeCustomizationReadModelResponse Map(
    HomeCustomizationReadModelProjection source
  )
  {
    ArgumentNullException.ThrowIfNull(source);
    var customization = source.Customization;
    var accessories = customization.Accessories.ToDictionary(item => item.Id, StringComparer.Ordinal);
    var themes = customization.Themes.ToDictionary(item => item.Id, StringComparer.Ordinal);
    var roomDesigns = customization.RoomDesigns.ToDictionary(item => item.Id, StringComparer.Ordinal);
    var catalogAccessories = HouseShopCatalog.Accessories.Select(item =>
    {
      var accessory = accessories[item.Key];

      return new HouseAccessoryCatalogEntryReadModel(
        source.Capabilities.AvailableAccessoryIds.Contains(item.Key),
        HouseShopCatalog.CatalogAreaForAccessory(item.Key),
        item.Key,
        item.Value.MinimumLevel,
        accessory.Owned,
        item.Value.Price,
        CapabilityMemberIds(
          source.CapabilityMemberId,
          source.Capabilities.PurchasableAccessoryIds.Contains(item.Key)
        )
      );
    }).ToArray();
    var catalogAreas = HouseShopCatalog.AreaMinimumLevels.Select(item =>
      new HouseAreaCatalogEntryReadModel(
        source.Capabilities.AvailableAreaIds.Contains(item.Key),
        item.Key,
        item.Value
      )
    ).ToArray();
    var catalogFurnitureSets = HouseShopCatalog.FurnitureSets.Select(item =>
      new FurnitureSetCatalogEntryReadModel(
        item.Value.AccessoryIds,
        source.Capabilities.AvailableFurnitureSetIds.Contains(item.Key),
        item.Value.AccessoryIds.Any(id =>
          HouseShopCatalog.CatalogAreaForAccessory(id) == "outside"
        ) ? "outside" : "inside",
        item.Key,
        item.Value.MinimumLevel,
        source.Capabilities.OwnedFurnitureSetIds.Contains(item.Key),
        item.Value.Price,
        CapabilityMemberIds(
          source.CapabilityMemberId,
          source.Capabilities.PurchasableFurnitureSetIds.Contains(item.Key)
        )
      )
    ).ToArray();
    var catalogHouseStages = HouseShopCatalog.HouseStages.Select(stage =>
      new HouseStageCatalogEntryReadModel(
        source.HouseLevel == stage.Level,
        stage.Id,
        stage.Level,
        stage.RoomCount
      )
    ).ToArray();
    var catalogRoomDesigns = HouseShopCatalog.RoomDesigns.Select(item =>
    {
      var design = roomDesigns[item.Key];

      return new RoomDesignCatalogEntryReadModel(
        source.Capabilities.AvailableRoomDesignIds.Contains(item.Key),
        item.Value.Area,
        item.Key,
        item.Value.MinimumLevel,
        design.Owned,
        item.Value.Price,
        CapabilityMemberIds(
          source.CapabilityMemberId,
          source.Capabilities.PurchasableRoomDesignIds.Contains(item.Key)
        ),
        item.Value.ThemeId
      );
    }).ToArray();
    var catalogThemes = HouseShopCatalog.Themes.Select(item =>
      new HouseThemeCatalogEntryReadModel(
        true,
        item.Key,
        themes[item.Key].Owned,
        item.Value,
        CapabilityMemberIds(
          source.CapabilityMemberId,
          source.Capabilities.PurchasableThemeIds.Contains(item.Key)
        )
      )
    ).ToArray();
    var state = new HomeCustomizationReadModelState(
      new HomeCapabilitiesReadModel(source.Capabilities.CanArrangeHouse),
      new HomeCatalogReadModel(
        catalogAccessories,
        catalogAreas,
        catalogFurnitureSets,
        catalogHouseStages,
        catalogRoomDesigns,
        catalogThemes
      ),
      customization.Accessories.Select(item =>
        new HouseAccessoryReadModel(item.Id, item.Equipped, item.Owned)
      ).ToArray(),
      customization.Themes.Select(item => new HouseEditionReadModel(item.Id, item.Owned)).ToArray(),
      customization.Placements.Select(item => new HousePlacementReadModel(
        item.Id,
        item.EntityType,
        item.EntityId,
        item.Placement.Area,
        item.Placement.X,
        item.Placement.Y,
        item.Placement.Scale
      )).ToArray(),
      customization.RoomDesigns.Select(item =>
        new RoomDesignReadModel(item.Id, item.Owned)
      ).ToArray(),
      customization.RoomDesigns
        .Where(item => item.Selected)
        .Select(item => new SelectedRoomDesignReadModel(item.Id, item.Room))
        .ToArray(),
      customization.SelectedThemeId
    );

    return new HomeCustomizationReadModelResponse(
      source.ReadModelType,
      source.Revision,
      state
    );
  }

  public static RewardShopReadModelResponse Map(RewardShopReadModelProjection source)
  {
    ArgumentNullException.ThrowIfNull(source);
    var presentations = source.Presentation.ShopRewards.ToDictionary(item => item.RewardId);
    var pendingRequests = source.Economy.RedemptionRequests
      .Where(item => item.Status == RedemptionStatus.Pending)
      .ToDictionary(item => item.RewardId);
    var rewards = source.Economy.ShopRewards
      .Where(reward => source.VisibleRewardIds.Contains(reward.Id))
      .Select(reward =>
      {
        var presentation = Required(
          presentations,
          reward.Id,
          "The reward presentation is missing."
        );
        var availability = Required(
          source.Availability,
          reward.Id,
          "The reward availability is missing."
        );
        pendingRequests.TryGetValue(reward.Id, out var request);

        return new ShopRewardReadModel(
          reward.Id.Value,
          null,
          reward.Title,
          presentation.Description,
          presentation.Icon,
          reward.Price.Value,
          FormatRewardCategory(reward.Category),
          reward.Quantity,
          presentation.Conditions,
          FormatDate(reward.Window.From),
          FormatDate(reward.Window.Until),
          reward.IsVisible,
          FormatRewardStatus(reward.Status),
          request?.RequesterId.Value,
          availability.Published,
          FormatPublicationStatus(availability.PublicationStatus),
          FormatNextPublicationChange(availability.NextPublicationChangeDate, source.TimeZone),
          ViewerCapability(availability.RequestableByMemberIds, source.ViewerId),
          ViewerCapability(availability.CancellableByMemberIds, source.ViewerId),
          ViewerCapability(availability.ApprovableByMemberIds, source.ViewerId)
        );
      }).ToArray();

    return new RewardShopReadModelResponse(
      source.ReadModelType,
      source.Revision,
      new RewardShopReadModelState(source.RedemptionOpen, rewards)
    );
  }

  public static SavingsReadModelResponse Map(SavingsFamilyReadModelProjection source)
  {
    ArgumentNullException.ThrowIfNull(source);
    var goalPresentations = source.Presentation.SavingGoals.ToDictionary(item => item.GoalId);
    var giftPresentations = source.Presentation.GuardianGifts.ToDictionary(item => item.GiftId);
    var goals = source.Economy.SavingsGoals
      .Where(goal => source.CalculatedState.Goals.ContainsKey(goal.Id))
      .Select(goal =>
      {
        var presentation = Required(
          goalPresentations,
          goal.Id,
          "The savings-goal presentation is missing."
        );
        var calculated = source.CalculatedState.Goals[goal.Id];
        var cancellationRequest = calculated.CancellationRequest is null
          ? null
          : new SavingGoalCancellationRequestReadModel(
            calculated.CancellationRequest.RequestedByMemberId.Value,
            Identifiers(calculated.CancellationRequest.ConsentedMemberIds),
            Identifiers(calculated.CancellationRequest.PendingMemberIds)
          );
        var commandLimits = calculated.CommandLimits.ToDictionary(
          item => item.Key.Value,
          item => new SavingsGoalCommandLimitsReadModel(
            item.Value.CanApproveCancellation,
            item.Value.CanCancel,
            item.Value.CanRejectCancellation,
            item.Value.CanRequestCancellation,
            item.Value.CanWithdraw,
            item.Value.MaximumDepositAmount,
            item.Value.MaximumSupportAmount
          ),
          StringComparer.Ordinal
        );

        return new SavingGoalReadModel(
          goal.Id.Value,
          null,
          goal.Title,
          presentation.Icon,
          goal.Owner.IsFamily ? "family" : goal.Owner.MemberId!.Value,
          goal.Target,
          goal.Saved,
          goal.StarterBonus,
          FormatInstant(goal.CreatedAt),
          goal.InterestEarned,
          calculated.ProjectedInterestCredit,
          calculated.RemainingAmount,
          calculated.WithdrawableBalance,
          Identifiers(calculated.VisibleToMemberIds),
          Identifiers(goal.Participants),
          source.Economy.SavingsVouchers.Any(item => item.Id == goal.Id)
            ? goal.Id.Value
            : null,
          cancellationRequest,
          commandLimits,
          FormatGoalVisibility(goal.Visibility),
          goal.Owner.IsFamily,
          goal.Cheered
        );
      }).ToArray();
    var vouchers = source.Economy.SavingsVouchers
      .Where(voucher => source.CalculatedState.Vouchers.ContainsKey(voucher.Id))
      .Select(voucher =>
      {
        var calculated = source.CalculatedState.Vouchers[voucher.Id];

        return new SavingsVoucherReadModel(
          voucher.Id.Value,
          voucher.Id.Value,
          voucher.Amount,
          voucher.OwnerId?.Value,
          Identifiers(voucher.Participants),
          Identifiers(voucher.Consents),
          VoucherStatus(voucher),
          FormatInstant(voucher.RedeemedAt),
          Identifiers(calculated.VisibleToMemberIds),
          Identifiers(calculated.ConsentableByMemberIds),
          Identifiers(calculated.RedeemableByMemberIds)
        );
      }).ToArray();
    var gifts = source.Economy.GuardianGoalGifts
      .Where(gift => gift.Id is not null)
      .Where(gift => source.CalculatedState.VisibleBalanceMemberIds.Contains(gift.OwnerId))
      .Select(gift =>
      {
        var presentation = Required(
          giftPresentations,
          gift.Id!,
          "The guardian-gift presentation is missing."
        );

        return new GuardianGiftReadModel(
          gift.Id!.Value,
          gift.OwnerId.Value,
          presentation.SenderName,
          presentation.GoalTitle,
          gift.Destination == GiftDestination.Goal ? "goal" : "balance",
          gift.Amount
        );
      }).ToArray();

    return new SavingsReadModelResponse(
      source.ReadModelType,
      source.Revision,
      new SavingsReadModelState(
        DecimalMap(source.CalculatedState.AvailableBalances),
        DecimalMap(source.CalculatedState.Balances),
        source.Currency.ToString(),
        goals,
        source.ExchangeRate.LadirchenPerCurrencyUnit,
        gifts,
        source.Economy.SettledInterestPeriods
          .OrderBy(item => item.Start)
          .Select(item => item.Key)
          .ToArray(),
        DecimalMap(source.CalculatedState.SharedSavedBalances),
        vouchers
      )
    );
  }

  private static T Required<TKey, T>(
    IReadOnlyDictionary<TKey, T> values,
    TKey key,
    string message
  )
    where TKey : notnull
  {
    return values.TryGetValue(key, out var value)
      ? value
      : throw new InvalidOperationException(message);
  }

  private static IReadOnlyDictionary<string, decimal> DecimalMap(
    IReadOnlyDictionary<FamilyMemberId, decimal> values
  )
  {
    return values.ToDictionary(item => item.Key.Value, item => item.Value, StringComparer.Ordinal);
  }

  private static IReadOnlyDictionary<string, int> IntegerMap(
    IReadOnlyDictionary<FamilyMemberId, int> values
  )
  {
    return values.ToDictionary(item => item.Key.Value, item => item.Value, StringComparer.Ordinal);
  }

  private static IReadOnlyList<string> Identifiers(IEnumerable<DomainIdentifier> identifiers)
  {
    return identifiers.Select(identifier => identifier.Value).ToArray();
  }

  private static IReadOnlyList<string> CapabilityMemberIds(
    FamilyMemberId memberId,
    bool capable
  )
  {
    return capable ? [memberId.Value] : [];
  }

  private static IReadOnlyList<string> ViewerCapability(
    IReadOnlyCollection<FamilyMemberId> memberIds,
    FamilyMemberId viewerId
  )
  {
    return memberIds.Contains(viewerId) ? [viewerId.Value] : [];
  }

  private static AvatarCatalogReadModel MapAvatarCatalog(AvatarCatalog catalog)
  {
    return new AvatarCatalogReadModel(
      MapAvatarProfileCatalog(catalog, AvatarProfile.Child),
      MapAvatarProfileCatalog(catalog, AvatarProfile.Guardian)
    );
  }

  private static AvatarProfileCatalogReadModel MapAvatarProfileCatalog(
    AvatarCatalog catalog,
    AvatarProfile profile
  )
  {
    return new AvatarProfileCatalogReadModel(
      catalog.Options("accessory", profile),
      catalog.Options("age", profile),
      catalog.Options("face", profile),
      catalog.Options("face shape", profile),
      catalog.Options("fun accessory", profile),
      catalog.Options("hair color", profile),
      catalog.Options("hair", profile),
      catalog.Options("outfit color", profile),
      catalog.Options("outfit", profile),
      catalog.Options("seasonal accessory", profile),
      catalog.Options("skin tone", profile)
    );
  }

  private static AvatarAppearanceReadModel? MapAvatarAppearance(AvatarAppearance? appearance)
  {
    return appearance is null
      ? null
      : new AvatarAppearanceReadModel(
        appearance.Age,
        appearance.SkinTone,
        appearance.FaceShape,
        appearance.Face,
        appearance.Hair,
        appearance.HairColor,
        appearance.Outfit,
        appearance.OutfitColor,
        appearance.Accessory,
        appearance.FunAccessory,
        appearance.SeasonalAccessory
      );
  }

  private static FamilyPermissionsReadModel MapPermissions(FamilyPermissions permissions)
  {
    return new FamilyPermissionsReadModel(
      permissions.InviteMembers,
      permissions.ManageContent,
      permissions.ManageFamily,
      permissions.ManageGoals,
      permissions.SupportGoals,
      permissions.ViewFamilyGoals,
      permissions.ViewGuardianGoals
    );
  }

  private static string FormatContributionKind(ContributionKind kind)
  {
    return kind switch
    {
      ContributionKind.Basic => "basic",
      ContributionKind.Extra => "extra",
      _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
  }

  private static string FormatContributionStatus(ContributionStatus status)
  {
    return status switch
    {
      ContributionStatus.Available => "available",
      ContributionStatus.Pending => "pending",
      ContributionStatus.Approved => "approved",
      _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };
  }

  private static string FormatMemberRole(MemberRole role)
  {
    return role switch
    {
      MemberRole.Child => "child",
      MemberRole.Guardian => "guardian",
      _ => throw new ArgumentOutOfRangeException(nameof(role)),
    };
  }

  private static string FormatGuardianAccess(GuardianAccess access)
  {
    return access switch
    {
      GuardianAccess.Admin => "admin",
      GuardianAccess.Supporter => "supporter",
      _ => throw new ArgumentOutOfRangeException(nameof(access)),
    };
  }

  private static string FormatPetKind(FamilyPetKind kind)
  {
    return kind switch
    {
      FamilyPetKind.Cat => "cat",
      FamilyPetKind.Dog => "dog",
      FamilyPetKind.Rabbit => "rabbit",
      FamilyPetKind.Bird => "bird",
      FamilyPetKind.Other => "other",
      _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
  }

  private static string FormatRewardCategory(ShopRewardCategory category)
  {
    return category switch
    {
      ShopRewardCategory.Time => "time",
      ShopRewardCategory.Activity => "activity",
      ShopRewardCategory.Allowance => "allowance",
      ShopRewardCategory.Gift => "gift",
      ShopRewardCategory.Privilege => "privilege",
      ShopRewardCategory.Custom => "custom",
      _ => throw new ArgumentOutOfRangeException(nameof(category)),
    };
  }

  private static string FormatRewardStatus(ShopRewardStatus status)
  {
    return status switch
    {
      ShopRewardStatus.Available => "available",
      ShopRewardStatus.Requested => "requested",
      ShopRewardStatus.Redeemed => "redeemed",
      _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };
  }

  private static string FormatPublicationStatus(ShopRewardPublicationStatus status)
  {
    return status switch
    {
      ShopRewardPublicationStatus.Hidden => "hidden",
      ShopRewardPublicationStatus.Scheduled => "scheduled",
      ShopRewardPublicationStatus.Expired => "expired",
      ShopRewardPublicationStatus.Visible => "visible",
      _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };
  }

  private static string FormatGoalVisibility(GoalVisibility visibility)
  {
    return visibility switch
    {
      GoalVisibility.Family => "family",
      GoalVisibility.Guardians => "guardians",
      GoalVisibility.Private => "private",
      _ => throw new ArgumentOutOfRangeException(nameof(visibility)),
    };
  }

  private static string VoucherStatus(GoalSavingsVoucher voucher)
  {
    if (voucher.RedemptionCount > 0)
    {
      return "redeemed";
    }

    return voucher.Redeemable ? "redeemable" : "pending";
  }

  private static string? FormatInstant(DateTimeOffset? value)
  {
    return value?.ToString("O", CultureInfo.InvariantCulture);
  }

  private static string FormatInstant(DateTimeOffset value)
  {
    return value.ToString("O", CultureInfo.InvariantCulture);
  }

  private static string? FormatDate(DateOnly? value)
  {
    return value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
  }

  private static string? FormatNextPublicationChange(
    DateOnly? transitionDate,
    IanaTimeZone timeZone
  )
  {
    if (transitionDate is null)
    {
      return null;
    }

    var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZone.Value);
    var localTransition = DateTime.SpecifyKind(
      transitionDate.Value.ToDateTime(TimeOnly.MinValue),
      DateTimeKind.Unspecified
    );
    var utcTransition = TimeZoneInfo.ConvertTimeToUtc(localTransition, zone);

    return new DateTimeOffset(utcTransition, TimeSpan.Zero).ToString(
      "O",
      CultureInfo.InvariantCulture
    );
  }
}
