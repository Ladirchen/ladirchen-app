using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record ProfileSetWeeklyParticipationCommandRequest(
  Guid CommandId,
  string ActorId,
  [property: JsonRequired] WeeklyParticipationPayload Payload
) : ExecuteFamilyCommandRequest(CommandId, ActorId)
{
  internal override System.Text.Json.JsonElement CreatePayloadElement()
  {
    return SerializePayload(Payload);
  }

  internal override string GetCommandName()
  {
    return "profile.set-weekly-participation";
  }
}
