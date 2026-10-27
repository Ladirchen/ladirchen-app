Feature: Ladi-stage savings interest rate
  The configured maximum savings interest rate is distributed proportionally across all Ladi stages.
  A child's personal Ladi stage is the sole input to their savings interest rate.
  With N ordered stages, stage K receives maximum rate multiplied by K and divided by N.
  The calculated rate is rounded to two decimal places.

  Scenario: Six Ladi stages divide the configured maximum interest rate proportionally
    Given the maximum savings interest rate is configured as 8 percent
    And there are 6 ordered Ladi stages
    And the child's personal Ladi stage is "<stageId>" at position <stageNumber>
    When the child's personal savings interest rate is determined
    Then the savings interest rate is <interestRate> percent
    Examples:
      | stageId      | stageNumber | interestRate |
      | idle-ladi    | 1           | 1.33         |
      | starter-ladi | 2           | 2.67         |
      | garden-ladi  | 3           | 4            |
      | sun-ladi     | 4           | 5.33         |
      | aurora-ladi  | 5           | 6.67         |
      | super-ladi   | 6           | 8            |

  Scenario: Changing the maximum rate preserves proportional stage rates
    Given the maximum savings interest rate is configured as <maximumRate> percent
    And there are 6 ordered Ladi stages
    And the child's personal Ladi stage is at position <stageNumber>
    When the child's personal savings interest rate is determined
    Then the savings interest rate is <interestRate> percent
    Examples:
      | maximumRate | stageNumber | interestRate |
      | 6           | 1           | 1            |
      | 6           | 3           | 3            |
      | 6           | 6           | 6            |
      | 12          | 1           | 2            |
      | 12          | 5           | 10           |
      | 12          | 6           | 12           |

  Scenario: Super Ladi always receives the configured maximum interest rate
    Given the maximum savings interest rate is configured as 8 percent
    And the child's personal Ladi stage is "super-ladi"
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 8 percent

  Scenario: Participation inputs are not evaluated again by the interest policy
    Given the child's personal participation has already determined their Ladi stage
    When the child's personal savings interest rate is determined
    Then the interest policy does not evaluate streak, completion or average rating again

  Scenario: Unrelated configuration does not change a stage's interest rate
    Given the maximum savings interest rate is configured as 8 percent
    And the child's personal Ladi stage is "sun-ladi" at position 4
    And the <unrelatedSetting> is configured as <value>
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 5.33 percent
    Examples:
      | unrelatedSetting                      | value |
      | family house level                    | 4     |
      | five-star contribution bonus percent | 25    |
      | completed family weekly streak        | 10    |
