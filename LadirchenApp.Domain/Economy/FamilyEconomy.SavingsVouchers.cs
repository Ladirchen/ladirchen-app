using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed partial class FamilyEconomy
{
  private readonly List<GoalSavingsVoucher> _savingsVouchers = new();

  public IReadOnlyList<GoalSavingsVoucher> SavingsVouchers
  {
    get
    {
      return _savingsVouchers.AsReadOnly();
    }
  }

  public bool ConsentSavingsVoucher(
    FamilyMemberId actor,
    SavingGoalId voucherId,
    DateTimeOffset now
  )
  {
    _family.ActiveMember(actor);
    var voucher = FindSavingsVoucher(voucherId);
    Rules.Require(voucher.OwnerId is null, DomainError.InvalidState, "A personal voucher needs no family consent.");
    Rules.Require(voucher.RedemptionCount == 0, DomainError.InvalidState, "A redeemed voucher cannot receive consent.");
    Rules.Require(
      voucher.Participants.Contains(actor),
      DomainError.Forbidden,
      "Only a voucher participant may consent."
    );
    var changed = voucher.Consent(actor);

    if (changed)
    {
      AddDomainEvent(new GoalChanged(Id, voucher.Id, "VoucherConsentChanged", 0, now));
    }

    return changed;
  }

  public void RedeemSavingsVoucher(
    FamilyMemberId actor,
    SavingGoalId voucherId,
    DateTimeOffset now
  )
  {
    _family.ActiveMember(actor);
    var voucher = FindSavingsVoucher(voucherId);
    Rules.Require(
      voucher.OwnerId == actor || voucher.Participants.Contains(actor),
      DomainError.Forbidden,
      "Only the voucher owner or a participating family member may redeem it."
    );
    Rules.Require(
      voucher.Redeem(now),
      DomainError.InvalidState,
      "The savings voucher is not redeemable or was already redeemed."
    );
    AddDomainEvent(new GoalChanged(Id, voucher.Id, "VoucherRedeemed", voucher.Amount, now));
  }

  public void ConsumeGuardianGift(FamilyMemberId actor, GuardianGiftId giftId)
  {
    _family.ActiveMember(actor);
    ArgumentNullException.ThrowIfNull(giftId);
    var gift = _guardianGifts.Find(item => item.Id == giftId)
      ?? throw new DomainRuleException(DomainError.NotFound, "The pending gift does not exist.");
    Rules.Require(gift.OwnerId == actor, DomainError.Forbidden, "Only the recipient may acknowledge this gift.");
    _guardianGifts.Remove(gift);
  }

  private void IssueVoucherIfCompleted(SavingGoal goal)
  {
    if (!goal.Completed || _savingsVouchers.Exists(item => item.Id == goal.Id))
    {
      return;
    }

    _savingsVouchers.Add(
      new GoalSavingsVoucher(
        goal.Id,
        goal.Saved,
        goal.Owner.MemberId,
        goal.Participants
      )
    );
  }

  private void AddPendingGift(GuardianGoalGift gift)
  {
    ArgumentNullException.ThrowIfNull(gift);
    Rules.Require(
      gift.Id is null || _guardianGifts.All(item => item.Id != gift.Id),
      DomainError.Duplicate,
      "This pending gift identity was already used."
    );
    _guardianGifts.Add(gift);
  }

  private void RequireNewPendingGiftId(GuardianGiftId? giftId)
  {
    Rules.Require(
      giftId is null || _guardianGifts.All(item => item.Id != giftId),
      DomainError.Duplicate,
      "This pending gift identity was already used."
    );
  }

  private GoalSavingsVoucher FindSavingsVoucher(SavingGoalId id)
  {
    ArgumentNullException.ThrowIfNull(id);

    return _savingsVouchers.Find(item => item.Id == id)
      ?? throw new DomainRuleException(DomainError.NotFound, "The savings voucher does not exist.");
  }
}
