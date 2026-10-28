Feature: Savings-goal ownership, editing and visibility
  Every personal savings goal has one immutable owner.
  Family goals belong to the family rather than to an individual member.
  Visibility controls who may see and support a personal goal.

  Scenario: A savings goal receives its owner when it is created
    Given "<creator>" creates a valid "<goalType>" savings goal for "<requestedOwner>"
    When the savings goal is created
    Then its owner is "<storedOwner>"
    Examples:
      | creator              | goalType | requestedOwner | storedOwner |
      | child                | Personal | self           | child       |
      | guardian             | Personal | self           | guardian    |
      | family administrator | Personal | a child        | child       |
      | family member        | Family   | family         | family      |

  Scenario: Editing a savings goal does not transfer ownership
    Given a personal savings goal belongs to a child
    When an authorized family member changes its title, icon, target and visibility
    Then the changed goal still belongs to that child

  Scenario: Unauthorized goal editing is rejected
    Given a personal savings goal belongs to a child
    When "<actor>" attempts to edit it
    Then the goal remains unchanged
    Examples:
      | actor                              |
      | another child                      |
      | a guardian without goal-management permission |

  Scenario: A child can edit their own personal savings goal
    Given a personal savings goal belongs to the signed-in child
    When the child changes its valid title, icon, target or visibility
    Then the changes are stored without changing the owner

  Scenario: A goal manager can edit a managed child's personal goal
    Given a personal savings goal belongs to the active child
    And the signed-in guardian can manage goals
    When the guardian changes valid goal data
    Then the changes are stored without changing the owner

  Scenario: Goal visibility determines who can see a personal goal
    Given a personal goal has "<visibility>" visibility
    When "<viewer>" views the family's savings goals
    Then the personal goal is "<result>"
    Examples:
      | visibility | viewer               | result  |
      | Private    | owner                | visible |
      | Private    | family administrator | hidden  |
      | Guardians  | guardian admin       | visible |
      | Guardians  | guardian supporter   | hidden  |
      | Guardians  | another child        | hidden  |
      | Family     | family member        | visible |

  Scenario: Guardian support does not debit a personal wallet
    Given an authorized guardian supports a visible savings goal
    And the goal has enough remaining target amount
    When the guardian contributes 25 Ladirchen
    Then the goal balance increases by 25 Ladirchen
    And no family-member wallet is debited

  Scenario: Guardian support is rounded and capped by the remaining target
    Given a visible savings goal needs <remaining> Ladirchen to reach its target
    When an authorized guardian contributes <entered> Ladirchen
    Then the goal balance increases by <credited> Ladirchen
    Examples:
      | remaining | entered | credited |
      | 100       | 10.4    | 10       |
      | 100       | 10.5    | 11       |
      | 5         | 20      | 5        |
      | 5         | -2      | 0        |

  Scenario: A guardian gift to a personal goal is recorded for its owner
    Given an authorized guardian supports a visible personal goal owned by a child
    When the support is accepted
    Then the credited amount and guardian name are recorded for that child

  Scenario: Supporting a family goal does not create a personal gift record
    Given an authorized guardian supports a family savings goal
    When the support is accepted
    Then the family goal balance increases
    And no personal guardian-gift record is created
