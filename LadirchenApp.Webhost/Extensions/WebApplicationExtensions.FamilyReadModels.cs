using System;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Helpers;
using LadirchenApp.Webhost.Infrastructure;
using LadirchenApp.Webhost.Models.ReadModels;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LadirchenApp.Webhost.Extensions;

public static partial class WebApplicationExtensions
{
  public static WebApplication MapFamilyReadModelApi(this WebApplication app)
  {
    var group = app
      .MapGroup("/api/families/{familyId:guid}/read-models")
      .RequireAuthorization()
      .WithTags("Family read models");
    group
      .MapGet("/contributions", GetContributionsReadModelAsync)
      .WithName("GetContributionsReadModel")
      .Produces<ContributionsReadModelResponse>()
      .Produces(StatusCodes.Status404NotFound)
      .ProducesProblem(StatusCodes.Status400BadRequest);
    group
      .MapGet("/family-profile", GetFamilyProfileReadModelAsync)
      .WithName("GetFamilyProfileReadModel")
      .Produces<FamilyProfileReadModelResponse>()
      .Produces(StatusCodes.Status404NotFound)
      .ProducesProblem(StatusCodes.Status400BadRequest);
    group
      .MapGet("/family-progression", GetFamilyProgressionReadModelAsync)
      .WithName("GetFamilyProgressionReadModel")
      .Produces<FamilyProgressionReadModelResponse>()
      .Produces(StatusCodes.Status404NotFound)
      .ProducesProblem(StatusCodes.Status400BadRequest);
    group
      .MapGet("/home-customization", GetHomeCustomizationReadModelAsync)
      .WithName("GetHomeCustomizationReadModel")
      .Produces<HomeCustomizationReadModelResponse>()
      .Produces(StatusCodes.Status404NotFound)
      .ProducesProblem(StatusCodes.Status400BadRequest);
    group
      .MapGet("/reward-shop", GetRewardShopReadModelAsync)
      .WithName("GetRewardShopReadModel")
      .Produces<RewardShopReadModelResponse>()
      .Produces(StatusCodes.Status404NotFound)
      .ProducesProblem(StatusCodes.Status400BadRequest);
    group
      .MapGet("/savings", GetSavingsReadModelAsync)
      .WithName("GetSavingsReadModel")
      .Produces<SavingsReadModelResponse>()
      .Produces(StatusCodes.Status404NotFound)
      .ProducesProblem(StatusCodes.Status400BadRequest);

    return app;
  }

  private static Task<IResult> GetContributionsReadModelAsync(
    Guid familyId,
    [FromQuery] string? viewerMemberId,
    GetFamilyAggregateQueryHandler handler,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    return GetFamilyReadModelAsync(
      familyId,
      "contributions",
      viewerMemberId,
      handler,
      context,
      ContributionsReadModelResponse.FromSnapshot,
      cancellationToken
    );
  }

  private static Task<IResult> GetFamilyProfileReadModelAsync(
    Guid familyId,
    [FromQuery] string? viewerMemberId,
    GetFamilyAggregateQueryHandler handler,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    return GetFamilyReadModelAsync(
      familyId,
      "family-profile",
      viewerMemberId,
      handler,
      context,
      FamilyProfileReadModelResponse.FromSnapshot,
      cancellationToken
    );
  }

  private static Task<IResult> GetFamilyProgressionReadModelAsync(
    Guid familyId,
    [FromQuery] string? viewerMemberId,
    GetFamilyAggregateQueryHandler handler,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    return GetFamilyReadModelAsync(
      familyId,
      "family-progression",
      viewerMemberId,
      handler,
      context,
      FamilyProgressionReadModelResponse.FromSnapshot,
      cancellationToken
    );
  }

  private static Task<IResult> GetHomeCustomizationReadModelAsync(
    Guid familyId,
    [FromQuery] string? viewerMemberId,
    GetFamilyAggregateQueryHandler handler,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    return GetFamilyReadModelAsync(
      familyId,
      "home-customization",
      viewerMemberId,
      handler,
      context,
      HomeCustomizationReadModelResponse.FromSnapshot,
      cancellationToken
    );
  }

  private static Task<IResult> GetRewardShopReadModelAsync(
    Guid familyId,
    [FromQuery] string? viewerMemberId,
    GetFamilyAggregateQueryHandler handler,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    return GetFamilyReadModelAsync(
      familyId,
      "reward-shop",
      viewerMemberId,
      handler,
      context,
      RewardShopReadModelResponse.FromSnapshot,
      cancellationToken
    );
  }

  private static Task<IResult> GetSavingsReadModelAsync(
    Guid familyId,
    [FromQuery] string? viewerMemberId,
    GetFamilyAggregateQueryHandler handler,
    HttpContext context,
    CancellationToken cancellationToken
  )
  {
    return GetFamilyReadModelAsync(
      familyId,
      "savings",
      viewerMemberId,
      handler,
      context,
      SavingsReadModelResponse.FromSnapshot,
      cancellationToken
    );
  }

  private static async Task<IResult> GetFamilyReadModelAsync<TResponse>(
    Guid familyId,
    string readModelType,
    string? viewerMemberId,
    GetFamilyAggregateQueryHandler handler,
    HttpContext context,
    Func<FamilyAggregateSnapshot, TResponse> createResponse,
    CancellationToken cancellationToken
  )
  {
    if (!FamilySessionIdentity.BelongsToFamily(context.User, familyId))
    {
      return Results.Forbid();
    }

    if (
      !FamilySessionIdentity.TryGetMemberId(context.User, out var authenticatedMemberId)
      || authenticatedMemberId is null
    )
    {
      return Results.Forbid();
    }

    try
    {
      var requestedViewerId = string.IsNullOrWhiteSpace(viewerMemberId)
        ? null
        : new FamilyMemberId(viewerMemberId);
      var query = new GetFamilyAggregateQuery(
        new FamilyId(familyId),
        readModelType,
        authenticatedMemberId,
        requestedViewerId
      );
      var snapshot = await handler.HandleAsync(query, cancellationToken);

      return snapshot is null
        ? Results.NotFound()
        : Results.Json(
          createResponse(snapshot),
          FamilyReadModelState.GetSerializerOptions()
        );
    }
    catch (DomainRuleException exception) when (exception.Code == DomainError.Forbidden)
    {
      return Results.Forbid();
    }
    catch (ArgumentException exception)
    {
      return Results.BadRequest(new ProblemDetails { Detail = exception.Message, Title = "Invalid read-model query" });
    }
  }
}
