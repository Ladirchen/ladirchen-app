using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record CompleteFamilySetupPayload(
  [property: JsonRequired] IReadOnlyList<FamilySetupMemberPayload> Members,
  [property: JsonRequired] IReadOnlyList<FamilySetupPetPayload> Pets
);
