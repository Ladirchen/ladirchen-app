using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence;
using LadirchenApp.Webhost.Models;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure;

public sealed class SchabiIntegrationService
{
  private const string DataProtectionPurpose = "Ladirchen.SchabiCalendarUrl.v1";
  private readonly LadirchenDbContext _database;
  private readonly IDataProtector _protector;
  private readonly IFamilyRepository _families;
  private readonly TimeProvider _timeProvider;

  public SchabiIntegrationService(
    LadirchenDbContext database,
    IDataProtectionProvider dataProtectionProvider,
    IFamilyRepository families,
    TimeProvider timeProvider
  )
  {
    _database = database;
    _protector = dataProtectionProvider.CreateProtector(DataProtectionPurpose);
    _families = families;
    _timeProvider = timeProvider;
  }

  public async Task<SchabiIntegrationResponse[]> ListAsync(
    FamilyId familyId,
    FamilyMemberId actorId,
    CancellationToken cancellationToken
  )
  {
    var family = await RequireAdministratorAsync(familyId, actorId, cancellationToken);
    var integrations = await _database.SchabiCalendarIntegrations
      .AsNoTracking()
      .Where(item => item.FamilyId == familyId.Value)
      .ToDictionaryAsync(item => item.MemberId, cancellationToken);

    return family.Members
      .Where(member => member.Role == MemberRole.Child && !member.InvitationPending)
      .Select(member => integrations.TryGetValue(member.Id.Value, out var integration)
        ? ToResponse(integration)
        : new SchabiIntegrationResponse(
          member.Id.Value,
          string.Empty,
          false,
          (int)DayOfWeek.Thursday,
          "06:00",
          null,
          null
        ))
      .ToArray();
  }

  public async Task<SchabiIntegrationResponse> SaveAsync(
    FamilyId familyId,
    FamilyMemberId actorId,
    FamilyMemberId childId,
    SchabiIntegrationRequest request,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(request);
    var family = await RequireAdministratorAsync(familyId, actorId, cancellationToken);
    var child = family.ActiveMember(childId);

    if (child.Role != MemberRole.Child)
    {
      throw new ArgumentException("A Schabi integration can only be assigned to a child.", nameof(childId));
    }

    var username = request.Username.Trim();

    if (string.IsNullOrWhiteSpace(username) || username.Length > 128)
    {
      throw new ArgumentException("A Schabi username with at most 128 characters is required.", nameof(request));
    }

    var now = _timeProvider.GetUtcNow();
    var syncDayOfWeek = RequiredDayOfWeek(request.SyncDayOfWeek);
    var syncTimeLocal = RequiredTime(request.SyncTimeLocal);
    var integration = await _database.SchabiCalendarIntegrations.SingleOrDefaultAsync(
      item => item.FamilyId == familyId.Value && item.MemberId == childId.Value,
      cancellationToken
    );

    if (integration is null)
    {
      if (string.IsNullOrWhiteSpace(request.CalendarUrl))
      {
        throw new ArgumentException("A Schabi calendar URL is required for a new integration.", nameof(request));
      }

      integration = new SchabiCalendarIntegrationEntity
      {
        CreatedAt = now,
        FamilyId = familyId.Value,
        MemberId = childId.Value,
      };
      _database.SchabiCalendarIntegrations.Add(integration);
    }

    integration.Enabled = request.Enabled;

    if (!string.IsNullOrWhiteSpace(request.CalendarUrl))
    {
      var calendarUrl = SchabiCalendarUrl.Parse(request.CalendarUrl);
      integration.ProtectedCalendarUrl = _protector.Protect(calendarUrl.AbsoluteUri);
    }

    integration.SchabiUsername = username;
    integration.SyncDayOfWeek = (int)syncDayOfWeek;
    integration.SyncTimeLocal = syncTimeLocal;
    integration.UpdatedAt = now;
    integration.LastError = null;
    await _database.SaveChangesAsync(cancellationToken);

    return ToResponse(integration);
  }

  public async Task DeleteAsync(
    FamilyId familyId,
    FamilyMemberId actorId,
    FamilyMemberId childId,
    CancellationToken cancellationToken
  )
  {
    _ = await RequireAdministratorAsync(familyId, actorId, cancellationToken);
    var integration = await _database.SchabiCalendarIntegrations.SingleOrDefaultAsync(
      item => item.FamilyId == familyId.Value && item.MemberId == childId.Value,
      cancellationToken
    );

    if (integration is null)
    {
      return;
    }

    _database.SchabiCalendarIntegrations.Remove(integration);
    await _database.SaveChangesAsync(cancellationToken);
  }

  internal string UnprotectCalendarUrl(string protectedCalendarUrl)
  {
    return _protector.Unprotect(protectedCalendarUrl);
  }

  private async Task<Family> RequireAdministratorAsync(
    FamilyId familyId,
    FamilyMemberId actorId,
    CancellationToken cancellationToken
  )
  {
    var loaded = await _families.LoadAsync(familyId, cancellationToken)
      ?? throw new InvalidOperationException("The family does not exist.");
    loaded.Root.RequireAdministrator(actorId);

    return loaded.Root;
  }

  private static SchabiIntegrationResponse ToResponse(SchabiCalendarIntegrationEntity integration)
  {
    return new SchabiIntegrationResponse(
      integration.MemberId,
      integration.SchabiUsername,
      integration.Enabled,
      integration.SyncDayOfWeek,
      integration.SyncTimeLocal.ToString("HH:mm", CultureInfo.InvariantCulture),
      integration.LastSuccessfulSyncAt,
      integration.LastError
    );
  }

  private static DayOfWeek RequiredDayOfWeek(int value)
  {
    if (!Enum.IsDefined((DayOfWeek)value))
    {
      throw new ArgumentException("The synchronization weekday is invalid.", nameof(value));
    }

    return (DayOfWeek)value;
  }

  private static TimeOnly RequiredTime(string value)
  {
    if (!TimeOnly.TryParseExact(
      value,
      "HH:mm",
      CultureInfo.InvariantCulture,
      DateTimeStyles.None,
      out var time
    ))
    {
      throw new ArgumentException("The synchronization time must use HH:mm.", nameof(value));
    }

    return time;
  }
}
