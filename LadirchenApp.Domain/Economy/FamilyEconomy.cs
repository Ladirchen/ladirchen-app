using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Economy;

public sealed partial class FamilyEconomy : AggregateRoot<FamilyId>
{
  private readonly FamilyAggregate _family;
  private readonly List<MemberWallet> _wallets;
  private readonly List<Contribution> _contributions = new();
  private readonly HashSet<ContributionId> _usedContributionIds = new();
  private readonly List<Promotion> _promotions = new();
  private readonly List<LedgerEntry> _ledger = new();
  private readonly HashSet<TransactionId> _operations = new();

  public FamilyEconomy(FamilyAggregate family)
    : base(family?.Id ?? throw new ArgumentNullException(nameof(family)))
  {
    this._family = family;
    _wallets = family.Members.Where(item => !item.InvitationPending).Select(item => new MemberWallet(item.Id)).ToList();
  }

  public IReadOnlyList<MemberWallet> Wallets
  {
    get
    {

      return _wallets.AsReadOnly();
    }
  }

  public IReadOnlyList<Contribution> Contributions
  {
    get
    {

      return _contributions.AsReadOnly();
    }
  }

  public IReadOnlyList<Promotion> Promotions
  {
    get
    {
      return _promotions.AsReadOnly();
    }
  }

  public IReadOnlyList<LedgerEntry> Ledger
  {
    get
    {
      return _ledger.AsReadOnly();
    }
  }

  public MemberWallet WalletFor(FamilyMemberId memberId)
  {
    _family.ActiveMember(memberId);

    return _wallets.Find(item => item.Id == memberId)
      ?? throw new DomainRuleException(DomainError.NotFound, "The wallet is not open.");
  }

  public void OpenWallet(FamilyMemberId actor, FamilyMemberId memberId)
  {
    _family.RequireAdministrator(actor);
    _family.ActiveMember(memberId);
    Rules.Require(_wallets.All(item => item.Id != memberId), DomainError.Duplicate, "The wallet is already open.");
    _wallets.Add(new MemberWallet(memberId));
  }

  public void GiveSpecialGift(
    FamilyMemberId actor,
    FamilyMemberId childId,
    LadirchenAmount amount,
    string reason,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    _family.RequireAdministrator(actor);
    RequireChild(childId);
    var value = Rules.Positive(amount);
    Rules.Require(
      value >= 1 && value <= 10000,
      DomainError.InvalidValue,
      "A special gift must be between 1 and 10000."
    );
    var text = Rules.Text(reason);
    RequireNewOperation(operationId);
    var wallet = WalletFor(childId);
    var balance = checked(wallet.Balance + value);
    wallet.Set(balance, wallet.Reserved);
    Record(
      operationId,
      new LedgerEntry(operationId, childId, null, LedgerKind.Credit, value, childId, text, now.ToUniversalTime(), actor)
    );
  }

  public void AddContribution(
    FamilyMemberId actor,
    ContributionId id,
    ContributionDefinition definition,
    DateTimeOffset now
  )
  {
    ArgumentNullException.ThrowIfNull(definition);
    var member = _family.ActiveMember(actor);
    Rules.Require(
      member.IsAdministrator || (member.Role == MemberRole.Guardian && actor == definition.AssigneeId),
      DomainError.Forbidden,
      "Only administrators or guardians creating their own contributions may create contributions."
    );

    if (definition.AssigneeId is not null)
    {
      _family.ActiveMember(definition.AssigneeId);
    }

    Rules.Require(
      !_usedContributionIds.Contains(id),
      DomainError.Duplicate,
      "This contribution identity was already used."
    );
    var contribution = new Contribution(
      id,
      definition.Title,
      definition.Kind,
      definition.Reward,
      definition.Energy,
      definition.AssigneeId
    );
    _contributions.Add(contribution);
    _usedContributionIds.Add(id);
    AddDomainEvent(new ContributionCreated(Id, id, now));
  }

  public void ClaimContribution(FamilyMemberId actor, ContributionId id, DateTimeOffset now)
  {
    RequireChild(actor);
    var contribution = FindContribution(id);
    RequireAvailable(contribution);
    Rules.Require(
      contribution.AssigneeId is null,
      DomainError.InvalidState,
      "An assigned contribution cannot be claimed."
    );
    contribution.Assign(actor);
    AddDomainEvent(new ContributionChanged(Id, id, "Claimed", now));
  }

  public void AssignContribution(FamilyMemberId actor, ContributionId id, FamilyMemberId? memberId, DateTimeOffset now)
  {
    _family.RequireAdministrator(actor);
    var contribution = FindContribution(id);
    RequireAvailable(contribution);

    if (memberId is not null)
    {
      _family.ActiveMember(memberId);
    }

    contribution.Assign(memberId);
    AddDomainEvent(new ContributionChanged(Id, id, "Assigned", now));
  }

  public void SetContributionPartners(
    FamilyMemberId actor,
    ContributionId id,
    IEnumerable<FamilyMemberId> partners,
    DateTimeOffset now
  )
  {
    ArgumentNullException.ThrowIfNull(partners);
    var member = _family.ActiveMember(actor);
    var contribution = FindContribution(id);
    RequireAvailable(contribution);
    Rules.Require(
      member.IsAdministrator || contribution.AssigneeId == actor,
      DomainError.Forbidden,
      "Only the assignee or an administrator may invite partners."
    );
    Rules.Require(
      contribution.Kind == ContributionKind.Extra,
      DomainError.InvalidState,
      "Only extra contributions have partners."
    );
    var ids = partners.Distinct().ToArray();

    foreach (var partner in ids)
    {
      RequireChild(partner);
      Rules.Require(
        partner != contribution.AssigneeId,
        DomainError.InvalidValue,
        "The assignee cannot also be a partner."
      );
    }

    contribution.SetPartners(ids);
    AddDomainEvent(new ContributionChanged(Id, id, "PartnersChanged", now));
  }

  public void SubmitContribution(FamilyMemberId actor, ContributionId id, DateTimeOffset now)
  {
    _family.ActiveMember(actor);
    var contribution = FindContribution(id);
    RequireAvailable(contribution);
    Rules.Require(
      contribution.AssigneeId == actor,
      DomainError.Forbidden,
      "Only the assignee may submit a contribution."
    );
    contribution.Submit();
    AddDomainEvent(new ContributionSubmitted(Id, id, actor, now));
  }

  public void ReturnContribution(FamilyMemberId actor, ContributionId id, DateTimeOffset now)
  {
    var contribution = FindContribution(id);
    RequireReviewer(actor, contribution);
    contribution.Return();
    AddDomainEvent(new ContributionReturned(Id, id, now));
  }

  public ContributionRewardBreakdown ApproveContribution(
    FamilyMemberId actor,
    ContributionId id,
    StarRating rating,
    TransactionId operationId,
    DateTimeOffset now,
    Percentage? perfectRatingBonus = null
  )
  {
    var contribution = FindContribution(id);
    RequireReviewer(actor, contribution);
    RequireNewOperation(operationId);
    var promotion = _promotions.Find(item =>
      item.ContributionId == id && item.Active && item.Deadline.IsOpen(now, _family.TimeZone)
    );
    var reward = ContributionRewardPolicy.Calculate(
      contribution.Reward,
      rating,
      perfectRatingBonus ?? new Percentage(5),
      promotion?.Multiplier,
      promotion?.TeamworkBonus
    );
    var assigneeId =
      contribution.AssigneeId ?? throw new DomainRuleException(DomainError.InvalidState, "An assignee is required.");
    var wallet = WalletFor(assigneeId);
    var balance = checked(wallet.Balance + reward.Total);
    contribution.Approve(rating, reward, now);
    wallet.Set(balance, wallet.Reserved);
    Record(
      operationId,
      new LedgerEntry(
        operationId,
        wallet.Id,
        null,
        LedgerKind.Credit,
        reward.Total,
        id,
        "Contribution reward",
        now.ToUniversalTime(),
        actor
      )
    );
    AddDomainEvent(new ContributionApproved(Id, id, actor, wallet.Id, rating, reward, now));

    return reward;
  }

  public void DeleteContribution(FamilyMemberId actor, ContributionId id, DateTimeOffset now)
  {
    _family.RequireAdministrator(actor);
    var contribution = FindContribution(id);
    _contributions.Remove(contribution);
    _promotions.RemoveAll(item => item.ContributionId == id);
    AddDomainEvent(new ContributionChanged(Id, id, "Deleted", now));
  }

  public void AddPromotion(
    FamilyMemberId actor,
    PromotionId id,
    ContributionId contributionId,
    RewardMultiplier multiplier,
    LadirchenAmount teamworkBonus,
    PromotionDeadline deadline,
    DateTimeOffset now
  )
  {
    _family.RequireAdministrator(actor);
    FindContribution(contributionId);
    Rules.Require(_promotions.All(item => item.Id != id), DomainError.Duplicate, "The promotion already exists.");
    var promotion = new Promotion(id, contributionId, multiplier, teamworkBonus, deadline);
    _promotions.Add(promotion);
    AddDomainEvent(new ContributionChanged(Id, contributionId, "PromotionAdded", now));
  }

  public void SetPromotionActive(FamilyMemberId actor, PromotionId id, bool active, DateTimeOffset now)
  {
    _family.RequireAdministrator(actor);
    var promotion = FindPromotion(id);
    promotion.SetActive(active);
    AddDomainEvent(new ContributionChanged(Id, promotion.ContributionId, "PromotionActivationChanged", now));
  }

  public void DeletePromotion(FamilyMemberId actor, PromotionId id, DateTimeOffset now)
  {
    _family.RequireAdministrator(actor);
    var promotion = FindPromotion(id);
    _promotions.Remove(promotion);
    AddDomainEvent(new ContributionChanged(Id, promotion.ContributionId, "PromotionDeleted", now));
  }

  private Contribution FindContribution(ContributionId id)
  {
    ArgumentNullException.ThrowIfNull(id);

    return _contributions.Find(item => item.Id == id)
      ?? throw new DomainRuleException(DomainError.NotFound, "The contribution does not exist.");
  }

  private Promotion FindPromotion(PromotionId id)
  {
    ArgumentNullException.ThrowIfNull(id);

    return _promotions.Find(item => item.Id == id)
      ?? throw new DomainRuleException(DomainError.NotFound, "The promotion does not exist.");
  }

  private void RequireChild(FamilyMemberId id)
  {
    Rules.Require(
      _family.ActiveMember(id).Role == MemberRole.Child,
      DomainError.Forbidden,
      "An active child is required."
    );
  }

  private static void RequireAvailable(Contribution contribution)
  {
    Rules.Require(
      contribution.Status == ContributionStatus.Available,
      DomainError.InvalidState,
      "The contribution must be available."
    );
  }

  private void RequireReviewer(FamilyMemberId actor, Contribution contribution)
  {
    var reviewer = _family.ActiveMember(actor);
    Rules.Require(
      contribution.Status == ContributionStatus.Pending && contribution.AssigneeId is not null,
      DomainError.InvalidState,
      "A submitted contribution is required."
    );
    var assigneeId =
      contribution.AssigneeId ?? throw new DomainRuleException(DomainError.InvalidState, "An assignee is required.");
    var assignee = _family.ActiveMember(assigneeId);
    Rules.Require(
      (assignee.Role == MemberRole.Child && reviewer.IsAdministrator)
        || (assignee.Role == MemberRole.Guardian && reviewer.Role == MemberRole.Child),
      DomainError.Forbidden,
      "Administrators review children; children review guardians."
    );
  }

  private void RequireNewOperation(TransactionId operationId)
  {
    ArgumentNullException.ThrowIfNull(operationId);
    Rules.Require(!_operations.Contains(operationId), DomainError.Duplicate, "This operation was already applied.");
  }

  private void Record(TransactionId operationId, params LedgerEntry[] entries)
  {
    _operations.Add(operationId);
    _ledger.AddRange(entries);

    foreach (var entry in entries)
    {
      AddDomainEvent(new LedgerRecorded(Id, entry));
    }
  }
}
