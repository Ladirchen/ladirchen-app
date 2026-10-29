using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record ProfileSaveOwnAppearanceCommandRequest(
  Guid CommandId,
  string ActorId,
  [property: JsonRequired] SaveAvatarAppearancePayload Payload
) : ExecuteFamilyCommandRequest(CommandId, ActorId)
{
  internal override System.Text.Json.JsonElement CreatePayloadElement()
  {
    return SerializePayload(Payload);
  }

  internal override string GetCommandName()
  {
    return "profile.save-own-appearance";
  }
}
