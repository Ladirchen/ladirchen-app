using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models;

public sealed record AuthenticateAccountRequest(
  [property: JsonRequired] string Identifier,
  [property: JsonRequired] string Password
);
