using System;
using System.Text.Json;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

public sealed record FamilyCommand(
  Guid CommandId,
  FamilyId FamilyId,
  FamilyMemberId ActorId,
  string CommandType,
  JsonElement Payload,
  FamilyMemberId? AuthenticatedMemberId = null
);
