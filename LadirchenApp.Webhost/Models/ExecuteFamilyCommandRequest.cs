using System.Text.Json;

namespace LadirchenApp.Webhost.Models;

public sealed record ExecuteFamilyCommandRequest(string ActorId, string CommandType, JsonElement Payload);
