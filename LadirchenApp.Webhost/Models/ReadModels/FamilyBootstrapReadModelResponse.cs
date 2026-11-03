using System.Text.Json.Serialization;

using LadirchenApp.Application.Aggregates;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record FamilyBootstrapReadModelResponse(
  [property: JsonRequired] ContributionsReadModelResponse Contributions,
  [property: JsonRequired] FamilyProfileReadModelResponse FamilyProfile,
  FamilyProgressionReadModelResponse? FamilyProgression,
  HomeCustomizationReadModelResponse? HomeCustomization,
  [property: JsonRequired] RewardShopReadModelResponse RewardShop,
  [property: JsonRequired] SavingsReadModelResponse Savings
)
{
  public static FamilyBootstrapReadModelResponse FromProjection(
    FamilyBootstrapReadModelProjection projection
  )
  {
    return new FamilyBootstrapReadModelResponse(
      ContributionsReadModelResponse.FromProjection(projection.Contributions),
      FamilyProfileReadModelResponse.FromProjection(projection.FamilyProfile),
      projection.FamilyProgression is null
        ? null
        : FamilyProgressionReadModelResponse.FromProjection(projection.FamilyProgression),
      projection.HomeCustomization is null
        ? null
        : HomeCustomizationReadModelResponse.FromProjection(projection.HomeCustomization),
      RewardShopReadModelResponse.FromProjection(projection.RewardShop),
      SavingsReadModelResponse.FromProjection(projection.Savings)
    );
  }
}
