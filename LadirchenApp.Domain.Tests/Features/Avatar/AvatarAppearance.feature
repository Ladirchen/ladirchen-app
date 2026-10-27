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
