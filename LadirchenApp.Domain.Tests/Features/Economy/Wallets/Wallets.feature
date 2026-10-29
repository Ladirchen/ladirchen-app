Feature: Member wallets and Ladirchen transfers
  A wallet separates total balance, reserved funds and available funds.
  Transfers conserve earned Ladirchen unless a family member who can manage content creates a special gift.

  Scenario: Child gifts transfer rather than create money
    Given a core economy
    And the child has 100 Ladirchen
    When the child gifts 25 Ladirchen to the sibling
    Then the child wallet contains 75 Ladirchen
    And the sibling wallet contains 25 Ladirchen
    And the transfer ledger has matching debit and credit

  Scenario: Reserved funds cannot be deposited into goals
    Given a core economy
    And the child has 100 Ladirchen
    And the child creates a "Private" goal targeting 200 Ladirchen
    And a published reward costs 80 Ladirchen with stock 1
    And the child requests the reward
    When the child tries to deposit 30 Ladirchen
    Then the economy command fails with "InsufficientFunds"
    And the goal contains 5 Ladirchen
    And the rejected command left the economy unchanged

  Scenario: Reserved funds cannot be gifted
    Given a core economy
    And the child has 100 Ladirchen
    And a published reward costs 80 Ladirchen with stock 1
    And the child requests the reward
    When the child tries to gift 30 Ladirchen
    Then the economy command fails with "InsufficientFunds"
    And the sibling wallet contains 0 Ladirchen
    And the rejected command left the economy unchanged

  Scenario: Special gifts are rounded and limited to the supported range [entered=-5; credited=1]
    Given a guardian administrator enters a special gift of -5 Ladirchen with a reason
    When the special gift amount is normalized
    Then 1 Ladirchen are credited to the child

  Scenario: Special gifts are rounded and limited to the supported range [entered=1.4; credited=1]
    Given a guardian administrator enters a special gift of 1.4 Ladirchen with a reason
    When the special gift amount is normalized
    Then 1 Ladirchen are credited to the child

  Scenario: Special gifts are rounded and limited to the supported range [entered=1.5; credited=2]
    Given a guardian administrator enters a special gift of 1.5 Ladirchen with a reason
    When the special gift amount is normalized
    Then 2 Ladirchen are credited to the child

  Scenario: Special gifts are rounded and limited to the supported range [entered=12000; credited=10000]
    Given a guardian administrator enters a special gift of 12000 Ladirchen with a reason
    When the special gift amount is normalized
    Then 10000 Ladirchen are credited to the child

  Scenario: A special gift requires content-management permission and a reason
    Given a family member attempts to create a special gift
    When the member lacks content-management permission or provides a blank reason
    Then no Ladirchen are credited

  Scenario: Only supported family currency settings are stored [currency=CHF; rate=10; result=stored]
    Given a guardian administrator configures family currency "CHF" and an exchange rate of 10
    When the family currency settings are saved
    Then the settings are stored

  Scenario: Only supported family currency settings are stored [currency=EUR; rate=10.4; result=stored with rate 10]
    Given a guardian administrator configures family currency "EUR" and an exchange rate of 10.4
    When the family currency settings are saved
    Then the settings are stored with rate 10

  Scenario: Only supported family currency settings are stored [currency=HUF; rate=1; result=stored]
    Given a guardian administrator configures family currency "HUF" and an exchange rate of 1
    When the family currency settings are saved
    Then the settings are stored

  Scenario: Only supported family currency settings are stored [currency=USD; rate=10; result=rejected]
    Given a guardian administrator configures family currency "USD" and an exchange rate of 10
    When the family currency settings are saved
    Then the settings are rejected

  Scenario: Only supported family currency settings are stored [currency=CHF; rate=0; result=rejected]
    Given a guardian administrator configures family currency "CHF" and an exchange rate of 0
    When the family currency settings are saved
    Then the settings are rejected
