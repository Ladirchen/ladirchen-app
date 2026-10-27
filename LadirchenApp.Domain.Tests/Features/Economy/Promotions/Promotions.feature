Feature: Contribution promotions
  Promotions temporarily modify the reward of one contribution.
  Availability is evaluated in the family timezone at approval time.

  Scenario: Promotions stop at the local deadline
    Given a core economy
    And a submitted child contribution worth 10 Ladirchen
    And a double reward promotion with 3 teamwork bonus until 17:00
    And the local time is "<time>"
    When the administrator approves it with 5 stars
    Then the child wallet contains <total> Ladirchen
    Examples:
      | time                 | total |
      | 2026-10-10T16:59:00 | 24    |
      | 2026-10-10T17:00:00 | 11    |
      | 2026-10-11T16:59:00 | 24    |

  Scenario: Promotion multiplier and teamwork bonus are applied before the five-star bonus
    Given a contribution has a base reward of 10 Ladirchen
    And an active promotion multiplies the reward by <multiplier>
    And the promotion adds <teamworkBonus> Ladirchen teamwork bonus
    And the five-star contribution bonus is configured as 5 percent
    When the contribution is approved with 5 stars before the deadline
    Then the promoted base reward is <promotedReward> Ladirchen
    And the five-star bonus is <ratingBonus> Ladirchen
    And the total reward is <totalReward> Ladirchen
    Examples:
      | multiplier | teamworkBonus | promotedReward | ratingBonus | totalReward |
      | 2          | 3             | 23             | 1           | 24          |
      | 3          | 3             | 33             | 2           | 35          |

  Scenario: Only active promotions before their deadline affect rewards
    Given a promotion is <state> and its deadline is <deadlineState>
    When its contribution is approved
    Then the promotion is <availability>
    Examples:
      | state    | deadlineState | availability |
      | active   | in the future | applied      |
      | inactive | in the future | ignored      |
      | active   | reached       | ignored      |

  Scenario: Only content managers can add or delete promotions
    Given a family member cannot manage content
    When the member attempts to add or delete a promotion
    Then the promotions remain unchanged

  Scenario: Deleting a contribution also deletes its promotions
    Given a contribution has an attached promotion
    When an administrator deletes the contribution
    Then the attached promotion no longer exists
