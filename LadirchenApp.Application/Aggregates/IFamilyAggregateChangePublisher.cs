using System.Threading;
using System.Threading.Tasks;

namespace LadirchenApp.Application.Aggregates;

public interface IFamilyAggregateChangePublisher
{
  Task PublishAsync(FamilyAggregateSnapshot snapshot, CancellationToken cancellationToken);
}
