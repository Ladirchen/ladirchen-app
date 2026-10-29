using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record ProfileCompleteSetupCommandRequest(
  Guid CommandId,
  string ActorId,
  [property: JsonRequired] CompleteFamilySetupPayload Payload
) : ExecuteFamilyCommandRequest(CommandId, ActorId)
{
  internal override System.Text.Json.JsonElement CreatePayloadElement()
  {
    return SerializePayload(Payload);
  }

  internal override string GetCommandName()
  {
    return "profile.complete-setup";
  }
}
