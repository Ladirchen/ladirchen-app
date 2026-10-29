Feature: Savings-goal ownership, editing and visibility
  Every personal savings goal has one immutable owner.
  Family goals belong to the family rather than to an individual member.
  Visibility controls who may see and support a personal goal.

  Scenario: A savings goal receives its owner when it is created [creator=child; goalType=Personal; requestedOwner=self; storedOwner=child]
    Given "child" creates a valid "Personal" savings goal for "self"
    When the savings goal is created
    Then its owner is "child"

  Scenario: A savings goal receives its owner when it is created [creator=guardian; goalType=Personal; requestedOwner=self; storedOwner=guardian]
    Given "guardian" creates a valid "Personal" savings goal for "self"
    When the savings goal is created
    Then its owner is "guardian"

  Scenario: A savings goal receives its owner when it is created [creator=guardian administrator; goalType=Personal; requestedOwner=a child; storedOwner=child]
    Given "guardian administrator" creates a valid "Personal" savings goal for "a child"
    When the savings goal is created
    Then its owner is "child"

  Scenario: A savings goal receives its owner when it is created [creator=family member; goalType=Family; requestedOwner=family; storedOwner=family]
    Given "family member" creates a valid "Family" savings goal for "family"
    When the savings goal is created
    Then its owner is "family"

  Scenario: Editing a savings goal does not transfer ownership
    Given a personal savings goal belongs to a child
    When a family member who can manage savings goals changes its title, icon, target and visibility
    Then the changed goal still belongs to that child

  Scenario: Unauthorized goal editing is rejected [actor=another child]
    Given a personal savings goal belongs to a child
    When "another child" attempts to edit it
    Then the goal remains unchanged

  Scenario: Unauthorized goal editing is rejected [actor=a guardian without goal-management permission]
    Given a personal savings goal belongs to a child
    When "a guardian without goal-management permission" attempts to edit it
    Then the goal remains unchanged

  Scenario: A child can edit their own personal savings goal
    Given a personal savings goal belongs to the signed-in child
    When the child changes its valid title, icon, target or visibility
    Then the changes are stored without changing the owner

  Scenario: A goal manager can edit a managed child's personal goal
    Given a personal savings goal belongs to the active child
    And the signed-in guardian can manage goals
    When the guardian changes valid goal data
    Then the changes are stored without changing the owner

  Scenario: Goal visibility determines who can see a personal goal [visibility=Private; viewer=owner; result=visible]
    Given a personal goal has "Private" visibility
    When "owner" views the family's savings goals
    Then the personal goal is "visible"

  Scenario: Goal visibility determines who can see a personal goal [visibility=Private; viewer=guardian administrator; result=hidden]
    Given a personal goal has "Private" visibility
    When "guardian administrator" views the family's savings goals
    Then the personal goal is "hidden"

  Scenario: Goal visibility determines who can see a personal goal [visibility=Guardians; viewer=guardian administrator; result=visible]
    Given a personal goal has "Guardians" visibility
    When "guardian administrator" views the family's savings goals
    Then the personal goal is "visible"

  Scenario: Goal visibility determines who can see a personal goal [visibility=Guardians; viewer=guardian supporter; result=hidden]
    Given a personal goal has "Guardians" visibility
    When "guardian supporter" views the family's savings goals
    Then the personal goal is "hidden"

  Scenario: Goal visibility determines who can see a personal goal [visibility=Guardians; viewer=another child; result=hidden]
    Given a personal goal has "Guardians" visibility
    When "another child" views the family's savings goals
    Then the personal goal is "hidden"

  Scenario: Goal visibility determines who can see a personal goal [visibility=Family; viewer=family member; result=visible]
    Given a personal goal has "Family" visibility
    When "family member" views the family's savings goals
    Then the personal goal is "visible"

  Scenario: Guardian support does not debit a personal wallet
    Given a guardian with goal-support permission supports a visible savings goal
    And the goal has enough remaining target amount
    When the guardian contributes 25 Ladirchen
    Then the goal balance increases by 25 Ladirchen
    And no family-member wallet is debited

  Scenario: Guardian support is rounded and capped by the remaining target [remaining=100; entered=10.4; credited=10]
    Given a visible savings goal needs 100 Ladirchen to reach its target
    When a guardian with goal-support permission contributes 10.4 Ladirchen
    Then the goal balance increases by 10 Ladirchen

  Scenario: Guardian support is rounded and capped by the remaining target [remaining=100; entered=10.5; credited=11]
    Given a visible savings goal needs 100 Ladirchen to reach its target
    When a guardian with goal-support permission contributes 10.5 Ladirchen
    Then the goal balance increases by 11 Ladirchen

  Scenario: Guardian support is rounded and capped by the remaining target [remaining=5; entered=20; credited=5]
    Given a visible savings goal needs 5 Ladirchen to reach its target
    When a guardian with goal-support permission contributes 20 Ladirchen
    Then the goal balance increases by 5 Ladirchen

  Scenario: Guardian support is rounded and capped by the remaining target [remaining=5; entered=-2; credited=0]
    Given a visible savings goal needs 5 Ladirchen to reach its target
    When a guardian with goal-support permission contributes -2 Ladirchen
    Then the goal balance increases by 0 Ladirchen

  Scenario: A guardian gift to a personal goal is recorded for its owner
    Given a guardian with goal-support permission supports a visible personal goal owned by a child
    When the support is accepted
    Then the credited amount and guardian name are recorded for that child

  Scenario: Supporting a family goal does not create a personal gift record
    Given a guardian with goal-support permission supports a family savings goal
    When the support is accepted
    Then the family goal balance increases
    And no personal guardian-gift record is created
