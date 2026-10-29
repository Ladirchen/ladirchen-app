using System;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
using LadirchenApp.Application.Queries;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Helpers;
using LadirchenApp.Webhost.Infrastructure;
using LadirchenApp.Webhost.Models;
using LadirchenApp.Webhost.Realtime;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

using Mumrich.SpaDevMiddleware.Domain.Contracts;
using Mumrich.SpaDevMiddleware.Extensions;

using Serilog;

namespace LadirchenApp.Webhost.Extensions;

public static partial class WebApplicationExtensions
{
  public static WebApplication MapFamilyProjectionApi(this WebApplication app)
  {
    app
      .MapPost(
        "/api/families/{familyId:guid}/projections/savings-interest",
        SimulateSavingsInterestAsync
      )
      .WithName("SimulateSavingsInterest")
      .WithTags("Family projections")
      .RequireAuthorization()
      .Produces<SavingsInterestSimulation>()
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status403Forbidden);

    return app;
  }

  private static async Task<IResult> SimulateSavingsInterestAsync(
    Guid familyId,
    SimulateSavingsInterestRequest request,
    SimulateSavingsInterestQueryHandler handler,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    if (!FamilySessionIdentity.BelongsToFamily(context.User, familyId))
    {
      return Results.Forbid();
    }

    if (!FamilySessionIdentity.TryGetMemberId(context.User, out var authenticatedMemberId))
    {
      return Results.Forbid();
    }

    try
    {
      var requestedMemberId = new FamilyMemberId(request.MemberId);

      if (requestedMemberId != authenticatedMemberId)
      {
        return Results.Forbid();
      }

      var query = new SimulateSavingsInterestQuery(
        new FamilyId(familyId),
        requestedMemberId,
        request.StreakDays,
        request.CompletionPercent,
        request.AverageRating
      );
      var result = await handler.HandleAsync(query, cancellationToken);

      return Results.Ok(result);
    }
    catch (Exception exception) when (exception is ArgumentException or DomainRuleException or InvalidOperationException)
    {
      return Results.BadRequest(
        new ProblemDetails { Detail = exception.Message, Title = "Invalid savings-interest simulation" }
      );
    }
  }
}
