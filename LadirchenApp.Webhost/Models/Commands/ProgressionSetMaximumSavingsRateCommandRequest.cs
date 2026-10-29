using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record ProgressionSetMaximumSavingsRateCommandRequest(
  Guid CommandId,
  string ActorId,
  [property: JsonRequired] PercentageFamilyCommandPayload Payload
) : ExecuteFamilyCommandRequest(CommandId, ActorId)
{
  internal override System.Text.Json.JsonElement CreatePayloadElement()
  {
    return SerializePayload(Payload);
  }

  internal override string GetCommandName()
  {
    return "progression.set-maximum-savings-rate";
  }
}
