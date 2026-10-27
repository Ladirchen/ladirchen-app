Feature: Contributions and rewards
  Scenario Outline: Star ratings determine the reward bonus
    Given a core economy
    And a submitted child contribution worth <base> Ladirchen
    When the administrator approves it with <stars> stars
    Then the child wallet contains <total> Ladirchen
    And the reward breakdown is <base> base and <bonus> bonus
    Examples:
      | base | stars | total | bonus |
      | 10   | 1     | 10    | 0     |
      | 10   | 4     | 10    | 0     |
      | 10   | 5     | 11    | 1     |
      | 50   | 5     | 53    | 3     |
      | 0    | 5     | 1     | 1     |

  Scenario: Approval cannot credit the same contribution twice
    Given a core economy
    And a submitted child contribution worth 10 Ladirchen
    When the administrator approves it with 5 stars
    And the administrator tries to approve it again
    Then the economy command fails with "InvalidState"
    And the child wallet contains 11 Ladirchen
    And there are 1 ledger entries
    And the rejected command left the economy unchanged

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
    When the administrator returns it
    And the child submits it again
    Then the contribution status is "Pending"

  Scenario: Only the assignee can submit an available contribution
    Given an available contribution assigned to the child
    When another family member attempts to submit it
    Then the contribution remains available

  Scenario: Assigning an available contribution removes previous partner invitations
    Given an available extra contribution has invited partners
    When an administrator assigns the contribution to another member
    Then the contribution has no invited partners

  Scenario: Extra contributions preserve their configured energy
    Given an administrator creates an extra contribution with 20 energy
    When the contribution is added
    Then its stored energy is 20

  Scenario: Children and guardians can work in the same contribution team
    Given an available extra contribution assigned to a <assigneeRole>
    When an active <partnerRole> family member is invited as a partner
    Then the <partnerRole> family member belongs to the contribution team
    And the assignee cannot be invited as their own partner
    Examples:
      | assigneeRole | partnerRole |
      | child        | guardian    |
      | guardian     | child       |
