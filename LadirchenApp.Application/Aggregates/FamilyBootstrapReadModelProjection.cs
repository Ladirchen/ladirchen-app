namespace LadirchenApp.Application.Aggregates;

public sealed record FamilyBootstrapReadModelProjection(
  ContributionsFamilyReadModelProjection Contributions,
  FamilyProfileReadModelProjection FamilyProfile,
  FamilyProgressionReadModelProjection? FamilyProgression,
  HomeCustomizationReadModelProjection? HomeCustomization,
  RewardShopReadModelProjection RewardShop,
  SavingsFamilyReadModelProjection Savings
);
