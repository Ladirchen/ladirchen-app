Feature: Family profiles and guardian participation
  A family member controls only their own personal profile data.
  Family administrators manage guardian invitations and access levels.
  Guardians may decide whether they participate in the weekly family goal.

  Scenario: A child nickname is limited to 18 characters
    Given the signed-in child is the active child
    When the child saves the nickname "<enteredNickname>"
    Then the stored nickname is "<storedNickname>"
    Examples:
      | enteredNickname         | storedNickname     |
      | Sunny                   | Sunny              |
      | 12345678901234567890    | 123456789012345678 |
      |                         |                    |

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

  Scenario: A valid guardian invitation records its initial state
    Given a family administrator enters a valid name and email address
    And selects "<access>" guardian access
    When the administrator invites the guardian
    Then the guardian is added with "<access>" access
    And the invitation is pending
    And the guardian does not participate in the weekly family goal
    Examples:
      | access    |
      | supporter |
      | admin     |

  Scenario: Invalid or unauthorized guardian invitations are rejected
    Given a family member attempts a guardian invitation with "<invalidInput>"
    When the invitation is submitted
    Then no guardian invitation is created
    Examples:
      | invalidInput                         |
      | no member-invitation permission      |
      | a name shorter than two characters   |
      | an invalid email address              |

  Scenario: An administrator can change another guardian's access
    Given the signed-in guardian can manage the family
    And another guardian belongs to the family
    When the administrator changes that guardian from supporter to administrator
    Then the other guardian has administrator access

  Scenario: A guardian cannot change their own access
    Given a guardian is signed in
    When that guardian attempts to change their own access
    Then their guardian access remains unchanged

  Scenario: Guardian weekly-goal participation can be changed by an authorized member
    Given a guardian currently does not participate in the weekly family goal
    And the participation change is requested by "<actor>"
    When participation is enabled
    Then the guardian participates in the weekly family goal
    Examples:
      | actor                              |
      | that guardian                      |
      | a family administrator             |

  Scenario: Unauthorized weekly-goal participation changes are rejected
    Given a family member attempts to change weekly-goal participation for "<target>"
    And the actor is "<actor>"
    When participation is changed
    Then weekly-goal participation remains unchanged
    Examples:
      | target             | actor                         |
      | another guardian   | guardian supporter            |
      | a child            | family administrator          |

  Scenario: Only a guardian can select the active child
    Given the family has two children
    When a guardian selects the second child
    Then the second child becomes the active child
    When a child attempts to select another child
    Then the active child remains unchanged
