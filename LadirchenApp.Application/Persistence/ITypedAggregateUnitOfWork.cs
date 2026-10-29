using System.Threading;
using System.Threading.Tasks;

namespace LadirchenApp.Application.Persistence;

public interface ITypedAggregateUnitOfWork
{
  Task<TypedAggregateSaveResult> SaveAsync(
    TypedAggregateSaveBatch batch,
    CancellationToken cancellationToken
  );
}
