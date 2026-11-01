using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed class Contribution : Entity<ContributionId>
{
  internal Contribution(ContributionId id, string title, ContributionKind kind, LadirchenAmount reward,
    Percentage energy, FamilyMemberId? assignee, FamilyMemberId? creatorId = null) : base(id)
  {
    ArgumentNullException.ThrowIfNull(reward);
    ArgumentNullException.ThrowIfNull(energy);
    Title = Rules.Text(title);
    Kind = Rules.Defined(kind);
    Reward = reward;
    Energy = energy;
    AssigneeId = assignee;
    CreatorId = creatorId;
  }

  public string Title
  {
    get; private set;
  }
  public ContributionKind Kind
  {
    get; private set;
  }
  public LadirchenAmount Reward
  {
    get; private set;
  }
  public Percentage Energy
  {
    get; private set;
  }
  public FamilyMemberId? AssigneeId
  {
    get; private set;
  }
  public FamilyMemberId? CreatorId
  {
    get;
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

  internal void Update(ContributionDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    Rules.Require(
      Status == ContributionStatus.Available,
      DomainError.InvalidState,
      "Only available contributions may be edited."
    );
    Title = Rules.Text(definition.Title);
    Kind = Rules.Defined(definition.Kind);
    Reward = definition.Reward ?? throw new ArgumentNullException(nameof(definition));
    Energy = definition.Energy ?? throw new ArgumentNullException(nameof(definition));
    AssigneeId = definition.AssigneeId;
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
