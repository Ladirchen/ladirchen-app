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

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Hosting;

using Mumrich.SpaDevMiddleware.Domain.Contracts;
using Mumrich.SpaDevMiddleware.Extensions;

using Serilog;

using FamilyAuthenticationService = LadirchenApp.Webhost.Infrastructure.AuthenticationService;

namespace LadirchenApp.Webhost.Extensions;

public static partial class WebApplicationExtensions
{
  public static WebApplication MapAuthenticationApi(this WebApplication app)
  {
    var group = app.MapGroup("/api/auth").WithTags("Authentication");
    group
      .MapGet("/registration-status", GetRegistrationStatusAsync)
      .WithName("GetAuthenticationRegistrationStatus")
      .Produces<AuthenticationRegistrationStatusResponse>();
    group
      .MapGet("/session", GetCurrentAuthenticationSessionAsync)
      .WithName("GetCurrentAuthenticationSession")
      .RequireAuthorization()
      .Produces<AuthenticationSessionResponse>()
      .Produces(StatusCodes.Status401Unauthorized);
    group
      .MapPost("/login", AuthenticateAccount)
      .WithName("AuthenticateAccount")
      .RequireRateLimiting("authentication")
      .Produces<AuthenticationSessionResponse>()
      .Produces(StatusCodes.Status429TooManyRequests)
      .Produces(StatusCodes.Status401Unauthorized);
    group
      .MapPost("/activate", ActivateMemberAccountAsync)
      .WithName("ActivateMemberAccount")
      .RequireRateLimiting("authentication")
      .Produces<AuthenticationSessionResponse>()
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status409Conflict)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status429TooManyRequests);
    group
      .MapPost("/register", RegisterAccountAsync)
      .WithName("RegisterAccount")
      .RequireRateLimiting("authentication")
      .Produces<AuthenticationSessionResponse>()
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status409Conflict)
      .Produces(StatusCodes.Status429TooManyRequests);
    group
      .MapPost("/member-invitations", CreateMemberInvitationAsync)
      .WithName("CreateMemberAccountInvitation")
      .RequireAuthorization()
      .RequireRateLimiting("authentication")
      .Produces<MemberAccountInvitationResponse>()
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status403Forbidden)
      .ProducesProblem(StatusCodes.Status409Conflict)
      .Produces(StatusCodes.Status429TooManyRequests);
    group
      .MapPost("/logout", (Func<HttpContext, Task<IResult>>)SignOutAccountAsync)
      .WithName("SignOutAccount")
      .RequireAuthorization()
      .Produces(StatusCodes.Status204NoContent);

    return app;
  }

  private static async Task<IResult> GetRegistrationStatusAsync(
    FamilyAuthenticationService authentication,
    CancellationToken cancellationToken
  )
  {
    var registrationAvailable = await authentication.IsRegistrationAvailableAsync(
      cancellationToken
    );

    return Results.Ok(new AuthenticationRegistrationStatusResponse(registrationAvailable));
  }

  private static async Task<IResult> GetCurrentAuthenticationSessionAsync(
    CurrentAuthenticationSessionReader sessions,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    if (
      !FamilySessionIdentity.TryGetFamilyId(context.User, out var familyId)
      || !FamilySessionIdentity.TryGetMemberId(context.User, out var memberId)
    )
    {
      return Results.Unauthorized();
    }

    var session = await sessions.ReadAsync(familyId!, memberId!, cancellationToken);

    return session is null ? Results.Unauthorized() : Results.Ok(session);
  }

  private static async Task<IResult> AuthenticateAccount(
    AuthenticateAccountRequest request,
    FamilyAuthenticationService authentication,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    var session = await authentication.AuthenticateAsync(request, cancellationToken);

    if (session is null)
    {
      return Results.Unauthorized();
    }

    await SignInAsync(context, session);

    return Results.Ok(session);
  }

  private static async Task<IResult> ActivateMemberAccountAsync(
    ActivateMemberAccountRequest request,
    MemberAccountService memberAccounts,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    try
    {
      var session = await memberAccounts.ActivateAsync(request, cancellationToken);

      if (session is null)
      {
        return Results.Unauthorized();
      }

      await SignInAsync(context, session);

      return Results.Ok(session);
    }
    catch (FamilyAggregateConflictException exception)
    {
      return Results.Conflict(
        new ProblemDetails
        {
          Detail = exception.Message,
          Title = "Account activation conflict",
        }
      );
    }
    catch (DomainRuleException exception)
    {
      return Results.Conflict(
        new ProblemDetails
        {
          Detail = exception.Message,
          Title = "Account activation rejected",
        }
      );
    }
    catch (InvalidOperationException exception)
    {
      return Results.Conflict(
        new ProblemDetails
        {
          Detail = exception.Message,
          Title = "Account activation rejected",
        }
      );
    }
    catch (ArgumentException exception)
    {
      return Results.BadRequest(
        new ProblemDetails
        {
          Detail = exception.Message,
          Title = "Invalid account activation",
        }
      );
    }
  }

  private static async Task<IResult> CreateMemberInvitationAsync(
    CreateMemberInvitationRequest request,
    MemberAccountService memberAccounts,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    if (
      !FamilySessionIdentity.TryGetFamilyId(context.User, out var familyId)
      || !FamilySessionIdentity.TryGetMemberId(context.User, out var memberId)
    )
    {
      return Results.Forbid();
    }

    try
    {
      var response = await memberAccounts.CreateInvitationAsync(
        familyId!,
        memberId!,
        request,
        cancellationToken
      );

      return Results.Ok(response);
    }
    catch (FamilyAggregateConflictException exception)
    {
      return Results.Conflict(
        new ProblemDetails
        {
          Detail = exception.Message,
          Title = "Member invitation conflict",
        }
      );
    }
    catch (DomainRuleException exception)
    {
      var statusCode = exception.Code == DomainError.Forbidden
        ? StatusCodes.Status403Forbidden
        : StatusCodes.Status400BadRequest;

      return Results.Problem(
        detail: exception.Message,
        statusCode: statusCode,
        title: "Member invitation rejected"
      );
    }
    catch (InvalidOperationException exception)
    {
      return Results.Conflict(
        new ProblemDetails
        {
          Detail = exception.Message,
          Title = "Member invitation rejected",
        }
      );
    }
    catch (ArgumentException exception)
    {
      return Results.BadRequest(
        new ProblemDetails
        {
          Detail = exception.Message,
          Title = "Invalid member invitation",
        }
      );
    }
  }

  private static async Task<IResult> RegisterAccountAsync(
    RegisterAccountRequest request,
    FamilyAuthenticationService authentication,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    try
    {
      var session = await authentication.RegisterAsync(request, cancellationToken);
      await SignInAsync(context, session);

      return Results.Ok(session);
    }
    catch (FamilyAggregateConflictException exception)
    {
      return Results.Conflict(
        new ProblemDetails
        {
          Detail = exception.Message,
          Title = "Registration conflict",
        }
      );
    }
    catch (InvalidOperationException exception)
    {
      return Results.Conflict(new ProblemDetails { Detail = exception.Message, Title = "Registration rejected" });
    }
    catch (ArgumentException exception)
    {
      return Results.BadRequest(new ProblemDetails { Detail = exception.Message, Title = "Invalid registration" });
    }
  }

  private static Task SignInAsync(HttpContext context, AuthenticationSessionResponse session)
  {
    return context.SignInAsync(
      FamilySessionIdentity.AuthenticationScheme,
      FamilySessionIdentity.CreatePrincipal(session),
      new AuthenticationProperties { AllowRefresh = true, IsPersistent = true }
    );
  }

  private static async Task<IResult> SignOutAccountAsync(HttpContext context)
  {
    await context.SignOutAsync(FamilySessionIdentity.AuthenticationScheme);

    return Results.NoContent();
  }
}