using System;
using System.Collections.Generic;
using System.Security.Claims;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Models;

namespace LadirchenApp.Webhost.Infrastructure;

public static class FamilySessionIdentity
{
  public const string AuthenticationScheme = "LadirchenFamilySession";
  public const string FamilyIdClaim = "ladirchen:family-id";
  public const string MemberIdClaim = "ladirchen:member-id";

  public static ClaimsPrincipal CreatePrincipal(AuthenticationSessionResponse session)
  {
    ArgumentNullException.ThrowIfNull(session);
    var claims = new List<Claim>
    {
      new(ClaimTypes.NameIdentifier, session.MemberId),
      new(FamilyIdClaim, session.FamilyId.ToString("D")),
      new(MemberIdClaim, session.MemberId),
    };
    var identity = new ClaimsIdentity(claims, AuthenticationScheme);

    return new ClaimsPrincipal(identity);
  }

  public static bool BelongsToFamily(ClaimsPrincipal principal, Guid familyId)
  {
    ArgumentNullException.ThrowIfNull(principal);

    return familyId != Guid.Empty
      && Guid.TryParse(principal.FindFirstValue(FamilyIdClaim), out var authenticatedFamilyId)
      && authenticatedFamilyId == familyId;
  }

  public static bool TryGetMemberId(ClaimsPrincipal principal, out FamilyMemberId? memberId)
  {
    ArgumentNullException.ThrowIfNull(principal);
    var value = principal.FindFirstValue(MemberIdClaim);
    memberId = string.IsNullOrWhiteSpace(value) ? null : new FamilyMemberId(value);

    return memberId is not null;
  }

  public static bool TryGetFamilyId(ClaimsPrincipal principal, out FamilyId? familyId)
  {
    ArgumentNullException.ThrowIfNull(principal);
    var value = principal.FindFirstValue(FamilyIdClaim);
    familyId = Guid.TryParse(value, out var parsed) && parsed != Guid.Empty
      ? new FamilyId(parsed)
      : null;

    return familyId is not null;
  }
}