using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record HomePlaceAccessoryCommandRequest(
  Guid CommandId,
  string ActorId,
  [property: JsonRequired] HouseAccessoryPlacementPayload Payload
) : ExecuteFamilyCommandRequest(CommandId, ActorId)
{
  internal override System.Text.Json.JsonElement CreatePayloadElement()
  {
    return SerializePayload(Payload);
  }

  internal override string GetCommandName()
  {
    return "home.place-accessory";
  }
}
