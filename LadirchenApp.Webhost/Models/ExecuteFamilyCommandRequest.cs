using System;
using System.Text.Json;

namespace LadirchenApp.Webhost.Models;

public sealed record ExecuteFamilyCommandRequest(
  Guid CommandId,
  string ActorId,
  string CommandType,
  JsonElement Payload
);
