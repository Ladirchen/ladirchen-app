Feature: Saving goals and gifts
  Scenario: Personal goals have a bounded starter bonus
    Given a core economy
    When the child creates a "Private" goal targeting <target> Ladirchen
    Then the goal contains <saved> Ladirchen
    And the goal allows withdrawing <withdrawable> Ladirchen
    Examples:
      | target | saved | withdrawable |
      | 100    | 5     | 0            |
      | 3      | 3     | 3            |

  Scenario: Deposits and withdrawals conserve earned funds
    Given a core economy
    And the child has 100 Ladirchen
    And the child creates a "Private" goal targeting 200 Ladirchen
    When the child deposits 30 Ladirchen
    And the child withdraws 50 Ladirchen
    Then the goal contains 5 Ladirchen
    And the child wallet contains 100 Ladirchen

  Scenario: Cancelling an incomplete personal or family goal forfeits its starter bonus
    Given an incomplete "<goalType>" savings goal contains <starterBonus> starter-bonus Ladirchen
    And the goal contains 30 contributed Ladirchen in addition to its starter bonus
    When an authorized family member cancels the goal before its target is reached
    Then the cancelled goal no longer exists
    And <starterBonus> starter-bonus Ladirchen are forfeited
    And only the 30 contributed Ladirchen are eligible to be returned
    Examples:
      | goalType | starterBonus |
      | Personal | 5            |
      | Family   | 15           |

  Scenario: Reaching the savings target unlocks the starter bonus
    Given a "<goalType>" savings goal contains <starterBonus> starter-bonus Ladirchen
    And contributed savings raise the goal balance to its target
    When the completed savings goal is closed
    Then its full balance including <starterBonus> starter-bonus Ladirchen is available
    And no starter bonus is forfeited
    Examples:
      | goalType | starterBonus |
      | Personal | 5            |
      | Family   | 15           |

  Scenario: Private goals are private even from administrators
    Given a core economy
    And the child creates a "Private" goal targeting 100 Ladirchen
    When the administrator tries to support it
    Then the economy command fails with "Forbidden"
    And the goal contains 5 Ladirchen
    And the rejected command left the economy unchanged

  Scenario: Supporters only support family-visible goals
    Given a core economy
    And the child creates a "<visibility>" goal targeting 100 Ladirchen
    When the supporter tries to support it
    Then the goal contains <saved> Ladirchen
    Examples:
      | visibility | saved |
      | Family     | 15    |
      | Guardians  | 5     |
      | Private    | 5     |

  Scenario: Targets cannot fall below savings
    Given a core economy
    And the child creates a "Family" goal targeting 100 Ladirchen
    When the child tries to reduce the goal target to 4 Ladirchen
    Then the economy command fails with "InvalidValue"
    And the rejected command left the economy unchanged

  Scenario: Family goals receive a starter bonus for every family member
    Given the family has <memberCount> children and guardians
    And the family-goal starter bonus is configured as 5 Ladirchen per family member
    When a family member creates a family savings goal targeting 100 Ladirchen
    Then the goal contains <starterBonus> starter-bonus Ladirchen
    And the starter bonus cannot be withdrawn before the goal is completed
    And the goal visibility is "Family"
    Examples:
      | memberCount | starterBonus |
      | 1           | 5            |
      | 3           | 15           |
      | 5           | 25           |

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
