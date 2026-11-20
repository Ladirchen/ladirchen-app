using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record ContributionsAddFocusCommandRequest(
  Guid CommandId,
  string ActorId,
  [property: JsonRequired] AddFocusPayload Payload
) : ExecuteFamilyCommandRequest(CommandId, ActorId)
{
  internal override System.Text.Json.JsonElement CreatePayloadElement()
  {
    return SerializePayload(Payload);
  }

  internal override string GetCommandName()
  {
    return "contributions.add-focus";
  }
}
