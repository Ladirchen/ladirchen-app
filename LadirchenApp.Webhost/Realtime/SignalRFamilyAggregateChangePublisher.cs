using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Webhost.Models;

using Microsoft.AspNetCore.SignalR;

namespace LadirchenApp.Webhost.Realtime;

public sealed class SignalRFamilyAggregateChangePublisher : IFamilyAggregateChangePublisher
{
  private readonly IHubContext<FamilyUpdatesHub> _hubContext;

  public SignalRFamilyAggregateChangePublisher(IHubContext<FamilyUpdatesHub> hubContext)
  {
    _hubContext = hubContext;
  }

  public Task PublishAsync(FamilyAggregateSnapshot snapshot, CancellationToken cancellationToken)
  {
    return _hubContext.Clients
      .Group(FamilyUpdatesHub.GroupName(snapshot.FamilyId.Value))
      .SendAsync(
        "FamilyAggregateChanged",
        FamilyAggregateChangedMessage.FromSnapshot(snapshot),
        cancellationToken
      );
  }
}
