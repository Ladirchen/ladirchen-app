using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models;

public sealed record RegisterAccountRequest(
  [property: JsonRequired] Guid FamilyId,
  [property: JsonRequired] string Email,
  [property: JsonRequired] string FamilyName,
  [property: JsonRequired] string GuardianName,
  [property: JsonRequired] string MemberId,
  [property: JsonRequired] string Password,
  [property: JsonRequired] string Username
);
