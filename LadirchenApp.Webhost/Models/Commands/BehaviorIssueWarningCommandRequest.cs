using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record BehaviorIssueWarningCommandRequest(
  Guid CommandId,
  string ActorId,
  [property: JsonRequired] BehaviorWarningPayload Payload
) : ExecuteFamilyCommandRequest(CommandId, ActorId)
{
  internal override System.Text.Json.JsonElement CreatePayloadElement()
  {
    return SerializePayload(Payload);
  }

  internal override string GetCommandName()
  {
    return "savings.issue-behavior-warning";
  }
}
