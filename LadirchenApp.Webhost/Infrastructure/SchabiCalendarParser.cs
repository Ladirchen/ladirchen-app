using System;
using System.Collections.Generic;
using System.Linq;

using Ical.Net;
using Ical.Net.CalendarComponents;

namespace LadirchenApp.Webhost.Infrastructure;

internal static class SchabiCalendarParser
{
  public static IReadOnlyList<SchabiCalendarTask> Parse(string content)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(content);
    var calendar = Calendar.Load(content)
      ?? throw new FormatException("The Schabi calendar is invalid.");

    return calendar.Events
      .Where(item => item.Start is not null)
      .Where(item => !IsDeletedSubscriptionNotice(item.Summary))
      .Select(item => new { Event = item, Start = item.Start! })
      .Select(item => new SchabiCalendarTask(
        string.IsNullOrWhiteSpace(item.Event.Uid)
          ? $"{item.Event.Summary}|{item.Start.Value:O}"
          : item.Event.Uid,
        string.IsNullOrWhiteSpace(item.Event.Summary) ? "Schulaufgabe" : item.Event.Summary.Trim(),
        BuildDescription(item.Event),
        DateOnly.FromDateTime(item.Start.Value)
      ))
      .DistinctBy(item => new { item.ExternalId, item.DueDate })
      .OrderBy(item => item.DueDate)
      .ThenBy(item => item.Title, StringComparer.Ordinal)
      .ToArray();
  }

  private static string BuildDescription(CalendarEvent calendarEvent)
  {
    var parts = new List<string>();
    if (!string.IsNullOrWhiteSpace(calendarEvent.Description))
    {
      parts.Add(calendarEvent.Description.Trim());
    }

    AddWebLink(parts, calendarEvent.Url);
    foreach (var attachment in calendarEvent.Attachments)
    {
      AddWebLink(parts, attachment.Uri);
    }

    return string.Join(
      "\n\n",
      parts.Distinct(StringComparer.OrdinalIgnoreCase)
    );
  }

  private static void AddWebLink(ICollection<string> parts, Uri? uri)
  {
    if (uri is not { IsAbsoluteUri: true }
      || uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
    {
      return;
    }

    parts.Add(uri.AbsoluteUri);
  }

  private static bool IsDeletedSubscriptionNotice(string? summary)
  {
    return summary?.Contains(
      "Kalender-Abo von Benutzer in Schabi gelöscht",
      StringComparison.OrdinalIgnoreCase
    ) == true;
  }
}
