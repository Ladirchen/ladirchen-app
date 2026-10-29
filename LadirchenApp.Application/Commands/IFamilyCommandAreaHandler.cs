using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;

namespace LadirchenApp.Application.Commands;

internal interface IFamilyCommandAreaHandler
{
  bool CanHandle(FamilyCommand command);

  Task<FamilyCommandResult> HandleAsync(
    FamilyCommand command,
    FamilyAggregateSnapshot profile,
    JsonObject profileState,
    JsonObject payload,
    CancellationToken cancellationToken
  );
}
