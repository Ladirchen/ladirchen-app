using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Avatar;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Family.Permissions;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed partial class GetFamilyAggregateQueryHandler
{
  private static FamilyAggregateSnapshot ProjectProgressionCapabilities(FamilyAggregateSnapshot progression)
  {
    var state = JsonNode.Parse(progression.State.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException("The family progression state must be an object.");
    var energy = state["familyEnergy"]?.GetValue<int>() ?? 0;
    var energyPolicy = new HouseEnergyPolicy();
    state["minimumSuccessfulWeekEnergy"] = HouseEnergyPolicy.MinimumSuccessfulWeekEnergy;
    state["canCloseWeekSuccessfully"] = energy >= HouseEnergyPolicy.MinimumSuccessfulWeekEnergy;
    state["houseAtmosphere"] = energyPolicy.Atmosphere(energy);
    state["houseVisualLevel"] = energyPolicy.VisualLevel(energy);

    using var document = JsonDocument.Parse(state.ToJsonString());

    return progression with { State = document.RootElement.Clone() };
  }

}
