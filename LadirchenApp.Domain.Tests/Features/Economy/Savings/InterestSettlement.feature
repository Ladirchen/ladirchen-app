Feature: Weekly savings interest settlement
  Interest is settled for completed ISO weeks using the family timezone.
  Each eligible goal uses its owner's immutable Ladi evaluation for the settled week.
  Settlement is atomic and the same week cannot be credited twice.

  Scenario: Settlement uses the Ladi stage captured for the closed week
    Given the child had the personal Ladi stage "garden-ladi" in a closed ISO week
    And the child currently has the personal Ladi stage "super-ladi"
    And the child has an eligible personal savings goal
    When interest for the closed ISO week is settled
    Then the interest rate is determined from "garden-ladi"
    And the current "super-ladi" stage does not change that settlement

  Scenario: All personal goals of one owner use the same weekly Ladi evaluation
    Given one child has multiple eligible personal savings goals
    And the child has one personal Ladi evaluation for the settled ISO week
    When interest for that week is settled
    Then every eligible goal of the child uses the same evaluated Ladi stage

  Scenario: Settlement does not recalculate personal participation
    Given every eligible goal owner has a personal Ladi evaluation for the settled ISO week
    When interest for that week is settled
    Then settlement uses the evaluated Ladi stages
    And it does not recalculate streak, completion or average rating from contributions

  Scenario: Closed weeks can only be credited once
    Given a core economy
    And the local time is "2026-10-01T10:00:00"
    And the child creates a "Private" goal targeting 100 Ladirchen
    And the local time is "2026-10-12T10:00:00"
    When interest for week 41 of 2026 is settled
    Then the goal contains 6 Ladirchen
    When interest for week 41 of 2026 is settled again
    Then the goal contains 6 Ladirchen
    And the repeated settlement left the economy unchanged

  Scenario: The current week cannot be credited early
    Given a core economy
    And the child creates a "Private" goal targeting 100 Ladirchen
    When interest for week 41 of 2026 is attempted
    Then the economy command fails with "InvalidState"
    And the rejected command left the economy unchanged

  Scenario: New goals do not receive retroactive interest
    Given a core economy
    And the local time is "2026-10-12T10:00:00"
    And the child creates a "Private" goal targeting 100 Ladirchen
    When interest for week 41 of 2026 is settled
    Then the goal contains 5 Ladirchen

  Scenario: Missing owner evaluation cannot partially credit goals
    Given a core economy
    And the local time is "2026-10-01T10:00:00"
    And the child creates a "Private" goal targeting 100 Ladirchen
    And the sibling creates a private goal
    And the local time is "2026-10-12T10:00:00"
    When weekly settlement is attempted without owner evaluations
    Then the economy command fails with "InvalidValue"
    And the rejected command left the economy unchanged
