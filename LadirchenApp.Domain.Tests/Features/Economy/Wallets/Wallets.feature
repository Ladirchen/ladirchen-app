Feature: Member wallets and Ladirchen transfers
  A wallet separates total balance, reserved funds and available funds.
  Transfers conserve earned Ladirchen unless an authorized guardian creates a special gift.

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

  Scenario: Special gifts are rounded and limited to the supported range
    Given an administrator enters a special gift of <entered> Ladirchen with a reason
    When the special gift amount is normalized
    Then <credited> Ladirchen are credited to the child
    Examples:
      | entered | credited |
      | -5      | 1        |
      | 1.4     | 1        |
      | 1.5     | 2        |
      | 12000   | 10000    |

  Scenario: A special gift requires content-management permission and a reason
    Given a family member attempts to create a special gift
    When the member lacks content-management permission or provides a blank reason
    Then no Ladirchen are credited

  Scenario: Only supported family currency settings are stored
    Given an administrator configures family currency "<currency>" and an exchange rate of <rate>
    When the family currency settings are saved
    Then the settings are <result>
    Examples:
      | currency | rate | result   |
      | CHF      | 10   | stored   |
      | EUR      | 10.4 | stored with rate 10 |
      | HUF      | 1    | stored   |
      | USD      | 10   | rejected |
      | CHF      | 0    | rejected |
