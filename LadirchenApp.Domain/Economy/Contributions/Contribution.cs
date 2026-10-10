using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed class Contribution : Entity<ContributionId>
{
  internal Contribution(ContributionId id, string title, ContributionKind kind, LadirchenAmount reward,
    Percentage energy, FamilyMemberId? assignee) : base(id)
  {
    ArgumentNullException.ThrowIfNull(reward);
    ArgumentNullException.ThrowIfNull(energy);
    Title = Rules.Text(title);
    Kind = Rules.Defined(kind);
    Reward = reward;
    Energy = kind == ContributionKind.Basic ? energy : new Percentage(0);
    AssigneeId = assignee;
  }

  public string Title
  {
    get;
  }
  public ContributionKind Kind
  {
    get;
  }
  public LadirchenAmount Reward
  {
    get;
  }
  public Percentage Energy
  {
    get;
  }
  public FamilyMemberId? AssigneeId
  {
    get; private set;
  }
  public ContributionStatus Status
  {
    get; private set;
  }
  public StarRating? Rating
  {
    get; private set;
  }
  public ContributionRewardBreakdown? EarnedReward
  {
    get; private set;
  }
  public DateTimeOffset? ApprovedAt
  {
    get; private set;
  }
  public IReadOnlyList<FamilyMemberId> InvitedPartners { get; private set; } = Array.Empty<FamilyMemberId>();

  internal void Assign(FamilyMemberId? member)
  {
    AssigneeId = member;
    InvitedPartners = Array.Empty<FamilyMemberId>();
  }
  internal void SetPartners(FamilyMemberId[] partners)
  {
    InvitedPartners = Array.AsReadOnly(partners);
  }

  internal void Submit()
  {
    Status = ContributionStatus.Pending;
  }

  internal void Return()
  {
    Status = ContributionStatus.Available;
  }

  internal void Approve(StarRating rating, ContributionRewardBreakdown reward, DateTimeOffset now)
  {
    Status = ContributionStatus.Approved;
    Rating = rating;
    EarnedReward = reward;
    ApprovedAt = now.ToUniversalTime();
  }
}