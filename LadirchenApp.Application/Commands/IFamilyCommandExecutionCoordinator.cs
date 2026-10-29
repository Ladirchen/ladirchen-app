using System;
using System.Threading;
using System.Threading.Tasks;

namespace LadirchenApp.Application.Commands;

public interface IFamilyCommandExecutionCoordinator
{
  Task<FamilyCommandResult> ExecuteOnceAsync(
    FamilyCommand command,
    Func<CancellationToken, Task<FamilyCommandResult>> execute,
    CancellationToken cancellationToken
  );
}
