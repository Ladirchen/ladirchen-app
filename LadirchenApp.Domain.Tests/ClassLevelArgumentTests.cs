using System.Threading.Tasks;

namespace LadirchenApp.Domain.Tests;

[Arguments(10)]
[Arguments(100)]
public class ClassLevelArgumentTests
{
  private readonly int _divisor;

  public ClassLevelArgumentTests(int divisor)
  {
    _divisor = divisor;
  }

  [Test]
  [Arguments(100)]
  [Arguments(50)]
  public async Task Divide_WithClassAndMethodArguments(int dividend)
  {
    var calculator = new Calculator();
    var result = calculator.Divide(dividend, _divisor);

    await Assert.That(result).IsGreaterThan(0);
  }
}
