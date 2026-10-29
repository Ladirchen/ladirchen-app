using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;

namespace LadirchenApp.Webhost.Tests.Infrastructure.Authentication;

internal sealed class NoOpFamilyAggregateChangePublisher : IFamilyAggregateChangePublisher
{
  public Task PublishAsync(FamilyAggregateSnapshot snapshot, CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}