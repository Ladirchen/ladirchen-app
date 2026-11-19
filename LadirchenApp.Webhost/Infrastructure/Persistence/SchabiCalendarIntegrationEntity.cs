using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class SchabiCalendarIntegrationEntity
{
  public Guid FamilyId { get; set; }
  public string MemberId { get; set; } = string.Empty;
  public string SchabiUsername { get; set; } = string.Empty;
  public string ProtectedCalendarUrl { get; set; } = string.Empty;
  public bool Enabled { get; set; }
  public int SyncDayOfWeek { get; set; } = (int)DayOfWeek.Thursday;
  public int SyncHorizonDays { get; set; } = 14;
  public TimeOnly SyncTimeLocal { get; set; } = new(6, 0);
  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset UpdatedAt { get; set; }
  public DateTimeOffset? LastAttemptAt { get; set; }
  public DateTimeOffset? LastSuccessfulSyncAt { get; set; }
  public string? LastError { get; set; }
}
