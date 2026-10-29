using System;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models;

public sealed record MemberAccountInvitationResponse(
  [property: JsonRequired] string ActivationCode,
  [property: JsonRequired] string Email,
  [property: JsonRequired] DateTimeOffset ExpiresAt,
  [property: JsonRequired] string MemberId
);
