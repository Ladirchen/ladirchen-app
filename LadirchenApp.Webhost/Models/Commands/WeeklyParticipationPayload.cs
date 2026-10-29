using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record WeeklyParticipationPayload(
  [property: JsonRequired] string MemberId,
  [property: JsonRequired] bool Participates
);
