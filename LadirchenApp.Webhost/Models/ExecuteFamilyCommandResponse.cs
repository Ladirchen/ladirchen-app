using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Application.Commands;

namespace LadirchenApp.Webhost.Models;

public sealed record ExecuteFamilyCommandResponse(IReadOnlyList<FamilyAggregateSnapshotResponse> ChangedAggregates)
{
  public static ExecuteFamilyCommandResponse FromResult(FamilyCommandResult result)
  {
    return new ExecuteFamilyCommandResponse(
      result.ChangedAggregates.Select(FamilyAggregateSnapshotResponse.FromSnapshot).ToArray()
    );
  }
}
