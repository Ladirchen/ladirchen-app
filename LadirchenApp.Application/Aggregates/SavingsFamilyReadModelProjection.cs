using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;

namespace LadirchenApp.Application.Aggregates;

public sealed record SavingsFamilyReadModelProjection(
  long Revision,
  FamilyEconomy Economy,
  FamilyEconomyPresentationState Presentation,
  SavingsReadModelProjection CalculatedState,
  FamilyCurrency Currency,
  ExchangeRate ExchangeRate
) : IFamilyReadModelProjection
{
  public string ReadModelType
  {
    get
    {
      return "savings";
    }
  }
}
