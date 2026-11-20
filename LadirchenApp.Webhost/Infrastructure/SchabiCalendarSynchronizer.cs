using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LadirchenApp.Webhost.Infrastructure;

public sealed class SchabiCalendarSynchronizer
{
  private const decimal ImportedTaskEnergy = 9;
  private const decimal ImportedTaskReward = 0;
  private static readonly TimeSpan _calendarRequestTimeout = TimeSpan.FromSeconds(30);
  private readonly LadirchenDbContext _database;
  private readonly SchabiIntegrationService _integrations;
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly ICommandHandler<FamilyCommand, FamilyCommandResult> _commands;
  private readonly ILogger<SchabiCalendarSynchronizer> _logger;
  private readonly TimeProvider _timeProvider;

  public SchabiCalendarSynchronizer(
    LadirchenDbContext database,
    SchabiIntegrationService integrations,
    IHttpClientFactory httpClientFactory,
    ICommandHandler<FamilyCommand, FamilyCommandResult> commands,
    ILogger<SchabiCalendarSynchronizer> logger,
    TimeProvider timeProvider
  )
  {
    _database = database;
    _integrations = integrations;
    _httpClientFactory = httpClientFactory;
    _commands = commands;
    _logger = logger;
    _timeProvider = timeProvider;
  }

  public async Task SynchronizeDueIntegrationsAsync(CancellationToken cancellationToken)
  {
    var integrations = await _database.SchabiCalendarIntegrations
      .Where(item => item.Enabled)
      .ToArrayAsync(cancellationToken);

    foreach (var integration in integrations)
    {
      await SynchronizeAsync(integration, force: false, cancellationToken);
    }
  }

  public async Task SynchronizeNowAsync(
    FamilyId familyId,
    FamilyMemberId childId,
    CancellationToken cancellationToken
  )
  {
    var integration = await _database.SchabiCalendarIntegrations
      .SingleOrDefaultAsync(item =>
        item.FamilyId == familyId.Value
        && item.MemberId == childId.Value,
        cancellationToken
      )
      ?? throw new ArgumentException("The Schabi integration is not configured.", nameof(childId));

    if (!integration.Enabled)
    {
      throw new ArgumentException("The Schabi integration is disabled.", nameof(childId));
    }

    await SynchronizeAsync(integration, force: true, cancellationToken);
  }

  private async Task SynchronizeAsync(
    SchabiCalendarIntegrationEntity integration,
    bool force,
    CancellationToken cancellationToken
  )
  {
    var family = await _database.Families
      .Where(item => item.FamilyId == integration.FamilyId)
      .Select(item => new
      {
        item.TimeZone,
        AdministratorId = item.Members
          .Where(member => !member.InvitationPending
            && member.Role == (int)MemberRole.Guardian
            && member.GuardianAccess == (int)GuardianAccess.Admin)
          .Select(member => member.MemberId)
          .First(),
      })
      .SingleAsync(cancellationToken);
    var zone = new IanaTimeZone(family.TimeZone);
    var now = _timeProvider.GetUtcNow();
    var localNow = zone.LocalTime(now);
    var today = DateOnly.FromDateTime(localNow.DateTime);
    var localTime = TimeOnly.FromDateTime(localNow.DateTime);

    DateOnly? lastSuccessfulLocalDate = integration.LastSuccessfulSyncAt is null
      ? null
      : zone.LocalDate(integration.LastSuccessfulSyncAt.Value);

    if (!force && !IsScheduledSyncDue(
      today,
      localTime,
      (DayOfWeek)integration.SyncDayOfWeek,
      integration.SyncTimeLocal,
      lastSuccessfulLocalDate
    ))
    {
      return;
    }

    integration.LastAttemptAt = now;
    integration.LastError = null;
    await _database.SaveChangesAsync(cancellationToken);

    try
    {
      var tasks = await DownloadTasksAsync(integration, cancellationToken);
      var periodEnd = GetSynchronizationPeriodEnd(today, integration.SyncHorizonDays);

      foreach (var task in tasks.Where(item => item.DueDate >= today && item.DueDate < periodEnd))
      {
        await CreateTaskAsync(integration, family.AdministratorId, task, cancellationToken);
      }

      if (!string.IsNullOrWhiteSpace(integration.ProtectedAppointmentsCalendarUrl))
      {
        var appointments = await DownloadCalendarAsync(
          integration.ProtectedAppointmentsCalendarUrl,
          cancellationToken
        );

        foreach (var appointment in appointments.Where(item => item.DueDate >= today && item.DueDate < periodEnd))
        {
          await CreateFocusAsync(
            integration,
            family.AdministratorId,
            appointment,
            today,
            cancellationToken
          );
        }
      }

      integration.LastSuccessfulSyncAt = _timeProvider.GetUtcNow();
      await _database.SaveChangesAsync(cancellationToken);
      _logger.LogInformation(
        "Schabi calendar synchronization completed for family {FamilyId} and member {MemberId}.",
        integration.FamilyId,
        integration.MemberId
      );
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      throw;
    }
    catch (Exception exception)
    {
      integration.LastError = "Schabi calendar synchronization failed. Check the personal calendar link.";
      await _database.SaveChangesAsync(cancellationToken);
      _logger.LogError(
        "Schabi calendar synchronization failed for family {FamilyId} and member {MemberId} ({ExceptionType}).",
        integration.FamilyId,
        integration.MemberId,
        exception.GetType().Name
      );
    }
  }

  internal static DateOnly GetSynchronizationPeriodEnd(DateOnly today, int syncHorizonDays)
  {
    return today.AddDays(syncHorizonDays);
  }

  internal static string GetFocusDescription(SchabiCalendarTask appointment)
  {
    return string.IsNullOrWhiteSpace(appointment.Description)
      ? $"Schabi-Termin am {appointment.DueDate:dd.MM.yyyy}"
      : appointment.Description;
  }

  internal static bool IsScheduledSyncDue(
    DateOnly today,
    TimeOnly localTime,
    DayOfWeek syncDayOfWeek,
    TimeOnly syncTimeLocal,
    DateOnly? lastSuccessfulLocalDate
  )
  {
    return lastSuccessfulLocalDate is null
      || today.DayOfWeek == syncDayOfWeek
        && localTime >= syncTimeLocal
        && lastSuccessfulLocalDate != today;
  }

  private async Task<SchabiCalendarTask[]> DownloadTasksAsync(
    SchabiCalendarIntegrationEntity integration,
    CancellationToken cancellationToken
  )
  {
    return await DownloadCalendarAsync(integration.ProtectedCalendarUrl, cancellationToken);
  }

  private async Task<SchabiCalendarTask[]> DownloadCalendarAsync(
    string protectedCalendarUrl,
    CancellationToken cancellationToken
  )
  {
    var calendarUrl = SchabiCalendarUrl.Parse(_integrations.UnprotectCalendarUrl(protectedCalendarUrl));
    var client = _httpClientFactory.CreateClient(nameof(SchabiCalendarSynchronizer));
    client.Timeout = _calendarRequestTimeout;
    var content = await client.GetStringAsync(calendarUrl, cancellationToken);

    return SchabiCalendarParser.Parse(content).ToArray();
  }

  private async Task CreateFocusAsync(
    SchabiCalendarIntegrationEntity integration,
    string administratorId,
    SchabiCalendarTask appointment,
    DateOnly today,
    CancellationToken cancellationToken
  )
  {
    var alreadyImported = await _database.Set<ContributionPresentationEntity>()
      .Where(item => item.FamilyId == integration.FamilyId
        && item.IsFocus
        && item.FocusEndDate == appointment.DueDate)
      .Join(
        _database.FamilyEconomies.SelectMany(item => item.Contributions),
        presentation => new { presentation.FamilyId, presentation.ContributionId },
        contribution => new { contribution.FamilyId, contribution.ContributionId },
        (presentation, contribution) => contribution
      )
      .AnyAsync(item => item.AssigneeId == integration.MemberId && item.Title == appointment.Title, cancellationToken);

    if (alreadyImported)
    {
      return;
    }

    var identity = string.Join(
      '|',
      "focus",
      integration.FamilyId.ToString("D", CultureInfo.InvariantCulture),
      integration.MemberId,
      appointment.ExternalId,
      appointment.DueDate.ToString("O", CultureInfo.InvariantCulture)
    );
    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(identity));
    var payload = JsonSerializer.SerializeToElement(new
    {
      id = $"schabi-focus-{Convert.ToHexString(hash.AsSpan(0, 16)).ToLowerInvariant()}",
      title = appointment.Title,
      description = GetFocusDescription(appointment),
      icon = "Ⓢ",
      period = "date",
      startDate = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
      endDate = appointment.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
      assigneeId = integration.MemberId,
      completionBonus = 0,
    });
    var command = new FamilyCommand(
      new Guid(hash.AsSpan(0, 16)),
      new FamilyId(integration.FamilyId),
      new FamilyMemberId(administratorId),
      "contributions.add-focus",
      payload
    );
    _ = await _commands.HandleAsync(command, cancellationToken);
  }

  private async Task CreateTaskAsync(
    SchabiCalendarIntegrationEntity integration,
    string administratorId,
    SchabiCalendarTask task,
    CancellationToken cancellationToken
  )
  {
    var description = string.IsNullOrWhiteSpace(task.Description)
      ? $"Fällig am {task.DueDate:dd.MM.yyyy}"
      : $"Fällig am {task.DueDate:dd.MM.yyyy}\n\n{task.Description}";
    var alreadyImported = await _database.FamilyEconomies
      .Where(item => item.FamilyId == integration.FamilyId)
      .SelectMany(item => item.Contributions)
      .Join(
        _database.Set<ContributionPresentationEntity>(),
        contribution => new { contribution.FamilyId, contribution.ContributionId },
        presentation => new { presentation.FamilyId, presentation.ContributionId },
        (contribution, presentation) => new { Contribution = contribution, Presentation = presentation }
      )
      .Where(item =>
        item.Contribution.ContributionId.StartsWith("schabi-")
        && item.Contribution.AssigneeId == integration.MemberId
        && item.Contribution.Title == task.Title
        && item.Presentation.DueDate == task.DueDate)
      .Select(item => new
      {
        item.Contribution.ContributionId,
        item.Contribution.Status,
        item.Presentation.Description,
      })
      .FirstOrDefaultAsync(cancellationToken);

    if (alreadyImported is not null)
    {
      if (alreadyImported.Status == 0
        && !string.Equals(alreadyImported.Description, description, StringComparison.Ordinal))
      {
        await UpdateTaskDescriptionAsync(
          integration,
          administratorId,
          alreadyImported.ContributionId,
          task,
          description,
          cancellationToken
        );
      }

      return;
    }

    var identity = string.Join(
      '|',
      integration.FamilyId.ToString("D", CultureInfo.InvariantCulture),
      integration.MemberId,
      task.ExternalId,
      task.DueDate.ToString("O", CultureInfo.InvariantCulture)
    );
    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(identity));
    var contributionId = $"schabi-{Convert.ToHexString(hash.AsSpan(0, 16)).ToLowerInvariant()}";
    var commandId = new Guid(hash.AsSpan(0, 16));
    var payload = JsonSerializer.SerializeToElement(new
    {
      id = contributionId,
      title = task.Title,
      description,
      icon = "Ⓢ",
      reward = ImportedTaskReward,
      energy = ImportedTaskEnergy,
      kind = "school",
      assigneeId = integration.MemberId,
      category = "Hausaufgabe",
      dueDate = task.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
      recurrence = "none",
    });
    var command = new FamilyCommand(
      commandId,
      new FamilyId(integration.FamilyId),
      new FamilyMemberId(administratorId),
      "contributions.add",
      payload
    );
    _ = await _commands.HandleAsync(command, cancellationToken);
  }

  private async Task UpdateTaskDescriptionAsync(
    SchabiCalendarIntegrationEntity integration,
    string administratorId,
    string contributionId,
    SchabiCalendarTask task,
    string description,
    CancellationToken cancellationToken
  )
  {
    var identity = string.Join(
      '|',
      "update",
      integration.FamilyId.ToString("D", CultureInfo.InvariantCulture),
      contributionId,
      description
    );
    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(identity));
    var payload = JsonSerializer.SerializeToElement(new
    {
      id = contributionId,
      title = task.Title,
      description,
      icon = "Ⓢ",
      reward = ImportedTaskReward,
      energy = ImportedTaskEnergy,
      kind = "school",
      assigneeId = integration.MemberId,
      category = "Hausaufgabe",
      dueDate = task.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
      recurrence = "none",
    });
    var command = new FamilyCommand(
      new Guid(hash.AsSpan(0, 16)),
      new FamilyId(integration.FamilyId),
      new FamilyMemberId(administratorId),
      "contributions.update",
      payload
    );
    _ = await _commands.HandleAsync(command, cancellationToken);
  }
}
