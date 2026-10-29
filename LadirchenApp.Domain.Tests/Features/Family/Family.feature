Feature: Family membership
  Scenario: Guardian administrators can add children
    Given a family with a guardian administrator
    When the guardian administrator adds a child
    Then the family has 2 active members

  Scenario: A child cannot manage family membership
    Given a family with a guardian administrator
    And the guardian administrator adds a child
    When the child tries to add a member
    Then the family command fails with "Forbidden"
    And the family has 2 active members

  Scenario: The last guardian administrator cannot be demoted
    Given a family with a guardian administrator
    When the guardian administrator tries to become a supporter
    Then the family command fails with "LastAdministrator"
    And the family has 1 active members

  Scenario: Multiple guardian administrators are allowed
    Given a family with a guardian administrator
    When a second guardian administrator is added
    And the guardian administrator tries to become a supporter
    Then the family has 2 active members
    And the number of guardian administrators is 1
