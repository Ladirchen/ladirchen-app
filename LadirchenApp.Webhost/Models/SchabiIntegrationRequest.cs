using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models;

public sealed record SchabiIntegrationRequest(
  [property: JsonRequired] string Username,
  string? CalendarUrl,
  string? AppointmentsCalendarUrl,
  bool Enabled,
  int SyncDayOfWeek,
  int SyncHorizonDays,
  [property: JsonRequired] string SyncTimeLocal
);
