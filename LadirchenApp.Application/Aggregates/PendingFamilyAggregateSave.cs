namespace LadirchenApp.Application.Aggregates;

public sealed record PendingFamilyAggregateSave(SaveFamilyAggregateCommand Command, int SchemaVersion);
