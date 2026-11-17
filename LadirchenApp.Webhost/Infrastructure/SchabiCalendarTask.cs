using System;

namespace LadirchenApp.Webhost.Infrastructure;

internal sealed record SchabiCalendarTask(
  string ExternalId,
  string Title,
  string Description,
  DateOnly DueDate
);
