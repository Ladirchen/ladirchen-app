using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed record GetFamilyBootstrapQuery(
  FamilyId FamilyId,
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
