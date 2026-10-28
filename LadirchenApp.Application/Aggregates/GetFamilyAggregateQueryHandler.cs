using System;
using System.Threading;
using System.Threading.Tasks;

namespace LadirchenApp.Application.Aggregates;

public sealed class GetFamilyAggregateQueryHandler
{
  private readonly IFamilyAggregateSnapshotRepository _repository;

  public GetFamilyAggregateQueryHandler(IFamilyAggregateSnapshotRepository repository)
  {
    _repository = repository;
  }

  public Task<FamilyAggregateSnapshot?> HandleAsync(
    GetFamilyAggregateQuery query,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(query);

    if (!FamilyAggregateSchema.TryGetVersion(query.AggregateType, out _))
    {
      throw new ArgumentException("The aggregate type is not supported.", nameof(query));
    }

    return _repository.GetAsync(query.FamilyId, query.AggregateType, cancellationToken);
  }
}
