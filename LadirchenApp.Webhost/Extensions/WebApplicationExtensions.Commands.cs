using System;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
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
  public static WebApplication MapFamilyCommandApi(this WebApplication app)
  {
    app
      .MapPost("/api/families/{familyId:guid}/commands", ExecuteFamilyCommandAsync)
      .WithName("ExecuteFamilyCommand")
      .WithTags("Family commands")
      .RequireAuthorization()
      .Produces<ExecuteFamilyCommandResponse>()
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status403Forbidden)
      .ProducesProblem(StatusCodes.Status404NotFound)
      .ProducesProblem(StatusCodes.Status409Conflict);

    return app;
  }
  private static async Task<IResult> ExecuteFamilyCommandAsync(
    Guid familyId,
    ExecuteFamilyCommandRequest request,
    ExecuteFamilyCommandHandler handler,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    if (!FamilySessionIdentity.BelongsToFamily(context.User, familyId))
    {
      return Results.Forbid();
    }

    var actorId = new FamilyMemberId(request.ActorId);

    if (!FamilySessionIdentity.TryGetMemberId(context.User, out var authenticatedMemberId))
    {
      return Results.Forbid();
    }

    try
    {
      var command = new FamilyCommand(
        request.CommandId,
        new FamilyId(familyId),
        actorId,
        request.CommandType,
        request.Payload,
        authenticatedMemberId
      );
      var result = await handler.HandleAsync(command, cancellationToken);

      return Results.Ok(ExecuteFamilyCommandResponse.FromResult(result));
    }
    catch (FamilyAggregateConflictException exception)
    {
      return Results.Conflict(
        new ProblemDetails
        {
          Detail = exception.Message,
          Title = "Family command conflict",
        }
      );
    }
    catch (FamilyCommandException exception)
    {
      return Results.BadRequest(
        new ProblemDetails
        {
          Detail = exception.Message,
          Title = "Family command rejected",
        }
      );
    }
    catch (DomainRuleException exception)
    {
      var statusCode = exception.Code switch
      {
        DomainError.Forbidden => StatusCodes.Status403Forbidden,
        DomainError.NotFound => StatusCodes.Status404NotFound,
        DomainError.InvalidValue => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status409Conflict,
      };

      return Results.Problem(
        detail: exception.Message,
        statusCode: statusCode,
        title: "Family command rejected"
      );
    }
    catch (ArgumentException exception)
    {
      return Results.BadRequest(
        new ProblemDetails
        {
          Detail = exception.Message,
          Title = "Invalid family command",
        }
      );
    }
  }
}
