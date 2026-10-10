Feature: Domain values
  Scenario: Amounts have value equality
    Given an amount of 12.5 Ladirchen
    Then it equals another amount of 12.5 Ladirchen

  Scenario: Negative currency is invalid
    When I construct an amount of -1 Ladirchen
    Then the amount is rejected
