Feature: Family setup
  A usable family requires named children and guardians. Pets are optional.

  Scenario: A family requires at least one named child and one named guardian
    Given family setup has been started
    When at least one child and one guardian have non-blank names
    Then the family setup can be completed

  Scenario: A new child participates in the weekly family goal by default
    Given family setup has been started
    When a child is added during family setup
    Then the child participates in the weekly family goal

  Scenario: A new guardian starts as a non-participating supporter
    Given family setup has been started
    When a guardian is added during family setup
    Then the guardian has supporter access
    And the guardian does not participate in the weekly family goal

  Scenario: Completed onboarding can be edited without replacing unchanged family data
    Given family onboarding has already been completed
    When an administrator edits the family setup
    Then the existing children, guardians and pets are available for editing
    And only saved changes replace the family setup
