using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Family;

namespace LadirchenApp.Application.Commands;

internal interface ITypedFamilyCommandAreaHandler
{
  bool CanHandle(FamilyCommand command);

  Task<FamilyCommandResult> HandleAsync(
    FamilyCommand command,
    LoadedAggregate<Family> family,
    CancellationToken cancellationToken
  );
}
