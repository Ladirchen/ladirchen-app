Feature: Participation interest
  Scenario Outline: Participation determines a capped rate
    Given participation of <days> streak days, <completion> percent complete and <stars> stars
    When the default interest rate is calculated
    Then the interest rate is <rate> percent
    Examples:
      | days | completion | stars | rate |
      | 0    | 0          | 0     | 1    |
      | 0    | 60         | 0     | 3.1  |
      | 1    | 50         | 5     | 5.75 |
      | 7    | 100        | 5     | 8    |
      | 0    | 33.33      | 2.5   | 3.42 |

  Scenario Outline: Credits preserve the minimum and target cap
    When interest on <saved> saved toward <target> at <rate> percent is calculated
    Then the interest credit is <credit> Ladirchen
    Examples:
      | saved | target | rate | credit |
      | 0     | 100    | 8    | 0      |
      | 100   | 100    | 8    | 0      |
      | 10    | 100    | 0    | 1      |
      | 30    | 100    | 5    | 2      |
      | 99.5  | 100    | 8    | 0.5    |

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

  Scenario: Extra ratings do not change savings interest
    Given a core economy
    And these approved contributions:
      | id    | kind  | stars | at                   |
      | basic | Basic | 5     | 2026-10-10T10:00:00Z |
      | extra | Extra | 1     | 2026-10-10T11:00:00Z |
    When participation for week 41 of 2026 is evaluated
    Then participation is 1 days, 100 percent and 5 stars

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
