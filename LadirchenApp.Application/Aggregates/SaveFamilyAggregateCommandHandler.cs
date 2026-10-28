using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LadirchenApp.Application.Aggregates;

public sealed class SaveFamilyAggregateCommandHandler
{
  private readonly IFamilyAggregateSnapshotRepository _repository;
  private readonly IFamilyAggregateChangePublisher _publisher;

  public SaveFamilyAggregateCommandHandler(
    IFamilyAggregateSnapshotRepository repository,
    IFamilyAggregateChangePublisher publisher
  )
  {
    _repository = repository;
    _publisher = publisher;
  }

  public async Task<FamilyAggregateSnapshot> HandleAsync(
    SaveFamilyAggregateCommand command,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(command);

    if (!FamilyAggregateSchema.TryGetVersion(command.AggregateType, out var schemaVersion))
    {
      throw new ArgumentException("The aggregate type is not supported.", nameof(command));
    }

    if (command.ExpectedRevision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(command), "The expected revision cannot be negative.");
    }

    if (command.State.ValueKind != JsonValueKind.Object)
    {
      throw new ArgumentException("Aggregate state must be a JSON object.", nameof(command));
    }

    var saved = await _repository.SaveAsync(command, schemaVersion, cancellationToken);
    await _publisher.PublishAsync(saved, cancellationToken);

    return saved;
  }
}
