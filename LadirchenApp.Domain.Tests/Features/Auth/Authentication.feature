Feature: Family account access
  Family account access identifies the family and the signed-in member.

  Scenario: A valid registration creates a family with its first administrator
    Given the family name and guardian name contain at least 2 characters
    And the username contains at least 3 characters
    And the password contains at least 6 characters
    And the password confirmation matches
    When the visitor registers a family account
    Then a new family is created
    And the registering guardian is its first administrator
    And that guardian is signed in

  Scenario: Valid credentials restore the corresponding family membership
    Given a local account belongs to a family member
    When that member signs in with the correct username and password
    Then the corresponding family is loaded
    And the corresponding member becomes the signed-in member

  Scenario: Invalid credentials do not start a family session
    Given no account matches the entered username and password
    When the visitor attempts to sign in
    Then no family session is started

  Scenario: Signing out ends the active family session
    Given a family member is signed in
    When the member signs out
    Then the active family session is cleared
