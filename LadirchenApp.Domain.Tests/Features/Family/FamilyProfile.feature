Feature: Family profiles and guardian participation
  A family member controls only their own personal profile data.
  Guardian administrators manage guardian invitations and access levels.
  Guardians may decide whether they participate in the weekly family goal.

  Scenario: A child nickname is limited to 18 characters [enteredNickname=Sunny; storedNickname=Sunny]
    Given the signed-in child is the active child
    When the child saves the nickname "Sunny"
    Then the stored nickname is "Sunny"

  Scenario: A child nickname is limited to 18 characters [enteredNickname=12345678901234567890; storedNickname=123456789012345678]
    Given the signed-in child is the active child
    When the child saves the nickname "12345678901234567890"
    Then the stored nickname is "123456789012345678"

  Scenario: A child nickname is limited to 18 characters [enteredNickname=empty; storedNickname=empty]
    Given the signed-in child is the active child
    When the child saves the nickname ""
    Then the stored nickname is ""

  Scenario: Surrounding whitespace is removed from a child nickname
    Given the signed-in child is the active child
    When the child saves the nickname "  Sunny  "
    Then the stored nickname is "Sunny"

  Scenario: Only the signed-in active child can change their nickname
    Given a nickname belongs to a child
    When a guardian or another child attempts to change that nickname
    Then the nickname remains unchanged

  Scenario: A blank nickname restores the family member's regular name
    Given the signed-in child has a nickname
    When the child saves a blank nickname
    Then the nickname is removed
    And the child's regular name is used for display

  Scenario: A valid guardian invitation records its initial state [access=supporter]
    Given a guardian administrator enters a valid name and email address
    And selects "supporter" guardian access
    When the guardian administrator invites the guardian
    Then the guardian is added with "supporter" access
    And the invitation is pending
    And the guardian does not participate in the weekly family goal

  Scenario: A valid guardian invitation records its initial state [access=admin]
    Given a guardian administrator enters a valid name and email address
    And selects "admin" guardian access
    When the guardian administrator invites the guardian
    Then the guardian is added with "admin" access
    And the invitation is pending
    And the guardian does not participate in the weekly family goal

  Scenario: Invalid or unauthorized guardian invitations are rejected [invalidInput=no member-invitation permission]
    Given a family member attempts a guardian invitation with "no member-invitation permission"
    When the invitation is submitted
    Then no guardian invitation is created

  Scenario: Invalid or unauthorized guardian invitations are rejected [invalidInput=a name shorter than two characters]
    Given a family member attempts a guardian invitation with "a name shorter than two characters"
    When the invitation is submitted
    Then no guardian invitation is created

  Scenario: Invalid or unauthorized guardian invitations are rejected [invalidInput=an invalid email address]
    Given a family member attempts a guardian invitation with "an invalid email address"
    When the invitation is submitted
    Then no guardian invitation is created

  Scenario: A guardian administrator can change another guardian's access
    Given the signed-in guardian can manage the family
    And another guardian belongs to the family
    When the guardian administrator changes that guardian from supporter to guardian administrator
    Then the other guardian has guardian administrator access

  Scenario: A guardian cannot change their own access
    Given a guardian is signed in
    When that guardian attempts to change their own access
    Then their guardian access remains unchanged

  Scenario: Guardian weekly-goal participation can be changed by that guardian or a guardian administrator [actor=that guardian]
    Given a guardian currently does not participate in the weekly family goal
    And the participation change is requested by "that guardian"
    When participation is enabled
    Then the guardian participates in the weekly family goal

  Scenario: Guardian weekly-goal participation can be changed by that guardian or a guardian administrator [actor=a guardian administrator]
    Given a guardian currently does not participate in the weekly family goal
    And the participation change is requested by "a guardian administrator"
    When participation is enabled
    Then the guardian participates in the weekly family goal

  Scenario: Unauthorized weekly-goal participation changes are rejected [target=another guardian; actor=guardian supporter]
    Given a family member attempts to change weekly-goal participation for "another guardian"
    And the actor is "guardian supporter"
    When participation is changed
    Then weekly-goal participation remains unchanged

  Scenario: Unauthorized weekly-goal participation changes are rejected [target=a child; actor=guardian administrator]
    Given a family member attempts to change weekly-goal participation for "a child"
    And the actor is "guardian administrator"
    When participation is changed
    Then weekly-goal participation remains unchanged

  Scenario: Only a guardian can select the active child
    Given the family has two children
    When a guardian selects the second child
    Then the second child becomes the active child
    When a child attempts to select another child
    Then the active child remains unchanged
