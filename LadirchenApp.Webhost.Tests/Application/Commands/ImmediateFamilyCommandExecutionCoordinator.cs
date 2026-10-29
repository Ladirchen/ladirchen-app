using System;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Commands;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

internal sealed class ImmediateFamilyCommandExecutionCoordinator : IFamilyCommandExecutionCoordinator
{
  public async Task<FamilyCommandResult> ExecuteOnceAsync(
    FamilyCommand command,
    Func<CancellationToken, Task<FamilyCommandResult>> execute,
    CancellationToken cancellationToken
  )
  {
    return await execute(cancellationToken);
  }
}