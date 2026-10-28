using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Auth;

public sealed record FamilySession(FamilyAggregate Family, FamilyMemberId MemberId);
