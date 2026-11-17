using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure;
using LadirchenApp.Webhost.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace LadirchenApp.Webhost.Extensions;

public static partial class WebApplicationExtensions
{
  public static WebApplication MapSchabiIntegrationApi(this WebApplication app)
  {
    var group = app.MapGroup("/api/families/{familyId:guid}/integrations/schabi")
      .WithTags("Schabi integration")
      .RequireAuthorization();
    group.MapGet("/", ListSchabiIntegrationsAsync)
      .WithName("ListSchabiIntegrations")
      .Produces<SchabiIntegrationResponse[]>();
    group.MapPut("/{childId}", SaveSchabiIntegrationAsync)
      .WithName("SaveSchabiIntegration")
      .Produces<SchabiIntegrationResponse>()
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status403Forbidden);
    group.MapDelete("/{childId}", DeleteSchabiIntegrationAsync)
      .WithName("DeleteSchabiIntegration")
      .Produces(StatusCodes.Status204NoContent)
      .ProducesProblem(StatusCodes.Status403Forbidden);
    group.MapPost("/{childId}/synchronize", SynchronizeSchabiIntegrationAsync)
      .WithName("SynchronizeSchabiIntegration")
      .Produces<SchabiIntegrationResponse>()
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status403Forbidden);

    return app;
  }

  private static async Task<IResult> ListSchabiIntegrationsAsync(
    Guid familyId,
    SchabiIntegrationService integrations,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    if (!TryGetFamilyActor(context, familyId, out var actorId))
    {
      return Results.Forbid();
    }

    try
    {
      return Results.Ok(await integrations.ListAsync(
        new FamilyId(familyId),
        actorId!,
        cancellationToken
      ));
    }
    catch (DomainRuleException)
    {
      return Results.Forbid();
    }
  }

  private static async Task<IResult> SaveSchabiIntegrationAsync(
    Guid familyId,
    string childId,
    SchabiIntegrationRequest request,
    SchabiIntegrationService integrations,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    if (!TryGetFamilyActor(context, familyId, out var actorId))
    {
      return Results.Forbid();
    }

    try
    {
      var response = await integrations.SaveAsync(
        new FamilyId(familyId),
        actorId!,
        new FamilyMemberId(childId),
        request,
        cancellationToken
      );

      return Results.Ok(response);
    }
    catch (DomainRuleException)
    {
      return Results.Forbid();
    }
    catch (ArgumentException exception)
    {
      return Results.BadRequest(new { error = exception.Message });
    }
  }

  private static async Task<IResult> DeleteSchabiIntegrationAsync(
    Guid familyId,
    string childId,
    SchabiIntegrationService integrations,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    if (!TryGetFamilyActor(context, familyId, out var actorId))
    {
      return Results.Forbid();
    }

    try
    {
      await integrations.DeleteAsync(
        new FamilyId(familyId),
        actorId!,
        new FamilyMemberId(childId),
        cancellationToken
      );

      return Results.NoContent();
    }
    catch (DomainRuleException)
    {
      return Results.Forbid();
    }
  }

  private static async Task<IResult> SynchronizeSchabiIntegrationAsync(
    Guid familyId,
    string childId,
    SchabiIntegrationService integrations,
    SchabiCalendarSynchronizer synchronizer,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    if (!TryGetFamilyActor(context, familyId, out var actorId))
    {
      return Results.Forbid();
    }

    try
    {
      _ = await integrations.ListAsync(new FamilyId(familyId), actorId!, cancellationToken);
      await synchronizer.SynchronizeNowAsync(
        new FamilyId(familyId),
        new FamilyMemberId(childId),
        cancellationToken
      );
      var response = (await integrations.ListAsync(
        new FamilyId(familyId),
        actorId!,
        cancellationToken
      )).Single(item => item.MemberId == childId);

      return Results.Ok(response);
    }
    catch (DomainRuleException)
    {
      return Results.Forbid();
    }
    catch (ArgumentException exception)
    {
      return Results.BadRequest(new { error = exception.Message });
    }
  }

  private static bool TryGetFamilyActor(
    HttpContext context,
    Guid familyId,
    out FamilyMemberId? actorId
  )
  {
    actorId = null;

    return FamilySessionIdentity.BelongsToFamily(context.User, familyId)
      && FamilySessionIdentity.TryGetMemberId(context.User, out actorId);
  }
}
