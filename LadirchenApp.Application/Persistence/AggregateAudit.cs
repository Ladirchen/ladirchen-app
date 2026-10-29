using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Persistence;

public sealed record AggregateAudit(FamilyMemberId UpdatedBy, DateTimeOffset UpdatedAt);
