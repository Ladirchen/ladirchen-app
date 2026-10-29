using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed record ShopRewardAvailability
{
  public ShopRewardAvailability(
    ShopRewardPublicationStatus publicationStatus,
    DateOnly? nextPublicationChangeDate,
    IReadOnlyList<FamilyMemberId> requestableByMemberIds,
    IReadOnlyList<FamilyMemberId> cancellableByMemberIds,
    IReadOnlyList<FamilyMemberId> approvableByMemberIds
  )
  {
    ArgumentNullException.ThrowIfNull(requestableByMemberIds);
    ArgumentNullException.ThrowIfNull(cancellableByMemberIds);
    ArgumentNullException.ThrowIfNull(approvableByMemberIds);
    PublicationStatus = publicationStatus;
    NextPublicationChangeDate = nextPublicationChangeDate;
    RequestableByMemberIds = requestableByMemberIds;
    CancellableByMemberIds = cancellableByMemberIds;
    ApprovableByMemberIds = approvableByMemberIds;
  }

  public ShopRewardPublicationStatus PublicationStatus
  {
    get;
  }
  public DateOnly? NextPublicationChangeDate
  {
    get;
  }
  public IReadOnlyList<FamilyMemberId> RequestableByMemberIds
  {
    get;
  }
  public IReadOnlyList<FamilyMemberId> CancellableByMemberIds
  {
    get;
  }
  public IReadOnlyList<FamilyMemberId> ApprovableByMemberIds
  {
    get;
  }
  public bool Published
  {
    get
    {
      return PublicationStatus == ShopRewardPublicationStatus.Visible;
    }
  }
}