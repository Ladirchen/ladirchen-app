Feature: Family role permissions
  Permissions are derived from the signed-in family member's role and guardian access.

  Scenario: Family roles receive their configured capabilities [role=child; inviteMembers=denied; manageGoals=denied; manageContent=denied; manageFamily=denied; supportGoals=denied; viewFamilyGoals=allowed; viewGuardianGoals=denied]
    Given the signed-in member is a "child"
    When family permissions are resolved
    Then member invitation is denied
    And goal management is denied
    And content management is denied
    And family management is denied
    And child-goal support is denied
    And family-goal visibility is allowed
    And guardian-goal visibility is denied

  Scenario: Family roles receive their configured capabilities [role=guardian supporter; inviteMembers=denied; manageGoals=denied; manageContent=denied; manageFamily=denied; supportGoals=allowed; viewFamilyGoals=allowed; viewGuardianGoals=denied]
    Given the signed-in member is a "guardian supporter"
    When family permissions are resolved
    Then member invitation is denied
    And goal management is denied
    And content management is denied
    And family management is denied
    And child-goal support is allowed
    And family-goal visibility is allowed
    And guardian-goal visibility is denied

  Scenario: Family roles receive their configured capabilities [role=guardian administrator; inviteMembers=allowed; manageGoals=allowed; manageContent=allowed; manageFamily=allowed; supportGoals=allowed; viewFamilyGoals=allowed; viewGuardianGoals=allowed]
    Given the signed-in member is a "guardian administrator"
    When family permissions are resolved
    Then member invitation is allowed
    And goal management is allowed
    And content management is allowed
    And family management is allowed
    And child-goal support is allowed
    And family-goal visibility is allowed
    And guardian-goal visibility is allowed

  Scenario: A missing member receives no permissions
    Given no signed-in family member can be resolved
    When family permissions are resolved
    Then every family capability is denied
