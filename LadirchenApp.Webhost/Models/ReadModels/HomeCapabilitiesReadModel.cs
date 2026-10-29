using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record HomeCapabilitiesReadModel([property: JsonRequired] bool CanArrangeHouse);
