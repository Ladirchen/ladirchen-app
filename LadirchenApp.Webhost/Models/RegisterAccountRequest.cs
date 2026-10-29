using System;

namespace LadirchenApp.Webhost.Models;

public sealed record RegisterAccountRequest(
  Guid FamilyId,
  string FamilyName,
  string GuardianName,
  string MemberId,
  string Password,
  string Username
);
