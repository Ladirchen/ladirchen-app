Feature: Saving goals and gifts
  Scenario: Personal goals have a bounded starter bonus [target=100; saved=5; withdrawable=0]
    Given a core economy
    When the child creates a "Private" goal targeting 100 Ladirchen
    Then the goal contains 5 Ladirchen
    And the goal allows withdrawing 0 Ladirchen

  Scenario: Personal goals have a bounded starter bonus [target=3; saved=3; withdrawable=3]
    Given a core economy
    When the child creates a "Private" goal targeting 3 Ladirchen
    Then the goal contains 3 Ladirchen
    And the goal allows withdrawing 3 Ladirchen

  Scenario: Deposits and withdrawals conserve earned funds
    Given a core economy
    And the child has 100 Ladirchen
    And the child creates a "Private" goal targeting 200 Ladirchen
    When the child deposits 30 Ladirchen
    And the child withdraws 50 Ladirchen
    Then the goal contains 5 Ladirchen
    And the child wallet contains 100 Ladirchen

  Scenario: Cancelling an incomplete personal goal refunds only its wallet-funded deposits
    Given an incomplete "Personal" savings goal contains 5 starter-bonus Ladirchen
    And the goal contains 30 contributed Ladirchen in addition to its starter bonus
    When the owner cancels the personal goal before its target is reached
    Then the cancelled goal no longer exists
    And 5 starter-bonus Ladirchen are forfeited
    And only the 30 contributed Ladirchen are eligible to be returned

  Scenario: Unanimously cancelling an incomplete family goal refunds only wallet-funded contributions
    Given an incomplete "Family" savings goal contains 15 starter-bonus Ladirchen
    And the goal contains 30 contributed Ladirchen in addition to its starter bonus
    When the child requests cancellation of the family goal
    And all other selected participants approve the family-goal cancellation
    Then the cancelled goal no longer exists
    And 15 starter-bonus Ladirchen are forfeited
    And only the 30 contributed Ladirchen are eligible to be returned

  Scenario: Cancelling a family goal refunds every wallet-funded contributor separately
    Given a family savings goal with the child, sibling and guardian as participants
    And the child and sibling each have 50 Ladirchen
    And the child contributes 10 Ladirchen and the sibling contributes 20 Ladirchen from their wallets
    And the guardian adds 7 non-refundable support Ladirchen
    When the child requests cancellation of the family goal
    And the sibling approves the family-goal cancellation
    Then the family goal remains unchanged while one participant is still pending
    When the guardian approves the family-goal cancellation
    Then the child wallet contains 50 Ladirchen
    And the sibling wallet contains 50 Ladirchen
    And the cancellation ledger refunds 10 Ladirchen to the child and 20 Ladirchen to the sibling
    And 22 non-refundable Ladirchen are forfeited

  Scenario: Rejecting a family-goal cancellation clears the request
    Given a family savings goal with the child, sibling and guardian as participants
    When the child requests cancellation of the family goal
    Then the cancellation request records the child and awaits the sibling and guardian
    When the sibling rejects the family-goal cancellation
    Then the family goal still exists without a cancellation request
    When the child requests cancellation of the family goal again
    Then the cancellation request records the child and awaits the sibling and guardian

  Scenario: A direct family-goal cancellation cannot bypass participant approval
    Given a family savings goal with the child, sibling and guardian as participants
    When the child attempts to cancel the family goal directly
    Then the economy command fails with "InvalidState"
    And the rejected command left the economy unchanged

  Scenario: Only selected participants decide a family-goal cancellation
    Given a family savings goal with the child and sibling as participants
    When the child requests cancellation of the family goal
    And a non-participating guardian attempts to approve the family-goal cancellation
    Then the economy command fails with "Forbidden"
    And the cancellation request still awaits the sibling

  Scenario: A one-participant family goal is cancelled by its requester's consent
    Given a family savings goal with only the child as participant
    And the child has 20 Ladirchen
    And the child contributes 10 Ladirchen to the family goal from their wallet
    When the child requests cancellation of the family goal
    Then the cancelled goal no longer exists
    And the child wallet contains 20 Ladirchen
    And the ledger records 5 forfeited Ladirchen

  Scenario: Savings interest is not refunded when an incomplete personal goal is cancelled
    Given a core economy
    And the local time is "2026-10-01T10:00:00"
    And the child has 100 Ladirchen
    And the child creates a "Private" goal targeting 100 Ladirchen
    And the child deposits 10 Ladirchen
    And the local time is "2026-10-12T10:00:00"
    When interest for week 41 of 2026 is settled
    And the child cancels the goal
    Then the child wallet contains 100 Ladirchen
    And the ledger records 6 forfeited Ladirchen

  Scenario: Savings interest cannot be withdrawn before the personal goal reaches its target
    Given a core economy
    And the local time is "2026-10-01T10:00:00"
    And the child has 100 Ladirchen
    And the child creates a "Private" goal targeting 100 Ladirchen
    And the child deposits 10 Ladirchen
    And the local time is "2026-10-12T10:00:00"
    When interest for week 41 of 2026 is settled
    And the child withdraws 11 Ladirchen
    Then the goal contains 6 Ladirchen
    And the child wallet contains 100 Ladirchen
    And the goal allows withdrawing 0 Ladirchen

  Scenario: Guardian support cannot be withdrawn before the personal goal reaches its target
    Given a core economy
    And the child has 100 Ladirchen
    And the child creates a "Family" goal targeting 100 Ladirchen
    And the child deposits 10 Ladirchen
    When the supporter tries to support it
    And the child withdraws 20 Ladirchen
    Then the goal contains 15 Ladirchen
    And the child wallet contains 100 Ladirchen
    And the goal allows withdrawing 0 Ladirchen

  Scenario: Reaching the savings target unlocks the starter bonus [goalType=Personal; starterBonus=5]
    Given a "Personal" savings goal contains 5 starter-bonus Ladirchen
    And contributed savings raise the goal balance to its target
    When the completed savings goal is closed
    Then its full balance including 5 starter-bonus Ladirchen is available
    And no starter bonus is forfeited

  Scenario: Reaching the savings target unlocks the starter bonus [goalType=Family; starterBonus=15]
    Given a "Family" savings goal contains 15 starter-bonus Ladirchen
    And contributed savings raise the goal balance to its target
    When the completed savings goal is closed
    Then its full balance including 15 starter-bonus Ladirchen is available
    And no starter bonus is forfeited

  Scenario: Private goals are private even from guardian administrators
    Given a core economy
    And the child creates a "Private" goal targeting 100 Ladirchen
    When the guardian administrator tries to support it
    Then the economy command fails with "Forbidden"
    And the goal contains 5 Ladirchen
    And the rejected command left the economy unchanged

  Scenario: Supporters only support family-visible goals [visibility=Family; saved=15]
    Given a core economy
    And the child creates a "Family" goal targeting 100 Ladirchen
    When the supporter tries to support it
    Then the goal contains 15 Ladirchen

  Scenario: Supporters only support family-visible goals [visibility=Guardians; saved=5]
    Given a core economy
    And the child creates a "Guardians" goal targeting 100 Ladirchen
    When the supporter tries to support it
    Then the goal contains 5 Ladirchen

  Scenario: Supporters only support family-visible goals [visibility=Private; saved=5]
    Given a core economy
    And the child creates a "Private" goal targeting 100 Ladirchen
    When the supporter tries to support it
    Then the goal contains 5 Ladirchen

  Scenario: Targets cannot fall below savings
    Given a core economy
    And the child creates a "Family" goal targeting 100 Ladirchen
    When the child tries to reduce the goal target to 4 Ladirchen
    Then the economy command fails with "InvalidValue"
    And the rejected command left the economy unchanged

  Scenario: Family goals receive a starter bonus for every family member [memberCount=1; starterBonus=5]
    Given the family has 1 children and guardians
    And the family-goal starter bonus is configured as 5 Ladirchen per family member
    When a family member creates a family savings goal targeting 100 Ladirchen
    Then the goal contains 5 starter-bonus Ladirchen
    And the starter bonus cannot be withdrawn before the goal is completed
    And the goal visibility is "Family"

  Scenario: Family goals receive a starter bonus for every family member [memberCount=3; starterBonus=15]
    Given the family has 3 children and guardians
    And the family-goal starter bonus is configured as 5 Ladirchen per family member
    When a family member creates a family savings goal targeting 100 Ladirchen
    Then the goal contains 15 starter-bonus Ladirchen
    And the starter bonus cannot be withdrawn before the goal is completed
    And the goal visibility is "Family"

  Scenario: Family goals receive a starter bonus for every family member [memberCount=5; starterBonus=25]
    Given the family has 5 children and guardians
    And the family-goal starter bonus is configured as 5 Ladirchen per family member
    When a family member creates a family savings goal targeting 100 Ladirchen
    Then the goal contains 25 starter-bonus Ladirchen
    And the starter bonus cannot be withdrawn before the goal is completed
    And the goal visibility is "Family"

  Scenario: The family-goal starter bonus cannot exceed the target
    Given the family has 4 children and guardians
    And the family-goal starter bonus is configured as 5 Ladirchen per family member
    When a family member creates a family savings goal targeting 12 Ladirchen
    Then the goal contains 12 starter-bonus Ladirchen

  Scenario: The family-member count is captured when the family goal is created
    Given the family has 3 children and guardians
    And the family-goal starter bonus is configured as 5 Ladirchen per family member
    When a family member creates a family savings goal
    And another family member is added later
    Then the existing goal contains 15 starter-bonus Ladirchen

  Scenario: A deposit is limited by available funds and the remaining target
    Given the child has a personal savings goal
    When the child deposits more than the available wallet balance or remaining target
    Then only the lower available amount is deposited

  Scenario: A child can gift only to another child's family-visible goal or a family goal
    Given the child views savings goals owned by other family members
    When the child attempts to gift Ladirchen to a non-family-visible goal
    Then the gift is rejected

  Scenario: Goal cheering respects guardian visibility permissions
    Given a guardian views a child's savings goal
    When the guardian toggles the cheer
    Then the cheer changes only if the guardian may view and support that goal
