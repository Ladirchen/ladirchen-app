using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Family.Permissions;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed partial class FamilyEconomy
{
  public ShopRewardAvailability GetShopRewardAvailability(
    ShopRewardId rewardId,
    DateTimeOffset now
  )
  {
    var reward = FindShopReward(rewardId);
    var localDate = _timeZone.LocalDate(now);
    var publicationStatus = reward.PublicationStatus(localDate);
    var redemptionOpen = ShopAvailabilityPolicy.RedemptionIsOpen(now, _timeZone);
    var pendingRequest = _requests.SingleOrDefault(item =>
      item.RewardId == rewardId && item.Status == RedemptionStatus.Pending
    );
    var requestableByMemberIds = RequestableMemberIds(
      reward,
      publicationStatus,
      redemptionOpen
    );
    IReadOnlyList<FamilyMemberId> cancellableByMemberIds = pendingRequest is null
      ? []
      : [pendingRequest.RequesterId];
    var approvableByMemberIds = ApprovableMemberIds(pendingRequest, redemptionOpen);

    return new ShopRewardAvailability(
      publicationStatus,
      reward.NextPublicationChangeDate(localDate),
      requestableByMemberIds,
      cancellableByMemberIds,
      approvableByMemberIds
    );
  }

  private IReadOnlyList<FamilyMemberId> RequestableMemberIds(
    ShopReward reward,
    ShopRewardPublicationStatus publicationStatus,
    bool redemptionOpen
  )
  {
    if (
      !redemptionOpen
      || publicationStatus != ShopRewardPublicationStatus.Visible
      || reward.Status != ShopRewardStatus.Available
      || reward.Quantity <= 0
    )
    {
      return [];
    }

    return _members
      .Where(member => !member.InvitationPending && member.Role == MemberRole.Child)
      .Where(member => WalletFor(member.Id).Available >= reward.Price.Value)
      .Select(member => member.Id)
      .ToArray();
  }

  private IReadOnlyList<FamilyMemberId> ApprovableMemberIds(
    RedemptionRequest? pendingRequest,
    bool redemptionOpen
  )
  {
    if (pendingRequest is null || !redemptionOpen)
    {
      return [];
    }

    var permissionPolicy = new FamilyPermissionPolicy();

    return _members
      .Where(member => permissionPolicy.Resolve(member.Role, member.Access, member.InvitationPending).ManageContent)
      .Select(member => member.Id)
      .ToArray();
  }
}