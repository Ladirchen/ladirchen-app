using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record HouseEntityReferencePayload([property: JsonRequired] string PlacementId);
