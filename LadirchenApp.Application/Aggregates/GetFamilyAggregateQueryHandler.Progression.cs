using System;
using System.Text.Json;
using System.Text.Json.Nodes;

using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.House;

namespace LadirchenApp.Application.Aggregates;

public sealed partial class GetFamilyAggregateQueryHandler
{
  private static FamilyAggregateSnapshot ProjectProgressionCapabilities(FamilyAggregateSnapshot progression)
  {
    var state = JsonNode.Parse(progression.State.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException("The family progression state must be an object.");
    var energy = FamilyProgressionSnapshotMapper.ReadFamilyEnergy(state);
    var energyPolicy = new HouseEnergyPolicy();
    state["minimumSuccessfulWeekEnergy"] = HouseEnergyPolicy.MinimumSuccessfulWeekEnergy;
    state["canCloseWeekSuccessfully"] = energy >= HouseEnergyPolicy.MinimumSuccessfulWeekEnergy;
    state["houseAtmosphere"] = energyPolicy.Atmosphere(energy);
    state["houseVisualLevel"] = energyPolicy.VisualLevel(energy);

    using var document = JsonDocument.Parse(state.ToJsonString());

    return progression with { State = document.RootElement.Clone() };
  }

}
