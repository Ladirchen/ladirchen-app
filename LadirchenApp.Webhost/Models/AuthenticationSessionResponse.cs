using System;

namespace LadirchenApp.Webhost.Models;

public sealed record AuthenticationSessionResponse(Guid FamilyId, string FamilyName, string MemberId);
