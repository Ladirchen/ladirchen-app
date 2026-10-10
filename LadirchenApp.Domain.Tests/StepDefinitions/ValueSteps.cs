using System;
using System.Threading.Tasks;

using LadirchenApp.Domain.Shared;

using Reqnroll;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

[Binding]
public sealed class ValueSteps
{
  private LadirchenAmount? _amount;
  private Exception? _error;

  [Given("an amount of {float} Ladirchen")]
  public void GivenAmount(decimal value)
  {
    _amount = new LadirchenAmount(value);
  }

  [When("I construct an amount of {float} Ladirchen")]
  public void ConstructAmount(decimal value)
  {
    try
    {
      _amount = new LadirchenAmount(value);
    }
    catch (ArgumentOutOfRangeException exception)
    {
      _error = exception;
    }
  }

  [Then("it equals another amount of {float} Ladirchen")]
  public async Task EqualAmount(decimal value)
  {
    await Assert.That(_amount == new LadirchenAmount(value)).IsTrue();
  }

  [Then("the amount is rejected")]
  public async Task RejectedAmount()
  {
    await Assert.That(_error is ArgumentOutOfRangeException).IsTrue();
  }
}
