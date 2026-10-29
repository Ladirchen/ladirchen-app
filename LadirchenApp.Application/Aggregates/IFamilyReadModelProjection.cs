namespace LadirchenApp.Application.Aggregates;

public interface IFamilyReadModelProjection
{
  string ReadModelType { get; }
  long Revision { get; }
}
