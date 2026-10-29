namespace LadirchenApp.Application.Persistence;

public sealed record TypedAggregateSaveResult(
  LoadedAggregate<LadirchenApp.Domain.Family.Family>? Family,
  LoadedFamilyEconomy? FamilyEconomy,
  LoadedAggregate<LadirchenApp.Domain.House.FamilyHouse>? FamilyProgression,
  LoadedAggregate<LadirchenApp.Domain.House.Shop.FamilyHouseShop>? HouseCustomization
);
