Feature: Contributions and rewards
  Scenario: Star ratings determine the reward bonus [base=10; stars=1; total=10; bonus=0]
    Given a core economy
    And a submitted child contribution worth 10 Ladirchen
    When the guardian administrator approves it with 1 stars
    Then the child wallet contains 10 Ladirchen
    And the reward breakdown is 10 base and 0 bonus

  Scenario: Star ratings determine the reward bonus [base=10; stars=4; total=10; bonus=0]
    Given a core economy
    And a submitted child contribution worth 10 Ladirchen
    When the guardian administrator approves it with 4 stars
    Then the child wallet contains 10 Ladirchen
    And the reward breakdown is 10 base and 0 bonus

  Scenario: Star ratings determine the reward bonus [base=10; stars=5; total=11; bonus=1]
    Given a core economy
    And a submitted child contribution worth 10 Ladirchen
    When the guardian administrator approves it with 5 stars
    Then the child wallet contains 11 Ladirchen
    And the reward breakdown is 10 base and 1 bonus

  Scenario: Star ratings determine the reward bonus [base=50; stars=5; total=53; bonus=3]
    Given a core economy
    And a submitted child contribution worth 50 Ladirchen
    When the guardian administrator approves it with 5 stars
    Then the child wallet contains 53 Ladirchen
    And the reward breakdown is 50 base and 3 bonus

  Scenario: Star ratings determine the reward bonus [base=0; stars=5; total=1; bonus=1]
    Given a core economy
    And a submitted child contribution worth 0 Ladirchen
    When the guardian administrator approves it with 5 stars
    Then the child wallet contains 1 Ladirchen
    And the reward breakdown is 0 base and 1 bonus

  Scenario: Approval cannot credit the same contribution twice
    Given a core economy
    And a submitted child contribution worth 10 Ladirchen
    When the guardian administrator approves it with 5 stars
    And the guardian administrator tries to approve it again
    Then the economy command fails with "InvalidState"
    And the child wallet contains 11 Ladirchen
    And there are 1 ledger entries
    And the rejected command left the economy unchanged

  Scenario: The configured five-star bonus is used for contribution approval
    Given a core economy
    And the five-star contribution bonus is configured as 25 percent
    And a submitted child contribution worth 10 Ladirchen
    When the guardian administrator approves it with 5 stars
    Then the child wallet contains 13 Ladirchen

  Scenario: A child can review a guardian contribution
    Given a core economy
    And a submitted guardian contribution worth 20 Ladirchen
    When the child approves the guardian contribution
    Then the guardian wallet contains 21 Ladirchen

  Scenario: A supporter cannot review child contributions
    Given a core economy
    And a submitted child contribution worth 10 Ladirchen
    When the supporter tries to approve the child contribution
    Then the economy command fails with "Forbidden"
    And the child wallet contains 0 Ladirchen
    And the rejected command left the economy unchanged

  Scenario: Claiming assigns a task without submitting it
    Given a core economy
    And an unassigned contribution
    When the child claims it
    Then the contribution status is "Available"

  Scenario: A returned contribution can be submitted again
    Given a core economy
    And a submitted child contribution worth 10 Ladirchen
    When the guardian administrator returns it
    And the child submits it again
    Then the contribution status is "Pending"

  Scenario: Only the assignee can submit an available contribution
    Given an available contribution assigned to the child
    When another family member attempts to submit it
    Then the contribution remains available

  Scenario: Assigning an available contribution removes previous partner invitations
    Given an available extra contribution has invited partners
    When a guardian administrator assigns the contribution to another member
    Then the contribution has no invited partners

  Scenario: Extra contributions preserve their configured energy
    Given a guardian administrator creates an extra contribution with 20 energy
    When the contribution is added
    Then its stored energy is 20

  Scenario: Children and guardians can work in the same contribution team [assigneeRole=child; partnerRole=guardian]
    Given an available extra contribution assigned to a child
    When an active guardian family member is invited as a partner
    Then the guardian family member belongs to the contribution team
    And the assignee cannot be invited as their own partner

  Scenario: Children and guardians can work in the same contribution team [assigneeRole=guardian; partnerRole=child]
    Given an available extra contribution assigned to a guardian
    When an active child family member is invited as a partner
    Then the child family member belongs to the contribution team
    And the assignee cannot be invited as their own partner

  Scenario: A family member who can manage content can create a contribution for a family member
    Given a family member has content-management permission
    And valid contribution data names an existing family member as assignee
    When the family member creates the contribution
    Then the contribution is available to its assignee

  Scenario: A guardian can create a contribution for themselves
    Given a guardian does not have content-management permission
    When the guardian creates a valid contribution assigned to themselves
    Then the contribution is available to that guardian

  Scenario: A family member cannot create an unauthorized contribution [assignee=another family member]
    Given a family member enters valid contribution data assigned to "another family member"
    And the family member cannot manage content
    When the family member attempts to create the contribution
    Then no contribution is created

  Scenario: A family member cannot create an unauthorized contribution [assignee=nobody]
    Given a family member enters valid contribution data assigned to "nobody"
    And the family member cannot manage content
    When the family member attempts to create the contribution
    Then no contribution is created

  Scenario: A contribution requires a non-blank title
    Given a family member is otherwise allowed to create a contribution
    When the contribution is submitted with a blank title
    Then no contribution is created

  Scenario: Only a family member who can manage content can assign an available contribution
    Given an available contribution
    When a family member without content-management permission attempts to assign it
    Then the contribution assignment remains unchanged

  Scenario: Invalid contribution assignments are rejected [state=pending]
    Given a family member who can manage content attempts to assign a contribution that is "pending"
    When the assignment is submitted
    Then the contribution assignment remains unchanged

  Scenario: Invalid contribution assignments are rejected [state=approved]
    Given a family member who can manage content attempts to assign a contribution that is "approved"
    When the assignment is submitted
    Then the contribution assignment remains unchanged

  Scenario: Invalid contribution assignments are rejected [state=assigned to a missing family member]
    Given a family member who can manage content attempts to assign a contribution that is "assigned to a missing family member"
    When the assignment is submitted
    Then the contribution assignment remains unchanged

  Scenario: Reassigning an available contribution replaces its assignee
    Given an available contribution is assigned to one family member
    And a different family member exists
    When a family member who can manage content assigns the contribution to the different family member
    Then the different family member is the contribution assignee
    And previous partner invitations are removed

  Scenario: Only a family member who can manage content can delete a contribution
    Given a contribution exists
    When a family member without content-management permission attempts to delete it
    Then the contribution remains unchanged

  Scenario: Deleting an approved contribution does not reclaim its credited reward
    Given an approved contribution has credited its assignee's wallet
    When a family member who can manage content deletes the contribution
    Then the contribution no longer exists
    And the previously credited wallet balance remains unchanged
