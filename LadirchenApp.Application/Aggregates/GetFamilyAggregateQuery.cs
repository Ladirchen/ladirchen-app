using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed record GetFamilyAggregateQuery(
  FamilyId FamilyId,
  string AggregateType,
  FamilyMemberId AuthenticatedMemberId,
  FamilyMemberId? ViewerMemberId = null
)
{
  public FamilyMemberId ViewerId
  {
    get
    {
      return ViewerMemberId ?? AuthenticatedMemberId;
    }
  }
}
