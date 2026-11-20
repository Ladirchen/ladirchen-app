using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed record ContributionsFamilyReadModelProjection(
  long Revision,
  FamilyMemberId ViewerId,
  MemberRole ViewerRole,
  FamilyEconomy Economy,
  FamilyEconomyPresentationState Presentation,
  ContributionReadModelProjection CalculatedState
) : IFamilyReadModelProjection
{
  public string ReadModelType
  {
    get
    {
      return "contributions";
    }
  }
}
