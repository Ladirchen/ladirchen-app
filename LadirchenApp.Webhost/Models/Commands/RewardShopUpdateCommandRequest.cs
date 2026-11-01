using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record RewardShopUpdateCommandRequest(
  Guid CommandId,
  string ActorId,
  [property: JsonRequired] AddShopRewardPayload Payload
) : ExecuteFamilyCommandRequest(CommandId, ActorId)
{
  internal override System.Text.Json.JsonElement CreatePayloadElement()
  {
    return SerializePayload(Payload);
  }

  internal override string GetCommandName()
  {
    return "reward-shop.update";
  }
}
