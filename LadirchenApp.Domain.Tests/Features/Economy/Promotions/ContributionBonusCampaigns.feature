Feature: Contribution bonus campaigns
  A family member who can manage content can create a time-limited bonus campaign for an existing contribution.
  A campaign multiplies the contribution reward and may add a non-negative teamwork bonus.
  A newly created campaign is active immediately and available to the family.
  Campaign availability is evaluated against its one-time deadline in the family timezone at approval time.

  Scenario: A bonus campaign expires permanently at its local deadline [time=2026-10-10T16:59:00; total=24]
    Given a core economy
    And a submitted child contribution worth 10 Ladirchen
    And an active double-reward campaign with 3 Ladirchen teamwork bonus until "2026-10-10T17:00:00"
    And the local time is "2026-10-10T16:59:00"
    When the guardian administrator approves the contribution with 5 stars
    Then the child wallet contains 24 Ladirchen

  Scenario: A bonus campaign expires permanently at its local deadline [time=2026-10-10T17:00:00; total=11]
    Given a core economy
    And a submitted child contribution worth 10 Ladirchen
    And an active double-reward campaign with 3 Ladirchen teamwork bonus until "2026-10-10T17:00:00"
    And the local time is "2026-10-10T17:00:00"
    When the guardian administrator approves the contribution with 5 stars
    Then the child wallet contains 11 Ladirchen

  Scenario: A bonus campaign expires permanently at its local deadline [time=2026-10-11T16:59:00; total=11]
    Given a core economy
    And a submitted child contribution worth 10 Ladirchen
    And an active double-reward campaign with 3 Ladirchen teamwork bonus until "2026-10-10T17:00:00"
    And the local time is "2026-10-11T16:59:00"
    When the guardian administrator approves the contribution with 5 stars
    Then the child wallet contains 11 Ladirchen

  Scenario: Campaign multiplier and teamwork bonus are applied before the five-star bonus [multiplier=2; teamworkBonus=3; promotedReward=23; ratingBonus=1; totalReward=24]
    Given a contribution has a base reward of 10 Ladirchen
    And an active campaign multiplies the reward by 2
    And the campaign adds 3 Ladirchen teamwork bonus
    And the five-star contribution bonus is configured as 5 percent
    When the contribution is approved with 5 stars before the deadline
    Then the promoted base reward is 23 Ladirchen
    And the five-star bonus is 1 Ladirchen
    And the total reward is 24 Ladirchen

  Scenario: Campaign multiplier and teamwork bonus are applied before the five-star bonus [multiplier=3; teamworkBonus=3; promotedReward=33; ratingBonus=2; totalReward=35]
    Given a contribution has a base reward of 10 Ladirchen
    And an active campaign multiplies the reward by 3
    And the campaign adds 3 Ladirchen teamwork bonus
    And the five-star contribution bonus is configured as 5 percent
    When the contribution is approved with 5 stars before the deadline
    Then the promoted base reward is 33 Ladirchen
    And the five-star bonus is 2 Ladirchen
    And the total reward is 35 Ladirchen

  Scenario: Only active campaigns before their deadline affect rewards [state=active; deadlineState=in the future; availability=applied]
    Given a bonus campaign is active and its deadline is in the future
    When its contribution is approved
    Then the bonus campaign is applied

  Scenario: Only active campaigns before their deadline affect rewards [state=inactive; deadlineState=in the future; availability=ignored]
    Given a bonus campaign is inactive and its deadline is in the future
    When its contribution is approved
    Then the bonus campaign is ignored

  Scenario: Only active campaigns before their deadline affect rewards [state=active; deadlineState=reached; availability=ignored]
    Given a bonus campaign is active and its deadline is reached
    When its contribution is approved
    Then the bonus campaign is ignored

  Scenario: A campaign can use a supported reward multiplier [multiplier=2]
    Given a family member who can manage content selects the 2-times reward multiplier
    When the family member who can manage content creates a valid bonus campaign
    Then the campaign is created with multiplier 2

  Scenario: A campaign can use a supported reward multiplier [multiplier=3]
    Given a family member who can manage content selects the 3-times reward multiplier
    When the family member who can manage content creates a valid bonus campaign
    Then the campaign is created with multiplier 3

  Scenario: A newly created campaign is active immediately
    Given an existing contribution
    And a family member who can manage content enters a supported multiplier, a deadline and a non-negative teamwork bonus
    When the family member who can manage content creates the bonus campaign
    Then the campaign is attached to the contribution
    And the campaign is active immediately
    And the campaign is available to the family before its deadline

  Scenario: Invalid campaign data does not create a campaign [invalidData=a contribution that does not exist]
    Given a family member who can manage content enters campaign data with "a contribution that does not exist"
    When the family member who can manage content attempts to create the bonus campaign
    Then no bonus campaign is created

  Scenario: Invalid campaign data does not create a campaign [invalidData=a missing deadline]
    Given a family member who can manage content enters campaign data with "a missing deadline"
    When the family member who can manage content attempts to create the bonus campaign
    Then no bonus campaign is created

  Scenario: Invalid campaign data does not create a campaign [invalidData=an unsupported multiplier]
    Given a family member who can manage content enters campaign data with "an unsupported multiplier"
    When the family member who can manage content attempts to create the bonus campaign
    Then no bonus campaign is created

  Scenario: Invalid campaign data does not create a campaign [invalidData=a negative teamwork bonus]
    Given a family member who can manage content enters campaign data with "a negative teamwork bonus"
    When the family member who can manage content attempts to create the bonus campaign
    Then no bonus campaign is created

  Scenario: Only family members who can manage content can add or delete bonus campaigns
    Given a family member cannot manage content
    When the member attempts to add or delete a bonus campaign
    Then the bonus campaigns remain unchanged

  Scenario: Deleting a contribution also deletes its bonus campaigns
    Given a contribution has an attached bonus campaign
    When a guardian administrator deletes the contribution
    Then the attached bonus campaign no longer exists
