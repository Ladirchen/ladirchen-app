using System;

namespace LadirchenApp.Webhost.Models;

public sealed record SchabiIntegrationResponse(
  string MemberId,
  string Username,
  bool Configured,
  bool Enabled,
  int SyncDayOfWeek,
  string SyncTimeLocal,
  DateTimeOffset? LastSuccessfulSyncAt,
  string? LastError
);
