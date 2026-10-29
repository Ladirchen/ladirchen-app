Feature: Shared family weekly streak
  The completed weekly streak belongs to the family and is separate from each child's personal participation streak.
  Successful weeks extend the continuous family streak across week boundaries.
  A family week from 30 to below 60 percent energy preserves the continuous family streak.
  A family week below 30 percent energy degrades the continuous family streak by one without resetting it.

  Scenario: A successful week extends the shared family streak
    Given the family energy is at least 60 percent
    And the completed family weekly streak is 3
    When the family week was successful closed
    Then the completed family weekly streak is 4
    And every family member participating in the weekly goal has a stored weekly streak of 4
    And a non-participating family member's stored weekly streak remains unchanged

  Scenario: The family streak continues at the maximum house level
    Given the stored house level is at its maximum
    And the completed family weekly streak is 7
    And the family energy is at least 60 percent
    When a family member who can manage the family closes the family week as successful
    Then the stored house level remains at its maximum
    And the completed family weekly streak is 8

  Scenario: Insufficient family energy does not extend the family streak
    Given the family energy is below 60 percent
    And the completed family weekly streak is 3
    When a family member who can manage the family attempts to close the family week as successful
    Then the week completion is rejected
    And the completed family weekly streak remains 3

  Scenario: A family week from 30 to below 60 percent energy preserves the shared family streak
    Given the completed family weekly streak is 4
    And the family energy is at least 30 but below 60 percent
    When a family member who can manage the family closes the family week as failed
    Then the completed family weekly streak remains 4
    And every family member participating in the weekly goal has a stored weekly streak of 4

  Scenario: A family week below 30 percent energy degrades the shared family streak
    Given the completed family weekly streak is 4
    And the family energy is below 30 percent
    When a family member who can manage the family closes the family week as failed
    Then the completed family weekly streak is 3
    And every family member participating in the weekly goal has a stored weekly streak of 3

  Scenario: A severely failed week cannot degrade the family streak below zero
    Given the completed family weekly streak is 0
    And the family energy is below 30 percent
    When a family member who can manage the family closes the family week as failed
    Then the completed family weekly streak remains 0
