using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record ContributionsAddFocusPreparationsCommandRequest(
  Guid CommandId,
  string ActorId,
  [property: JsonRequired] AddFocusPreparationsPayload Payload
) : ExecuteFamilyCommandRequest(CommandId, ActorId)
{
  internal override System.Text.Json.JsonElement CreatePayloadElement()
  {
    return SerializePayload(Payload);
  }

  internal override string GetCommandName()
  {
    return "contributions.add-focus-preparations";
  }
}
