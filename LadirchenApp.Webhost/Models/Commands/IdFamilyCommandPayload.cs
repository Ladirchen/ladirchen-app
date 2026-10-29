using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record IdFamilyCommandPayload([property: JsonRequired] string Id);
