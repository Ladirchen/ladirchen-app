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
      .MapPost("/login", AuthenticateAccount)
      .WithName("AuthenticateAccount")
      .RequireRateLimiting("authentication")
      .Produces<AuthenticationSessionResponse>()
      .Produces(StatusCodes.Status429TooManyRequests)
      .Produces(StatusCodes.Status401Unauthorized);
    group
      .MapPost("/register", RegisterAccountAsync)
      .WithName("RegisterAccount")
      .RequireRateLimiting("authentication")
      .Produces<AuthenticationSessionResponse>()
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status409Conflict)
      .Produces(StatusCodes.Status429TooManyRequests);
    group
      .MapPost("/logout", (Func<HttpContext, Task<IResult>>)SignOutAccountAsync)
      .WithName("SignOutAccount")
      .RequireAuthorization()
      .Produces(StatusCodes.Status204NoContent);

    return app;
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
