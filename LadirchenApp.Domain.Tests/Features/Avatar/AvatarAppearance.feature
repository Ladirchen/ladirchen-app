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

  Scenario: Avatar options respect the selected profile role
    Given the avatar editor is opened for a "<profile>" profile
    When available avatar parts are listed
    Then only parts supporting "<profile>" are available
    Examples:
      | profile  |
      | child    |
      | guardian |

  Scenario: A family member can save only their own active avatar appearance
    Given a family member is signed in and their own profile is active
    When the member saves a valid avatar appearance
    Then the appearance is stored for the signed-in member
    And every other family member's appearance remains unchanged

  Scenario: An inactive or different family profile cannot be changed
    Given the signed-in member attempts to save an appearance for "<profile>"
    When the appearance is submitted
    Then every avatar appearance remains unchanged
    Examples:
      | profile                         |
      | another family member           |
      | a child who is not active       |

  Scenario: Every saved avatar value must belong to its catalog
    Given an avatar appearance contains an unknown "<part>" value
    When the family member attempts to save the appearance
    Then the appearance is rejected
    And the previously stored appearance remains unchanged
    Examples:
      | part               |
      | age                |
      | skin tone          |
      | face shape         |
      | face               |
      | hair               |
      | hair color         |
      | outfit             |
      | outfit color       |
      | accessory          |
      | fun accessory      |
      | seasonal accessory |
