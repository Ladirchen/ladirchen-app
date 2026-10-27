Feature: Family role permissions
  Permissions are derived from the signed-in family member's role and guardian access.

  Scenario: Family roles receive their configured capabilities
    Given the signed-in member is a "<role>"
    When family permissions are resolved
    Then content management is <manageContent>
    And family management is <manageFamily>
    And child-goal support is <supportGoals>
    And family-goal visibility is <viewFamilyGoals>
    And guardian-goal visibility is <viewGuardianGoals>
    Examples:
      | role               | manageContent | manageFamily | supportGoals | viewFamilyGoals | viewGuardianGoals |
      | child              | denied        | denied       | denied       | allowed         | denied            |
      | guardian supporter | denied        | denied       | allowed      | allowed         | denied            |
      | guardian admin     | allowed       | allowed      | allowed      | allowed         | allowed           |

  Scenario: A missing member receives no permissions
    Given no signed-in family member can be resolved
    When family permissions are resolved
    Then every family capability is denied
