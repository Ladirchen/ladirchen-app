Feature: Personal family member invitations
  A guardian administrator prepares a family membership for each personal member account.
  An invited child or guardian remains inactive until the invitation is accepted.

  Scenario: A guardian administrator invites a child
    Given a family can invite personal members
    When the guardian administrator invites the child "Sam" with email "sam@example.com"
    Then the invited child is pending
    And the pending child does not participate in the weekly family goal
    And the pending child is not the active child

  Scenario: A guardian administrator invites a guardian
    Given a family can invite personal members
    When the guardian administrator invites the guardian "Taylor" with email "taylor@example.com"
    Then the invited guardian is pending
    And the invited guardian has supporter access
    And the pending guardian does not participate in the weekly family goal

  Scenario: Accepting a child invitation activates the first child
    Given a pending personal invitation for a child
    When the personal member invitation is accepted
    Then the invited child is active
    And the accepted child participates in the weekly family goal
    And the accepted child is the family's first active child
    And family onboarding is complete

  Scenario: Accepting a guardian invitation activates the guardian
    Given a pending personal invitation for a guardian
    When the personal member invitation is accepted
    Then the invited guardian is active
    And the accepted guardian does not participate in the weekly family goal

  Scenario: A member without family administration permission cannot invite another member
    Given a family has an active child without family administration permission
    When the child attempts to invite another family member
    Then the personal member invitation is rejected as forbidden
    And the rejected personal member invitation leaves the family unchanged

  Scenario: Invalid member details cannot create an invitation
    Given these invalid personal member invitations:
      | name | email                      | role     | guardianAccess |
      | A    | short-name@example.com     | child    | supporter      |
      | Sam  | invalid-email              | child    | supporter      |
      | Sam  | child-admin@example.com    | child    | admin          |
      | Sam  | invalid-role@example.com   | unknown  | supporter      |
      | Sam  | invalid-access@example.com | guardian | unknown        |
    When the invalid personal member invitations are submitted
    Then every invalid personal member invitation is rejected
    And no invalid personal member invitation changes the family

  Scenario: An email address cannot be assigned to two family members
    Given a personal member invitation already uses "sam@example.com"
    When the guardian administrator invites another member with email "SAM@example.com"
    Then the duplicate personal member invitation is rejected
    And the rejected personal member invitation leaves the family unchanged
