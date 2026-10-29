using System;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
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
  public static WebApplication MapFamilyAggregateApi(this WebApplication app)
  {
    var group = app
      .MapGroup("/api/families/{familyId:guid}/aggregates")
      .RequireAuthorization()
      .WithTags("Family aggregates");
    group
      .MapGet("/{aggregateType}", GetFamilyAggregateAsync)
      .WithName("GetFamilyAggregate")
      .Produces<FamilyAggregateSnapshotResponse>()
      .Produces(StatusCodes.Status404NotFound)
      .ProducesProblem(StatusCodes.Status400BadRequest);
    return app;
  }
  private static async Task<IResult> GetFamilyAggregateAsync(
    Guid familyId,
    string aggregateType,
    GetFamilyAggregateQueryHandler handler,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    if (!FamilySessionIdentity.BelongsToFamily(context.User, familyId))
    {
      return Results.Forbid();
    }

    try
    {
      var query = new GetFamilyAggregateQuery(new FamilyId(familyId), aggregateType);
      var snapshot = await handler.HandleAsync(query, cancellationToken);

      return snapshot is null
        ? Results.NotFound()
        : Results.Ok(FamilyAggregateSnapshotResponse.FromSnapshot(snapshot));
    }
    catch (ArgumentException exception)
    {
      return Results.BadRequest(new ProblemDetails { Detail = exception.Message, Title = "Invalid aggregate query" });
    }
  }

}
