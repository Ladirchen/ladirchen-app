Feature: Savings interest credit
  Interest credit is awarded only to an eligible savings balance.
  It preserves the minimum credit without exceeding the savings target.

  Scenario: Credits preserve the minimum and target cap
    When interest on <saved> saved toward <target> at <rate> percent is calculated
    Then the interest credit is <credit> Ladirchen
    Examples:
      | saved | target | rate | credit |
      | 0     | 100    | 8    | 0      |
      | 100   | 100    | 8    | 0      |
      | 10    | 100    | 0    | 1      |
      | 30    | 100    | 5    | 2      |
      | 99.5  | 100    | 8    | 0.5    |
