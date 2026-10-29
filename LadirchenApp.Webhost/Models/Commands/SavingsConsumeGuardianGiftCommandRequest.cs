using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record SavingsConsumeGuardianGiftCommandRequest(
  Guid CommandId,
  string ActorId,
  [property: JsonRequired] GuardianGiftReferencePayload Payload
) : ExecuteFamilyCommandRequest(CommandId, ActorId)
{
  internal override System.Text.Json.JsonElement CreatePayloadElement()
  {
    return SerializePayload(Payload);
  }

  internal override string GetCommandName()
  {
    return "savings.consume-guardian-gift";
  }
}
