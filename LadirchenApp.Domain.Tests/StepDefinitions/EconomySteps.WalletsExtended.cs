using System;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using Reqnroll;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

public sealed partial class EconomySteps
{
  private readonly SpecialGiftPolicy _specialGiftPolicy = new();
  private readonly FamilyCurrencySettingsPolicy _currencyPolicy = new();
  private decimal _enteredGift;
  private decimal _normalizedGift;
  private string _currency = string.Empty;
  private decimal _enteredRate;
  private bool _settingsStored;
  private ExchangeRate? _storedRate;

  [Given("a guardian administrator enters a special gift of {float} Ladirchen with a reason")]
  public void EnterSpecialGift(decimal amount)
  {
    CreateEconomy();
    _enteredGift = amount;
  }

  [When("the special gift amount is normalized")]
  public void NormalizeGift()
  {
    _normalizedGift = _specialGiftPolicy.Normalize(_enteredGift);
    _economy.GiveSpecialGift(_admin, _child, new LadirchenAmount(_normalizedGift), "Gift", NextOperation(), _now);
  }

  [Then("{float} Ladirchen are credited to the child")]
  public async Task NormalizedGiftCredited(decimal amount)
  {
    await Assert.That(_economy.WalletFor(_child).Balance).IsEqualTo(amount);
  }

  [Given("a family member attempts to create a special gift")]
  public void AttemptSpecialGift()
  {
    CreateEconomy();
  }

  [When("the member lacks content-management permission or provides a blank reason")]
  public void InvalidSpecialGift()
  {
    Try(() => _economy.GiveSpecialGift(_supporter, _child, new LadirchenAmount(10), "Gift", NextOperation(), _now));
  }

  [Then("no Ladirchen are credited")]
  public async Task NoGiftCredited()
  {
    await Assert.That(_economy.WalletFor(_child).Balance).IsEqualTo(0);
  }

  [Given("a guardian administrator configures family currency {string} and an exchange rate of {float}")]
  public void ConfigureCurrency(string currency, decimal rate)
  {
    CreateEconomy();
    _currency = currency;
    _enteredRate = rate;
  }

  [When("the family currency settings are saved")]
  public void SaveCurrencySettings()
  {
    _settingsStored = _currencyPolicy.TryNormalize(_currency, _enteredRate, out var currency, out _storedRate);

    if (_settingsStored)
    {
      _family.UpdateSettings(_admin, _family.TimeZone, currency, _storedRate!, _now);
    }
  }

  [Then("the settings are {word}")]
  public async Task SettingsResult(string result)
  {
    await Assert.That(_settingsStored).IsEqualTo(result == "stored");
  }

  [Then("the settings are stored with rate {int}")]
  public async Task SettingsStoredWithRate(int rate)
  {
    await Assert.That(_settingsStored).IsTrue();
    await Assert.That(_storedRate?.LadirchenPerCurrencyUnit).IsEqualTo(rate);
  }
}
