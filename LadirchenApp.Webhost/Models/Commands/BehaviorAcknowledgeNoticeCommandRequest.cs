using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record BehaviorAcknowledgeNoticeCommandRequest(
  Guid CommandId,
  string ActorId,
  [property: JsonRequired] BehaviorIncidentPayload Payload
) : ExecuteFamilyCommandRequest(CommandId, ActorId)
{
  internal override System.Text.Json.JsonElement CreatePayloadElement()
  {
    return SerializePayload(Payload);
  }

  internal override string GetCommandName()
  {
    return "savings.acknowledge-behavior-notice";
  }
}
