using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Family.Permissions;
using LadirchenApp.Webhost.Infrastructure;
using LadirchenApp.Webhost.Infrastructure.Persistence;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;
using LadirchenApp.Webhost.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LadirchenApp.Webhost.Extensions;

public static partial class WebApplicationExtensions
{
  public static WebApplication MapWebPushApi(this WebApplication app)
  {
    var group = app.MapGroup("/api/push")
      .WithTags("Push notifications")
      .RequireAuthorization();
    group
      .MapGet("/configuration", GetWebPushConfiguration)
      .WithName("GetWebPushConfiguration")
      .Produces<WebPushConfigurationResponse>();
    group
      .MapPost("/subscriptions", SaveWebPushSubscriptionAsync)
      .WithName("SaveWebPushSubscription")
      .Produces(StatusCodes.Status204NoContent)
      .ProducesProblem(StatusCodes.Status403Forbidden)
      .ProducesProblem(StatusCodes.Status503ServiceUnavailable);
    group
      .MapDelete("/subscriptions", DeleteWebPushSubscriptionAsync)
      .WithName("DeleteWebPushSubscription")
      .Produces(StatusCodes.Status204NoContent);

    return app;
  }

  private static IResult GetWebPushConfiguration(HttpContext context)
  {
    var configuration = WebPushConfiguration.Resolve(context.RequestServices.GetRequiredService<IConfiguration>());

    return Results.Ok(new WebPushConfigurationResponse(
      configuration is not null,
      configuration?.PublicKey
    ));
  }

  private static async Task<IResult> SaveWebPushSubscriptionAsync(
    WebPushSubscriptionRequest request,
    LadirchenDbContext database,
    HttpContext context,
    TimeProvider timeProvider,
    CancellationToken cancellationToken
  )
  {
    if (WebPushConfiguration.Resolve(context.RequestServices.GetRequiredService<IConfiguration>()) is null)
    {
      return Results.Problem(
        statusCode: StatusCodes.Status503ServiceUnavailable,
        title: "Web Push is not configured."
      );
    }

    if (!TryReadSession(context, out var familyId, out var memberId))
    {
      return Results.Forbid();
    }

    var member = await database.Set<FamilyMemberEntity>()
      .AsNoTracking()
      .SingleOrDefaultAsync(item =>
        item.FamilyId == familyId
        && item.MemberId == memberId,
        cancellationToken
      );

    var canReceivePush = member is not null
      && new FamilyPermissionPolicy().Resolve(
        (MemberRole)member.Role,
        (GuardianAccess)member.GuardianAccess,
        member.InvitationPending
      ).ReceivePushNotifications;

    if (!canReceivePush)
    {
      return Results.Forbid();
    }

    var endpoint = Required(request.Endpoint, nameof(request.Endpoint), 2048);
    var endpointHash = HashEndpoint(endpoint);
    var existing = await database.WebPushSubscriptions
      .SingleOrDefaultAsync(item => item.EndpointHash == endpointHash, cancellationToken);
    var now = timeProvider.GetUtcNow();

    if (existing is null)
    {
      database.WebPushSubscriptions.Add(new WebPushSubscriptionEntity
      {
        Auth = Required(request.Auth, nameof(request.Auth), 256),
        CreatedAt = now,
        Endpoint = endpoint,
        EndpointHash = endpointHash,
        FamilyId = familyId,
        Locale = NormalizeLocale(request.Locale),
        MemberId = memberId,
        P256dh = Required(request.P256dh, nameof(request.P256dh), 512),
        SubscriptionId = Guid.NewGuid(),
        UpdatedAt = now,
      });
    }
    else
    {
      if (existing.FamilyId != familyId || existing.MemberId != memberId)
      {
        await database.WebPushNotifications
          .Where(item => item.SubscriptionId == existing.SubscriptionId)
          .ExecuteDeleteAsync(cancellationToken);
      }

      existing.Auth = Required(request.Auth, nameof(request.Auth), 256);
      existing.Endpoint = endpoint;
      existing.FamilyId = familyId;
      existing.Locale = NormalizeLocale(request.Locale);
      existing.MemberId = memberId;
      existing.P256dh = Required(request.P256dh, nameof(request.P256dh), 512);
      existing.UpdatedAt = now;
    }

    await database.SaveChangesAsync(cancellationToken);

    return Results.NoContent();
  }

  private static async Task<IResult> DeleteWebPushSubscriptionAsync(
    [FromQuery] string endpoint,
    LadirchenDbContext database,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    if (!TryReadSession(context, out var familyId, out var memberId))
    {
      return Results.Forbid();
    }

    var endpointHash = HashEndpoint(Required(endpoint, nameof(endpoint), 2048));
    var subscription = await database.WebPushSubscriptions
      .SingleOrDefaultAsync(item =>
        item.EndpointHash == endpointHash
        && item.FamilyId == familyId
        && item.MemberId == memberId,
        cancellationToken
      );

    if (subscription is not null)
    {
      database.WebPushSubscriptions.Remove(subscription);
      await database.SaveChangesAsync(cancellationToken);
    }

    return Results.NoContent();
  }

  private static bool TryReadSession(
    HttpContext context,
    out Guid familyId,
    out string memberId
  )
  {
    var hasFamily = FamilySessionIdentity.TryGetFamilyId(context.User, out var typedFamilyId);
    var hasMember = FamilySessionIdentity.TryGetMemberId(context.User, out var typedMemberId);
    familyId = typedFamilyId?.Value ?? Guid.Empty;
    memberId = typedMemberId?.Value ?? string.Empty;

    return hasFamily && hasMember;
  }

  private static string HashEndpoint(string endpoint)
  {
    return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(endpoint)));
  }

  private static string NormalizeLocale(string? locale)
  {
    return string.Equals(locale, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "de";
  }

  private static string Required(string value, string parameterName, int maximumLength)
  {
    if (string.IsNullOrWhiteSpace(value) || value.Length > maximumLength)
    {
      throw new ArgumentException("The push subscription value is invalid.", parameterName);
    }

    return value;
  }
}