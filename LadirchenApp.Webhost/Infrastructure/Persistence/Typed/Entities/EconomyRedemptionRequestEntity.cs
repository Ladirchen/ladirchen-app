using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class EconomyRedemptionRequestEntity
{
  public Guid FamilyId { get; set; }
  public string RequestId { get; set; } = string.Empty;
  public string RewardId { get; set; } = string.Empty;
  public string RequesterMemberId { get; set; } = string.Empty;
  public decimal Price { get; set; }
  public DateTimeOffset RequestedAt { get; set; }
  public int Status { get; set; }
  public DateTimeOffset? DecidedAt { get; set; }
  public FamilyEconomyRootEntity Economy { get; set; } = null!;
}
