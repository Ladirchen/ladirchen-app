using System;

namespace LadirchenApp.Domain.House;

internal sealed record DailyHouseEnergyState(DateOnly Date, int Energy);
