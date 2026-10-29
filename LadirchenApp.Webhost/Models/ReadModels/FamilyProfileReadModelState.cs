using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record FamilyProfileReadModelState(
  [property: JsonRequired] AvatarCatalogReadModel AvatarCatalog,
  [property: JsonRequired] IReadOnlyList<FamilyMemberReadModel> Members,
  [property: JsonRequired] bool OnboardingCompleted,
  [property: JsonRequired] IReadOnlyList<FamilyPetReadModel> Pets,
  [property: JsonRequired] string SubscriptionTier,
  [property: JsonRequired] string TimeZone
);
