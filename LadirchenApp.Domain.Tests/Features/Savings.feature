Feature: Saving goals and gifts
  Scenario Outline: Personal goals have a bounded starter bonus
    Given a core economy
    When the child creates a "Private" goal targeting <target> Ladirchen
    Then the goal contains <saved> Ladirchen
    And the goal allows withdrawing <withdrawable> Ladirchen
    Examples:
      | target | saved | withdrawable |
      | 100    | 5     | 0            |
      | 3      | 3     | 3            |

  Scenario: Deposits and withdrawals conserve earned funds
    Given a core economy
    And the child has 100 Ladirchen
    And the child creates a "Private" goal targeting 200 Ladirchen
    When the child deposits 30 Ladirchen
    And the child withdraws 50 Ladirchen
    Then the goal contains 5 Ladirchen
    And the child wallet contains 100 Ladirchen

  Scenario: Cancelling an incomplete goal forfeits its bonus
    Given a core economy
    And the child has 100 Ladirchen
    And the child creates a "Private" goal targeting 200 Ladirchen
    And the child deposits 30 Ladirchen
    When the child cancels the goal
    Then the child wallet contains 100 Ladirchen
    And the cancelled goal is no longer visible
    And the ledger records 5 forfeited Ladirchen

  Scenario: Completion unlocks the bonus
    Given a core economy
    And the child has 100 Ladirchen
    And the child creates a "Private" goal targeting 35 Ladirchen
    And the child deposits 30 Ladirchen
    When the child withdraws 35 Ladirchen
    Then the child wallet contains 105 Ladirchen
    And the goal contains 0 Ladirchen

  Scenario: Private goals are private even from administrators
    Given a core economy
    And the child creates a "Private" goal targeting 100 Ladirchen
    When the administrator tries to support it
    Then the economy command fails with "Forbidden"
    And the goal contains 5 Ladirchen
    And the rejected command left the economy unchanged

  Scenario Outline: Supporters only support family-visible goals
    Given a core economy
    And the child creates a "<visibility>" goal targeting 100 Ladirchen
    When the supporter tries to support it
    Then the goal contains <saved> Ladirchen
    Examples:
      | visibility | saved |
      | Family     | 15    |
      | Guardians  | 5     |
      | Private    | 5     |

  Scenario: Child gifts transfer rather than create money
    Given a core economy
    And the child has 100 Ladirchen
    When the child gifts 25 Ladirchen to the sibling
    Then the child wallet contains 75 Ladirchen
    And the sibling wallet contains 25 Ladirchen
    And the transfer ledger has matching debit and credit

  Scenario: Targets cannot fall below savings
    Given a core economy
    And the child creates a "Family" goal targeting 100 Ladirchen
    When the child tries to reduce the goal target to 4 Ladirchen
    Then the economy command fails with "InvalidValue"
    And the rejected command left the economy unchanged
