Feature: Economy domain events
  Successful economy commands publish business events after their state changes are accepted.
  Monetary changes also produce ledger records linked to the operation and initiator.
  Rejected commands leave state, ledger and domain events unchanged.

  Scenario: Contribution lifecycle changes publish contribution events [action=create; event=ContributionCreated]
    Given a contribution is in the state required for "create"
    When a family member permitted to perform "create"
    Then the economy publishes "ContributionCreated"

  Scenario: Contribution lifecycle changes publish contribution events [action=submit; event=ContributionSubmitted]
    Given a contribution is in the state required for "submit"
    When a family member permitted to perform "submit"
    Then the economy publishes "ContributionSubmitted"

  Scenario: Contribution lifecycle changes publish contribution events [action=return; event=ContributionReturned]
    Given a contribution is in the state required for "return"
    When a family member permitted to perform "return"
    Then the economy publishes "ContributionReturned"

  Scenario: Contribution lifecycle changes publish contribution events [action=approve; event=ContributionApproved]
    Given a contribution is in the state required for "approve"
    When a family member permitted to perform "approve"
    Then the economy publishes "ContributionApproved"

  Scenario: Contribution lifecycle changes publish contribution events [action=assign; event=ContributionChanged]
    Given a contribution is in the state required for "assign"
    When a family member permitted to perform "assign"
    Then the economy publishes "ContributionChanged"

  Scenario: Contribution lifecycle changes publish contribution events [action=delete; event=ContributionChanged]
    Given a contribution is in the state required for "delete"
    When a family member permitted to perform "delete"
    Then the economy publishes "ContributionChanged"

  Scenario: Saving-goal lifecycle changes publish goal events [action=create; event=GoalCreated]
    Given a saving goal is in the state required for "create"
    When a family member permitted to perform "create"
    Then the economy publishes "GoalCreated"

  Scenario: Saving-goal lifecycle changes publish goal events [action=update; event=GoalChanged]
    Given a saving goal is in the state required for "update"
    When a family member permitted to perform "update"
    Then the economy publishes "GoalChanged"

  Scenario: Saving-goal lifecycle changes publish goal events [action=deposit; event=GoalChanged]
    Given a saving goal is in the state required for "deposit"
    When a family member permitted to perform "deposit"
    Then the economy publishes "GoalChanged"

  Scenario: Saving-goal lifecycle changes publish goal events [action=withdraw; event=GoalChanged]
    Given a saving goal is in the state required for "withdraw"
    When a family member permitted to perform "withdraw"
    Then the economy publishes "GoalChanged"

  Scenario: Saving-goal lifecycle changes publish goal events [action=cancel; event=GoalChanged]
    Given a saving goal is in the state required for "cancel"
    When a family member permitted to perform "cancel"
    Then the economy publishes "GoalChanged"

  Scenario: Saving-goal lifecycle changes publish goal events [action=complete; event=GoalCompleted]
    Given a saving goal is in the state required for "complete"
    When a family member permitted to perform "complete"
    Then the economy publishes "GoalCompleted"

  Scenario: Shop lifecycle changes publish shop events [action=add; event=ShopCatalogChanged]
    Given a shop reward is in the state required for "add"
    When a family member permitted to perform "add"
    Then the economy publishes "ShopCatalogChanged"

  Scenario: Shop lifecycle changes publish shop events [action=update; event=ShopCatalogChanged]
    Given a shop reward is in the state required for "update"
    When a family member permitted to perform "update"
    Then the economy publishes "ShopCatalogChanged"

  Scenario: Shop lifecycle changes publish shop events [action=hide; event=ShopCatalogChanged]
    Given a shop reward is in the state required for "hide"
    When a family member permitted to perform "hide"
    Then the economy publishes "ShopCatalogChanged"

  Scenario: Shop lifecycle changes publish shop events [action=request; event=ShopRewardRequested]
    Given a shop reward is in the state required for "request"
    When a family member permitted to perform "request"
    Then the economy publishes "ShopRewardRequested"

  Scenario: Shop lifecycle changes publish shop events [action=approve; event=ShopRewardDecided]
    Given a shop reward is in the state required for "approve"
    When a family member permitted to perform "approve"
    Then the economy publishes "ShopRewardDecided"

  Scenario: Shop lifecycle changes publish shop events [action=reject; event=ShopRewardDecided]
    Given a shop reward is in the state required for "reject"
    When a family member permitted to perform "reject"
    Then the economy publishes "ShopRewardDecided"

  Scenario: Shop lifecycle changes publish shop events [action=cancel; event=ShopRewardDecided]
    Given a shop reward is in the state required for "cancel"
    When a family member permitted to perform "cancel"
    Then the economy publishes "ShopRewardDecided"

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
