using System.Threading.Tasks;

namespace LadirchenApp.Domain.Tests;

[ClassDataSource<InMemoryDb>(Shared = SharedType.PerTestSession)]
public class SharedDatabaseTests
{
  private readonly InMemoryDb _database;

  public SharedDatabaseTests(InMemoryDb db)
  {
    _database = db;
  }

  [ClassDataSource<Calculator>]
  public required Calculator Calculator
  {
    get; init;
  }

  [Test]
  public async Task Calculator_ResultCanBeStored()
  {
    var result = Calculator.Add(2, 3);
    await _database.SetAsync("result", result.ToString());

    await Assert.That(await _database.GetAsync("result")).IsEqualTo("5");
  }
}
