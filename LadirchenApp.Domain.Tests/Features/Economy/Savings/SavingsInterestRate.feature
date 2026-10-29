Feature: Ladi-stage savings interest rate
  The configured maximum savings interest rate is distributed proportionally across all Ladi stages.
  A child's personal Ladi stage is the sole input to their savings interest rate.
  With N ordered stages, stage K receives maximum rate multiplied by K and divided by N.
  The calculated rate is rounded to two decimal places.

  Scenario: Six Ladi stages divide the configured maximum interest rate proportionally [stageId=idle-ladi; stageNumber=1; interestRate=1.33]
    Given the maximum savings interest rate is configured as 8 percent
    And there are 6 ordered Ladi stages
    And the child's personal Ladi stage is "idle-ladi" at position 1
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 1.33 percent

  Scenario: Six Ladi stages divide the configured maximum interest rate proportionally [stageId=starter-ladi; stageNumber=2; interestRate=2.67]
    Given the maximum savings interest rate is configured as 8 percent
    And there are 6 ordered Ladi stages
    And the child's personal Ladi stage is "starter-ladi" at position 2
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 2.67 percent

  Scenario: Six Ladi stages divide the configured maximum interest rate proportionally [stageId=garden-ladi; stageNumber=3; interestRate=4]
    Given the maximum savings interest rate is configured as 8 percent
    And there are 6 ordered Ladi stages
    And the child's personal Ladi stage is "garden-ladi" at position 3
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 4 percent

  Scenario: Six Ladi stages divide the configured maximum interest rate proportionally [stageId=sun-ladi; stageNumber=4; interestRate=5.33]
    Given the maximum savings interest rate is configured as 8 percent
    And there are 6 ordered Ladi stages
    And the child's personal Ladi stage is "sun-ladi" at position 4
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 5.33 percent

  Scenario: Six Ladi stages divide the configured maximum interest rate proportionally [stageId=aurora-ladi; stageNumber=5; interestRate=6.67]
    Given the maximum savings interest rate is configured as 8 percent
    And there are 6 ordered Ladi stages
    And the child's personal Ladi stage is "aurora-ladi" at position 5
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 6.67 percent

  Scenario: Six Ladi stages divide the configured maximum interest rate proportionally [stageId=super-ladi; stageNumber=6; interestRate=8]
    Given the maximum savings interest rate is configured as 8 percent
    And there are 6 ordered Ladi stages
    And the child's personal Ladi stage is "super-ladi" at position 6
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 8 percent

  Scenario: Changing the maximum rate preserves proportional stage rates [maximumRate=6; stageNumber=1; interestRate=1]
    Given the maximum savings interest rate is configured as 6 percent
    And there are 6 ordered Ladi stages
    And the child's personal Ladi stage is at position 1
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 1 percent

  Scenario: Changing the maximum rate preserves proportional stage rates [maximumRate=6; stageNumber=3; interestRate=3]
    Given the maximum savings interest rate is configured as 6 percent
    And there are 6 ordered Ladi stages
    And the child's personal Ladi stage is at position 3
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 3 percent

  Scenario: Changing the maximum rate preserves proportional stage rates [maximumRate=6; stageNumber=6; interestRate=6]
    Given the maximum savings interest rate is configured as 6 percent
    And there are 6 ordered Ladi stages
    And the child's personal Ladi stage is at position 6
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 6 percent

  Scenario: Changing the maximum rate preserves proportional stage rates [maximumRate=12; stageNumber=1; interestRate=2]
    Given the maximum savings interest rate is configured as 12 percent
    And there are 6 ordered Ladi stages
    And the child's personal Ladi stage is at position 1
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 2 percent

  Scenario: Changing the maximum rate preserves proportional stage rates [maximumRate=12; stageNumber=5; interestRate=10]
    Given the maximum savings interest rate is configured as 12 percent
    And there are 6 ordered Ladi stages
    And the child's personal Ladi stage is at position 5
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 10 percent

  Scenario: Changing the maximum rate preserves proportional stage rates [maximumRate=12; stageNumber=6; interestRate=12]
    Given the maximum savings interest rate is configured as 12 percent
    And there are 6 ordered Ladi stages
    And the child's personal Ladi stage is at position 6
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 12 percent

  Scenario: Super Ladi always receives the configured maximum interest rate
    Given the maximum savings interest rate is configured as 8 percent
    And the child's personal Ladi stage is "super-ladi"
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 8 percent

  Scenario: Participation inputs are not evaluated again by the interest policy
    Given the child's personal participation has already determined their Ladi stage
    When the child's personal savings interest rate is determined
    Then the interest policy does not evaluate streak, completion or average rating again

  Scenario: Unrelated configuration does not change a stage's interest rate [unrelatedSetting=family house level; value=4]
    Given the maximum savings interest rate is configured as 8 percent
    And the child's personal Ladi stage is "sun-ladi" at position 4
    And the family house level is configured as 4
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 5.33 percent

  Scenario: Unrelated configuration does not change a stage's interest rate [unrelatedSetting=five-star contribution bonus percent; value=25]
    Given the maximum savings interest rate is configured as 8 percent
    And the child's personal Ladi stage is "sun-ladi" at position 4
    And the five-star contribution bonus percent is configured as 25
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 5.33 percent

  Scenario: Unrelated configuration does not change a stage's interest rate [unrelatedSetting=completed family weekly streak; value=10]
    Given the maximum savings interest rate is configured as 8 percent
    And the child's personal Ladi stage is "sun-ladi" at position 4
    And the completed family weekly streak is configured as 10
    When the child's personal savings interest rate is determined
    Then the savings interest rate is 5.33 percent
