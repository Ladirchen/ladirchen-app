using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record RoomDesignReadModel(
  [property: JsonRequired] string Id,
  [property: JsonRequired] bool Owned
);
