using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record AvatarCatalogReadModel(
  [property: JsonRequired] AvatarProfileCatalogReadModel Child,
  [property: JsonRequired] AvatarProfileCatalogReadModel Guardian
);
