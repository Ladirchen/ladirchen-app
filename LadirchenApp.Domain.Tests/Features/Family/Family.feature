Feature: Family membership
  Scenario: Administrators can add children
    Given a family with an administrator
    When the administrator adds a child
    Then the family has 2 active members

  Scenario: A child cannot manage family membership
    Given a family with an administrator
    And the administrator adds a child
    When the child tries to add a member
    Then the family command fails with "Forbidden"
    And the family has 2 active members

  Scenario: The last administrator cannot be demoted
    Given a family with an administrator
    When the administrator tries to become a supporter
    Then the family command fails with "LastAdministrator"
    And the family has 1 active members

  Scenario: Multiple administrators are allowed
    Given a family with an administrator
    When a second administrator is added
    And the administrator tries to become a supporter
    Then the family has 2 active members
    And the family has 1 administrators
