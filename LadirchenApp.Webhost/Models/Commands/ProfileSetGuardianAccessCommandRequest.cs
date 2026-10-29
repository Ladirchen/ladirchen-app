using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record ProfileSetGuardianAccessCommandRequest(
  Guid CommandId,
  string ActorId,
  [property: JsonRequired] GuardianAccessPayload Payload
) : ExecuteFamilyCommandRequest(CommandId, ActorId)
{
  internal override System.Text.Json.JsonElement CreatePayloadElement()
  {
    return SerializePayload(Payload);
  }

  internal override string GetCommandName()
  {
    return "profile.set-guardian-access";
  }
}
