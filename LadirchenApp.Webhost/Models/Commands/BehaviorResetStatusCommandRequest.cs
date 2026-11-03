using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record BehaviorResetStatusCommandRequest(
  Guid CommandId,
  string ActorId,
  [property: JsonRequired] BehaviorChildPayload Payload
) : ExecuteFamilyCommandRequest(CommandId, ActorId)
{
  internal override System.Text.Json.JsonElement CreatePayloadElement()
  {
    return SerializePayload(Payload);
  }

  internal override string GetCommandName()
  {
    return "savings.reset-behavior-status";
  }
}
