using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models;

public sealed record AuthenticationSessionResponse(
  [property: JsonRequired] Guid FamilyId,
  [property: JsonRequired] string FamilyName,
  [property: JsonRequired] string MemberId
);
