using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record BehaviorChildPayload([property: JsonRequired] string ChildId);
