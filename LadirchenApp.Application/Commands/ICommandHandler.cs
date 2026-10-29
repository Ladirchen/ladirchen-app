using System.Threading;
using System.Threading.Tasks;

namespace LadirchenApp.Application.Commands;

public interface ICommandHandler<in TCommand, TResult>
{
  Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
