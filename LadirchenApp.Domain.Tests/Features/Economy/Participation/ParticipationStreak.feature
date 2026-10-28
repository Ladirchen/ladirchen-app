Feature: Personal participation streak
  A participation streak belongs to one participating family member and is calculated independently from other family members.
  Children and participating guardians use the same personal streak rules.
  Approved contribution days are determined in the family timezone.
  Savings interest consumes the resulting streak-day value without recalculating the streak.

  Scenario: Only the family member's own contribution days extend their streak
    Given a participating <memberRole> has completed 2 weekly streaks
    And the current weekly target is 7 days
    And the family member has approved contributions on 3 distinct days of the current week
    And other family members have approved contributions on 2 additional days
    When the family member's participation streak is calculated
    Then 17 streak days are used for the calculation
    And contributions from other family members are ignored
    Examples:
      | memberRole |
      | child      |
      | guardian   |

  Scenario: Each family member's participation streak is calculated independently
    Given a child and a participating guardian have approved contributions on different days
    When their participation streaks are calculated
    Then the child's streak contains only the child's approved contribution days
    And the guardian's streak contains only the guardian's approved contribution days

  Scenario: ISO week dates use the family timezone
    Given a core economy
    And these approved contributions:
      | id     | kind  | stars | at                   |
      | first  | Basic | 5     | 2026-12-31T23:30:00Z |
      | second | Basic | 3     | 2027-01-01T10:00:00Z |
      | third  | Extra | 1     | 2027-01-03T23:30:00Z |
    When participation for week 53 of 2026 is evaluated
    Then participation is 1 days, 100 percent and 4 stars

  Scenario: Repeated daylight-saving times count as one day
    Given a core economy
    And these approved contributions:
      | id     | kind  | stars | at                   |
      | first  | Basic | 5     | 2026-10-25T00:30:00Z |
      | second | Basic | 5     | 2026-10-25T01:30:00Z |
    When participation for week 43 of 2026 is evaluated
    Then participation is 1 days, 100 percent and 5 stars
