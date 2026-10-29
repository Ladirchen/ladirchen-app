using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed partial class FamilyEconomy
{
  private readonly List<ShopReward> _shopRewards = new();
  private readonly List<RedemptionRequest> _requests = new();
  public IReadOnlyList<ShopReward> ShopRewards
  {
    get
    {
      return _shopRewards.Where(item => !item.Deleted).ToList().AsReadOnly();
    }
  }

  public IReadOnlyList<RedemptionRequest> RedemptionRequests
  {
    get
    {
      return _requests.AsReadOnly();
    }
  }

  public void AddShopReward(FamilyMemberId actor, ShopRewardId id, ShopRewardDefinition definition, DateTimeOffset now)
  {
    ArgumentNullException.ThrowIfNull(definition);
    _family.RequireAdministrator(actor);
    Rules.Require(
      _shopRewards.All(item => item.Id != id),
      DomainError.Duplicate,
      "This reward identity was already used."
    );
    var reward = new ShopReward(id, definition);
    _shopRewards.Add(reward);
    AddDomainEvent(new ShopCatalogChanged(Id, id, "Added", now));
  }

  public void UpdateShopReward(
    FamilyMemberId actor,
    ShopRewardId id,
    string title,
    LadirchenAmount price,
    int quantity,
    AvailabilityWindow window,
    DateTimeOffset now
  )
  {
    _family.RequireAdministrator(actor);
    var reward = FindShopReward(id);
    Rules.Require(
      reward.Status != ShopRewardStatus.Requested,
      DomainError.InvalidState,
      "Pending requests lock price and stock."
    );
    var text = Rules.Text(title);
    ArgumentNullException.ThrowIfNull(price);
    ArgumentNullException.ThrowIfNull(window);
    ArgumentOutOfRangeException.ThrowIfNegative(quantity);
    reward.Update(text, price, quantity, window);
    AddDomainEvent(new ShopCatalogChanged(Id, id, "Updated", now));
  }

  public void SetShopRewardVisibility(FamilyMemberId actor, ShopRewardId id, bool visible, DateTimeOffset now)
  {
    _family.RequireAdministrator(actor);
    var reward = FindShopReward(id);
    reward.SetVisibility(visible);
    AddDomainEvent(new ShopCatalogChanged(Id, id, "VisibilityChanged", now));
  }

  public void RequestShopReward(
    FamilyMemberId actor,
    ShopRewardId rewardId,
    RedemptionRequestId requestId,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    RequireChild(actor);
    ArgumentNullException.ThrowIfNull(requestId);
    var reward = FindShopReward(rewardId);
    Rules.Require(
      reward.Status == ShopRewardStatus.Available && reward.Quantity > 0,
      DomainError.InvalidState,
      "The offer must be available and in stock."
    );
    Rules.Require(reward.IsPublished(now, _family.TimeZone), DomainError.Closed, "The offer is not published today.");
    RequireShopOpen(now);
    Rules.Require(
      _requests.All(item => item.Id != requestId),
      DomainError.Duplicate,
      "This request identity was already used."
    );
    RequireNewOperation(operationId);
    var wallet = WalletFor(actor);
    Rules.Require(
      wallet.Available >= reward.Price.Value,
      DomainError.InsufficientFunds,
      "The purchase exceeds available funds."
    );
    var reserved = checked(wallet.Reserved + reward.Price.Value);
    var request = new RedemptionRequest(requestId, rewardId, actor, reward.Price, now);
    wallet.Set(wallet.Balance, reserved);
    reward.Reserve();
    _requests.Add(request);
    Record(
      operationId,
      new LedgerEntry(
        operationId,
        actor,
        null,
        LedgerKind.Reserve,
        reward.Price.Value,
        requestId,
        "Shop reservation",
        now.ToUniversalTime()
      )
    );
    AddDomainEvent(new ShopRewardRequested(Id, requestId, actor, rewardId, reward.Price.Value, now));
  }

  public void CancelShopRewardRequest(
    FamilyMemberId actor,
    RedemptionRequestId requestId,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    RequireChild(actor);
    var request = FindPendingRequest(requestId);
    Rules.Require(
      request.RequesterId == actor,
      DomainError.Forbidden,
      "Only the requester may cancel their reservation."
    );
    ReleaseRequest(actor, request, RedemptionStatus.Cancelled, operationId, now);
  }

  public void RejectShopReward(
    FamilyMemberId actor,
    RedemptionRequestId requestId,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    _family.RequireAdministrator(actor);
    ReleaseRequest(actor, FindPendingRequest(requestId), RedemptionStatus.Rejected, operationId, now);
  }

  public void ApproveShopReward(
    FamilyMemberId actor,
    RedemptionRequestId requestId,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    _family.RequireAdministrator(actor);
    RequireShopOpen(now);
    var request = FindPendingRequest(requestId);
    RequireNewOperation(operationId);
    var reward = FindShopReward(request.RewardId);
    var wallet = WalletFor(request.RequesterId);
    var price = request.Price.Value;
    Rules.Require(
      reward.Quantity > 0 && wallet.Reserved >= price && wallet.Balance >= price,
      DomainError.InvalidState,
      "The reservation and stock must still cover the captured price."
    );
    wallet.Set(wallet.Balance - price, wallet.Reserved - price);
    reward.Redeem();
    request.Decide(RedemptionStatus.Approved, now);
    Record(
      operationId,
      new LedgerEntry(
        operationId,
        request.RequesterId,
        null,
        LedgerKind.Capture,
        price,
        requestId,
        "Shop redemption",
        now.ToUniversalTime(),
        actor
      )
    );
    AddDomainEvent(new ShopRewardDecided(Id, requestId, actor, RedemptionStatus.Approved, price, now));
  }

  public void DeleteShopReward(FamilyMemberId actor, ShopRewardId id, TransactionId operationId, DateTimeOffset now)
  {
    _family.RequireAdministrator(actor);
    var reward = FindShopReward(id);
    var request = _requests.Find(item => item.RewardId == id && item.Status == RedemptionStatus.Pending);

    if (request is not null)
    {
      ReleaseRequest(actor, request, RedemptionStatus.Cancelled, operationId, now);
    }
    else
    {
      RequireNewOperation(operationId);
      Record(operationId);
    }

    reward.Delete();
    AddDomainEvent(new ShopCatalogChanged(Id, id, "Deleted", now));
  }

  private void ReleaseRequest(
    FamilyMemberId actor,
    RedemptionRequest request,
    RedemptionStatus decision,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    RequireNewOperation(operationId);
    var reward = FindShopReward(request.RewardId);
    var wallet = WalletFor(request.RequesterId);
    Rules.Require(wallet.Reserved >= request.Price.Value, DomainError.InvalidState, "The reserved funds are missing.");
    wallet.Set(wallet.Balance, wallet.Reserved - request.Price.Value);
    reward.Release();
    request.Decide(decision, now);
    Record(
      operationId,
      new LedgerEntry(
        operationId,
        request.RequesterId,
        null,
        LedgerKind.Release,
        request.Price.Value,
        request.Id,
        "Shop reservation released",
        now.ToUniversalTime(),
        actor
      )
    );
    AddDomainEvent(new ShopRewardDecided(Id, request.Id, actor, decision, request.Price.Value, now));
  }

  private ShopReward FindShopReward(ShopRewardId id)
  {
    ArgumentNullException.ThrowIfNull(id);

    return _shopRewards.Find(item => item.Id == id && !item.Deleted)
      ?? throw new DomainRuleException(DomainError.NotFound, "The offer does not exist.");
  }

  private RedemptionRequest FindPendingRequest(RedemptionRequestId id)
  {
    ArgumentNullException.ThrowIfNull(id);
    var request = _requests.Find(item => item.Id == id);
    Rules.Require(
      request is not null && request.Status == RedemptionStatus.Pending,
      DomainError.InvalidState,
      "A pending redemption request is required."
    );

    return request!;
  }

  private void RequireShopOpen(DateTimeOffset now)
  {
    Rules.Require(
      ShopAvailabilityPolicy.RedemptionIsOpen(now, _family.TimeZone),
      DomainError.Closed,
      $"Shop requests and approvals close at {ShopAvailabilityPolicy.ClosingHour:00}:00 local time."
    );
  }
}
