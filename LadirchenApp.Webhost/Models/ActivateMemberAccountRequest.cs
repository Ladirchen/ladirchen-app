using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models;

public sealed record ActivateMemberAccountRequest(
  [property: JsonRequired] string ActivationCode,
  [property: JsonRequired] string Email,
  [property: JsonRequired] string Password,
  [property: JsonRequired] string Username
);
