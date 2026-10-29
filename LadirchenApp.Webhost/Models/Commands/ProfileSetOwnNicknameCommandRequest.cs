using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record ProfileSetOwnNicknameCommandRequest(
  Guid CommandId,
  string ActorId,
  [property: JsonRequired] NicknamePayload Payload
) : ExecuteFamilyCommandRequest(CommandId, ActorId)
{
  internal override System.Text.Json.JsonElement CreatePayloadElement()
  {
    return SerializePayload(Payload);
  }

  internal override string GetCommandName()
  {
    return "profile.set-own-nickname";
  }
}
