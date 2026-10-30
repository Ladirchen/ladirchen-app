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
  private readonly IReadOnlyList<FamilyEconomyMember> _members;
  private readonly IanaTimeZone _timeZone;
  private readonly List<MemberWallet> _wallets;
  private readonly List<Contribution> _contributions = new();
  private readonly HashSet<ContributionId> _usedContributionIds = new();
  private readonly List<Promotion> _promotions = new();
  private readonly List<LedgerEntry> _ledger = new();
  private readonly HashSet<TransactionId> _operations = new();

  public FamilyEconomy(FamilyAggregate family)
    : base(family?.Id ?? throw new ArgumentNullException(nameof(family)))
  {
    _timeZone = family.TimeZone;
    _members = family.Members
      .Select(item => new FamilyEconomyMember(item.Id, item.Role, item.Access, item.InvitationPending))
      .ToArray();
    _wallets = _members.Where(item => !item.InvitationPending).Select(item => new MemberWallet(item.Id)).ToList();
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

  public IReadOnlyCollection<TransactionId> AppliedOperationIds
  {
    get
    {
      return _operations.ToArray();
    }
  }

  public IReadOnlyCollection<ContributionId> UsedContributionIds
  {
    get
    {
      return _usedContributionIds.ToArray();
    }
  }

  public MemberWallet WalletFor(FamilyMemberId memberId)
  {
    ActiveMember(memberId);

    return _wallets.Find(item => item.Id == memberId)
      ?? throw new DomainRuleException(DomainError.NotFound, "The wallet is not open.");
  }

  public void OpenWallet(FamilyMemberId actor, FamilyMemberId memberId)
  {
    RequireAdministrator(actor);
    ActiveMember(memberId);
    Rules.Require(_wallets.All(item => item.Id != memberId), DomainError.Duplicate, "The wallet is already open.");
    _wallets.Add(new MemberWallet(memberId));
  }

  public void GiveSpecialGift(
    FamilyMemberId actor,
    FamilyMemberId childId,
    LadirchenAmount amount,
    string reason,
    TransactionId operationId,
    DateTimeOffset now,
    GuardianGiftId? giftId = null
  )
  {
    RequireAdministrator(actor);
    RequireChild(childId);
    RequireNewPendingGiftId(giftId);
    var value = Rules.Round(Rules.Positive(amount));
    Rules.Require(
      value >= SpecialGiftPolicy.MinimumAmount && value <= SpecialGiftPolicy.MaximumAmount,
      DomainError.InvalidValue,
      $"A special gift must be between {SpecialGiftPolicy.MinimumAmount} and {SpecialGiftPolicy.MaximumAmount}."
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

    if (giftId is not null)
    {
      AddPendingGift(
        new GuardianGoalGift(giftId, childId, actor, value, null, GiftDestination.Balance)
      );
    }
  }

  public void AddContribution(
    FamilyMemberId actor,
    ContributionId id,
    ContributionDefinition definition,
    DateTimeOffset now
  )
  {
    ArgumentNullException.ThrowIfNull(definition);
    var member = ActiveMember(actor);
    Rules.Require(
      member.IsAdministrator || (member.Role == MemberRole.Guardian && actor == definition.AssigneeId),
      DomainError.Forbidden,
      "Only administrators or guardians creating their own contributions may create contributions."
    );

    if (definition.AssigneeId is not null)
    {
      ActiveMember(definition.AssigneeId);
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
    RequireAdministrator(actor);
    var contribution = FindContribution(id);
    RequireAvailable(contribution);

    if (memberId is not null)
    {
      ActiveMember(memberId);
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
    var member = ActiveMember(actor);
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
      ActiveMember(partner);
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
    ActiveMember(actor);
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
    DateTimeOffset now
  )
  {
    var contribution = FindContribution(id);
    RequireReviewer(actor, contribution);
    RequireNewOperation(operationId);
    var promotion = _promotions.Find(item =>
      item.ContributionId == id && item.Active && item.Deadline.IsOpen(now, _timeZone)
    );
    var reward = ContributionRewardPolicy.Calculate(
      contribution.Reward,
      rating,
      _contributionSettings.PerfectRatingBonus,
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
    RequireAdministrator(actor);
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
    RequireAdministrator(actor);
    FindContribution(contributionId);
    Rules.Require(
      multiplier.Value is 2 or 3,
      DomainError.InvalidValue,
      "A bonus campaign supports only two-times or three-times rewards."
    );
    Rules.Require(_promotions.All(item => item.Id != id), DomainError.Duplicate, "The promotion already exists.");
    var promotion = new Promotion(id, contributionId, multiplier, teamworkBonus, deadline);
    _promotions.Add(promotion);
    AddDomainEvent(new ContributionChanged(Id, contributionId, "PromotionAdded", now));
  }

  public void SetPromotionActive(FamilyMemberId actor, PromotionId id, bool active, DateTimeOffset now)
  {
    RequireAdministrator(actor);
    var promotion = FindPromotion(id);
    promotion.SetActive(active);
    AddDomainEvent(new ContributionChanged(Id, promotion.ContributionId, "PromotionActivationChanged", now));
  }

  public void DeletePromotion(FamilyMemberId actor, PromotionId id, DateTimeOffset now)
  {
    RequireAdministrator(actor);
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
      ActiveMember(id).Role == MemberRole.Child,
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
    var reviewer = ActiveMember(actor);
    Rules.Require(
      contribution.Status == ContributionStatus.Pending && contribution.AssigneeId is not null,
      DomainError.InvalidState,
      "A submitted contribution is required."
    );
    var assigneeId =
      contribution.AssigneeId ?? throw new DomainRuleException(DomainError.InvalidState, "An assignee is required.");
    var assignee = ActiveMember(assigneeId);
    Rules.Require(
      (assignee.Role == MemberRole.Child && reviewer.IsAdministrator)
        || (assignee.Role == MemberRole.Guardian
          && (reviewer.Role == MemberRole.Child || (reviewer.IsAdministrator && reviewer.Id != assigneeId))),
      DomainError.Forbidden,
      "Administrators review children and other guardians; children review guardians."
    );
  }

  private void RequireNewOperation(TransactionId operationId)
  {
    ArgumentNullException.ThrowIfNull(operationId);
    Rules.Require(!_operations.Contains(operationId), DomainError.Duplicate, "This operation was already applied.");
  }

  private FamilyEconomyMember ActiveMember(FamilyMemberId id)
  {
    ArgumentNullException.ThrowIfNull(id);
    var member = _members.FirstOrDefault(item => item.Id == id);
    Rules.Require(
      member is not null && !member.InvitationPending,
      DomainError.Forbidden,
      "An active family member is required."
    );

    return member!;
  }

  private void RequireAdministrator(FamilyMemberId actor)
  {
    Rules.Require(
      ActiveMember(actor).IsAdministrator,
      DomainError.Forbidden,
      "Only an administrator may manage this content."
    );
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
