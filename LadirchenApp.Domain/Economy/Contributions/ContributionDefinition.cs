using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record ContributionDefinition(string Title, ContributionKind Kind, LadirchenAmount Reward,
  Percentage Energy, FamilyMemberId? AssigneeId);