using System.Threading.Tasks;

namespace LadirchenApp.Domain.Tests;

public class DependencyInjectionTests
{
  // A new InMemoryDb is created for each test
  [Test]
  [ClassDataSource<InMemoryDb>]
  public async Task Database_SetAndGet(InMemoryDb db)
  {
    await db.SetAsync("key", "value");

    var result = await db.GetAsync("key");

    await Assert.That(result).IsEqualTo("value");
  }

  // The same InMemoryDb instance is shared across all tests in this class
  [Test]
  [ClassDataSource<InMemoryDb>(Shared = SharedType.PerClass)]
  public async Task Database_SharedPerClass(InMemoryDb db)
  {
    await db.SetAsync("shared", "data");

    var result = await db.GetAsync("shared");

    await Assert.That(result).IsNotNull();
  }
}
