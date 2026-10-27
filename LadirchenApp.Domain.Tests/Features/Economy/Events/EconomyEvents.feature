Feature: Economy domain events
  Successful economy commands publish business events after their state changes are accepted.
  Monetary changes also produce ledger records linked to the operation and initiator.
  Rejected commands leave state, ledger and domain events unchanged.

  Scenario: Contribution lifecycle changes publish contribution events
    Given a contribution is in the state required for "<action>"
    When an authorized member performs "<action>"
    Then the economy publishes "<event>"
    Examples:
      | action    | event                 |
      | create    | ContributionCreated   |
      | submit    | ContributionSubmitted |
      | return    | ContributionReturned  |
      | approve   | ContributionApproved  |
      | assign    | ContributionChanged   |
      | delete    | ContributionChanged   |

  Scenario: Saving-goal lifecycle changes publish goal events
    Given a saving goal is in the state required for "<action>"
    When an authorized member performs "<action>"
    Then the economy publishes "<event>"
    Examples:
      | action   | event         |
      | create   | GoalCreated   |
      | update   | GoalChanged   |
      | deposit  | GoalChanged   |
      | withdraw | GoalChanged   |
      | cancel   | GoalChanged   |
      | complete | GoalCompleted |

  Scenario: Shop lifecycle changes publish shop events
    Given a shop reward is in the state required for "<action>"
    When an authorized member performs "<action>"
    Then the economy publishes "<event>"
    Examples:
      | action  | event               |
      | add     | ShopCatalogChanged  |
      | update  | ShopCatalogChanged  |
      | hide    | ShopCatalogChanged  |
      | request | ShopRewardRequested |
      | approve | ShopRewardDecided   |
      | reject  | ShopRewardDecided   |
      | cancel  | ShopRewardDecided   |

  Scenario: A monetary operation publishes one ledger event per ledger entry
    Given an accepted operation creates monetary ledger entries
    When the operation is recorded
    Then each ledger entry publishes a "LedgerRecorded" event
    And every entry contains the operation, member, source, amount, reason and UTC occurrence time

  Scenario: Weekly interest publishes goal and settlement events
    Given an eligible personal saving goal receives weekly interest
    When the closed week is settled
    Then the economy publishes a "GoalInterestCredited" event for the goal
    And the economy publishes one "WeeklyInterestCredited" event for the settlement

  Scenario: Rejected commands publish no business events
    Given an economy command violates a domain rule
    When the command is rejected
    Then no ledger entries are recorded
    And no domain events are published
