using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models;

public sealed record AuthenticationRegistrationStatusResponse(
  [property: JsonRequired] bool RegistrationAvailable
);
