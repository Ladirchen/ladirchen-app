using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record HomeCatalogPricePayload(
  [property: JsonRequired] string CatalogType,
  [property: JsonRequired] string Id,
  [property: JsonRequired] decimal Price,
  decimal? PromotionalPrice,
  string? PromotionUntil
);

public sealed record HomeSetCatalogPriceCommandRequest(
  Guid CommandId,
  string ActorId,
  [property: JsonRequired] HomeCatalogPricePayload Payload
) : ExecuteFamilyCommandRequest(CommandId, ActorId)
{
  internal override System.Text.Json.JsonElement CreatePayloadElement()
  {
    return SerializePayload(Payload);
  }

  internal override string GetCommandName()
  {
    return "home.set-catalog-price";
  }
}
