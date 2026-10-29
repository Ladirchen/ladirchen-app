using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record WeeklyInterestPeriodPayload(
  [property: JsonRequired] int Year,
  [property: JsonRequired] int Week
);
