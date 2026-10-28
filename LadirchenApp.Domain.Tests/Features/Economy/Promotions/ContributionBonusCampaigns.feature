Feature: Contribution bonus campaigns
  A content manager can create a time-limited bonus campaign for an existing contribution.
  A campaign multiplies the contribution reward and may add a non-negative teamwork bonus.
  A newly created campaign is active immediately and available to the family.
  Campaign availability is evaluated against its one-time deadline in the family timezone at approval time.

  Scenario: A bonus campaign expires permanently at its local deadline
    Given a core economy
    And a submitted child contribution worth 10 Ladirchen
    And an active double-reward campaign with 3 Ladirchen teamwork bonus until "2026-10-10T17:00:00"
    And the local time is "<time>"
    When the administrator approves the contribution with 5 stars
    Then the child wallet contains <total> Ladirchen
    Examples:
      | time                | total |
      | 2026-10-10T16:59:00 | 24    |
      | 2026-10-10T17:00:00 | 11    |
      | 2026-10-11T16:59:00 | 11    |

  Scenario: Campaign multiplier and teamwork bonus are applied before the five-star bonus
    Given a contribution has a base reward of 10 Ladirchen
    And an active campaign multiplies the reward by <multiplier>
    And the campaign adds <teamworkBonus> Ladirchen teamwork bonus
    And the five-star contribution bonus is configured as 5 percent
    When the contribution is approved with 5 stars before the deadline
    Then the promoted base reward is <promotedReward> Ladirchen
    And the five-star bonus is <ratingBonus> Ladirchen
    And the total reward is <totalReward> Ladirchen
    Examples:
      | multiplier | teamworkBonus | promotedReward | ratingBonus | totalReward |
      | 2          | 3             | 23             | 1           | 24          |
      | 3          | 3             | 33             | 2           | 35          |

  Scenario: Only active campaigns before their deadline affect rewards
    Given a bonus campaign is <state> and its deadline is <deadlineState>
    When its contribution is approved
    Then the bonus campaign is <availability>
    Examples:
      | state    | deadlineState | availability |
      | active   | in the future | applied      |
      | inactive | in the future | ignored      |
      | active   | reached       | ignored      |

  Scenario: A campaign can use a supported reward multiplier
    Given a content manager selects the <multiplier>-times reward multiplier
    When the content manager creates a valid bonus campaign
    Then the campaign is created with multiplier <multiplier>
    Examples:
      | multiplier |
      | 2          |
      | 3          |

  Scenario: A newly created campaign is active immediately
    Given an existing contribution
    And a content manager enters a supported multiplier, a deadline and a non-negative teamwork bonus
    When the content manager creates the bonus campaign
    Then the campaign is attached to the contribution
    And the campaign is active immediately
    And the campaign is available to the family before its deadline

  Scenario: Invalid campaign data does not create a campaign
    Given a content manager enters campaign data with "<invalidData>"
    When the content manager attempts to create the bonus campaign
    Then no bonus campaign is created
    Examples:
      | invalidData                 |
      | a contribution that does not exist |
      | a missing deadline          |
      | an unsupported multiplier   |
      | a negative teamwork bonus   |

  Scenario: Only content managers can add or delete bonus campaigns
    Given a family member cannot manage content
    When the member attempts to add or delete a bonus campaign
    Then the bonus campaigns remain unchanged

  Scenario: Deleting a contribution also deletes its bonus campaigns
    Given a contribution has an attached bonus campaign
    When an administrator deletes the contribution
    Then the attached bonus campaign no longer exists
