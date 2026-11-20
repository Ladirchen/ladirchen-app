using System;

namespace LadirchenApp.Webhost.Models;

public sealed record SchabiIntegrationResponse(
  string MemberId,
  string Username,
  bool Configured,
  bool AppointmentsConfigured,
  bool Enabled,
  int SyncDayOfWeek,
  int SyncHorizonDays,
  string SyncTimeLocal,
  DateTimeOffset? LastSuccessfulSyncAt,
  string? LastError
);
