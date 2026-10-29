Feature: Family-member avatar appearance
  Avatar appearances use catalog-backed values and role-specific options.
  A family member may save only their own currently active appearance.

  Scenario: A missing child appearance receives a deterministic default variant
    Given a child has no saved avatar appearance
    When the child's appearance is resolved by their index among children
    Then a complete child appearance is returned
    And the same child index always receives the same default variant

  Scenario: A missing guardian appearance receives a deterministic guardian preset
    Given a guardian has no saved avatar appearance
    When the guardian's appearance is resolved by their index among guardians
    Then a complete adult or senior guardian appearance is returned

  Scenario: Avatar options respect the selected profile role [profile=child]
    Given the avatar editor is opened for a "child" profile
    When available avatar parts are listed
    Then only parts supporting "child" are available

  Scenario: Avatar options respect the selected profile role [profile=guardian]
    Given the avatar editor is opened for a "guardian" profile
    When available avatar parts are listed
    Then only parts supporting "guardian" are available

  Scenario: A family member can save only their own active avatar appearance
    Given a family member is signed in and their own profile is active
    When the member saves a valid avatar appearance
    Then the appearance is stored for the signed-in member
    And every other family member's appearance remains unchanged

  Scenario: An inactive or different family profile cannot be changed [profile=another family member]
    Given the signed-in member attempts to save an appearance for "another family member"
    When the appearance is submitted
    Then every avatar appearance remains unchanged

  Scenario: An inactive or different family profile cannot be changed [profile=a child who is not active]
    Given the signed-in member attempts to save an appearance for "a child who is not active"
    When the appearance is submitted
    Then every avatar appearance remains unchanged

  Scenario: Every saved avatar value must belong to its catalog [part=age]
    Given an avatar appearance contains an unknown "age" value
    When the family member attempts to save the appearance
    Then the appearance is rejected
    And the previously stored appearance remains unchanged

  Scenario: Every saved avatar value must belong to its catalog [part=skin tone]
    Given an avatar appearance contains an unknown "skin tone" value
    When the family member attempts to save the appearance
    Then the appearance is rejected
    And the previously stored appearance remains unchanged

  Scenario: Every saved avatar value must belong to its catalog [part=face shape]
    Given an avatar appearance contains an unknown "face shape" value
    When the family member attempts to save the appearance
    Then the appearance is rejected
    And the previously stored appearance remains unchanged

  Scenario: Every saved avatar value must belong to its catalog [part=face]
    Given an avatar appearance contains an unknown "face" value
    When the family member attempts to save the appearance
    Then the appearance is rejected
    And the previously stored appearance remains unchanged

  Scenario: Every saved avatar value must belong to its catalog [part=hair]
    Given an avatar appearance contains an unknown "hair" value
    When the family member attempts to save the appearance
    Then the appearance is rejected
    And the previously stored appearance remains unchanged

  Scenario: Every saved avatar value must belong to its catalog [part=hair color]
    Given an avatar appearance contains an unknown "hair color" value
    When the family member attempts to save the appearance
    Then the appearance is rejected
    And the previously stored appearance remains unchanged

  Scenario: Every saved avatar value must belong to its catalog [part=outfit]
    Given an avatar appearance contains an unknown "outfit" value
    When the family member attempts to save the appearance
    Then the appearance is rejected
    And the previously stored appearance remains unchanged

  Scenario: Every saved avatar value must belong to its catalog [part=outfit color]
    Given an avatar appearance contains an unknown "outfit color" value
    When the family member attempts to save the appearance
    Then the appearance is rejected
    And the previously stored appearance remains unchanged

  Scenario: Every saved avatar value must belong to its catalog [part=accessory]
    Given an avatar appearance contains an unknown "accessory" value
    When the family member attempts to save the appearance
    Then the appearance is rejected
    And the previously stored appearance remains unchanged

  Scenario: Every saved avatar value must belong to its catalog [part=fun accessory]
    Given an avatar appearance contains an unknown "fun accessory" value
    When the family member attempts to save the appearance
    Then the appearance is rejected
    And the previously stored appearance remains unchanged

  Scenario: Every saved avatar value must belong to its catalog [part=seasonal accessory]
    Given an avatar appearance contains an unknown "seasonal accessory" value
    When the family member attempts to save the appearance
    Then the appearance is rejected
    And the previously stored appearance remains unchanged
