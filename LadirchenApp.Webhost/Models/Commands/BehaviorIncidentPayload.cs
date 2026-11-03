using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record BehaviorIncidentPayload([property: JsonRequired] string IncidentId);
