using System;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Helpers;
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

public static class WebApplicationExtensions
{
  public static void ConfigLadierchenWebhostApp(
    this WebApplication app,
    ISpaMiddlewareSettings appSettings,
    ILogger appLogger,
    string aboutInfoPath = "about"
  )
  {
    app.UseSerilogRequestLogging();
    app.UseNswag();
    app.UseWhenDev(appLogger);

    app.MapAboutInfo(aboutInfoPath);
    app.MapFamilyAggregateApi();
    app.MapHub<FamilyUpdatesHub>("/hubs/family-updates");
    app.MapSinglePageApps(appSettings);
  }

  public static WebApplication UseNswag(this WebApplication aWebApplication)
  {
    aWebApplication.UseOpenApi(settings =>
      settings.PostProcess = (document, _) =>
      {
        AboutInfo aboutInfo = AssemblyHelper.GetAboutInfoFromAssembly();

        document.Info.Title = aboutInfo.Name;
        document.Info.Version = aboutInfo.SemVer;
      }
    );
    aWebApplication.UseSwaggerUi();

    return aWebApplication;
  }

  public static WebApplication UseWhenDev(this WebApplication aWebApplication, ILogger aLogger)
  {
    aLogger.Information(
      "*** App built (Hosting Environment: {EnvironmentName})",
      aWebApplication.Environment.EnvironmentName
    );

    if (aWebApplication.Environment.IsDevelopment())
    {
      aWebApplication.UseDeveloperExceptionPage();
    }

    return aWebApplication;
  }

  public static WebApplication MapAboutInfo(this WebApplication aWebApplication, string aPath = "about")
  {
    aWebApplication.MapGet(aPath, () => AssemblyHelper.GetAboutInfoFromAssembly()).WithTags("Info");

    return aWebApplication;
  }

  public static WebApplication MapFamilyAggregateApi(this WebApplication app)
  {
    var group = app.MapGroup("/api/families/{familyId:guid}/aggregates").WithTags("Family aggregates");
    group
      .MapGet("/{aggregateType}", GetFamilyAggregateAsync)
      .WithName("GetFamilyAggregate")
      .Produces<FamilyAggregateSnapshotResponse>()
      .Produces(StatusCodes.Status404NotFound)
      .ProducesProblem(StatusCodes.Status400BadRequest);
    group
      .MapPut("/{aggregateType}", SaveFamilyAggregateAsync)
      .WithName("SaveFamilyAggregate")
      .Produces<FamilyAggregateSnapshotResponse>()
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status409Conflict);

    return app;
  }

  private static async Task<IResult> GetFamilyAggregateAsync(
    Guid familyId,
    string aggregateType,
    GetFamilyAggregateQueryHandler handler,
    CancellationToken cancellationToken
  )
  {
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

  private static async Task<IResult> SaveFamilyAggregateAsync(
    Guid familyId,
    string aggregateType,
    SaveFamilyAggregateRequest request,
    SaveFamilyAggregateCommandHandler handler,
    CancellationToken cancellationToken
  )
  {
    if (!string.Equals(aggregateType, request.AggregateType, StringComparison.Ordinal))
    {
      return Results.BadRequest(
        new ProblemDetails
        {
          Detail = "The route and request aggregate types must match.",
          Title = "Invalid aggregate command",
        }
      );
    }

    try
    {
      var command = new SaveFamilyAggregateCommand(
        new FamilyId(familyId),
        aggregateType,
        request.ExpectedRevision,
        request.State,
        new FamilyMemberId(request.UpdatedBy)
      );
      var snapshot = await handler.HandleAsync(command, cancellationToken);

      return Results.Ok(FamilyAggregateSnapshotResponse.FromSnapshot(snapshot));
    }
    catch (FamilyAggregateConflictException exception)
    {
      return Results.Conflict(
        new ProblemDetails
        {
          Detail = exception.Message,
          Title = "Aggregate revision conflict",
        }
      );
    }
    catch (ArgumentException exception)
    {
      return Results.BadRequest(new ProblemDetails { Detail = exception.Message, Title = "Invalid aggregate command" });
    }
  }
}
