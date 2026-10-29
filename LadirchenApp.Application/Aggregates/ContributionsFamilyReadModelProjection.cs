using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;

namespace LadirchenApp.Application.Aggregates;

public sealed record ContributionsFamilyReadModelProjection(
  long Revision,
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
