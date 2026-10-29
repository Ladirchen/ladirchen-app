using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record PercentageFamilyCommandPayload([property: JsonRequired] decimal Percent);
