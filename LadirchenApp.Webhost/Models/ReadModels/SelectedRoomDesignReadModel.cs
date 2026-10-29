using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record SelectedRoomDesignReadModel(
  [property: JsonRequired] string DesignId,
  [property: JsonRequired] string ZoneId
);
