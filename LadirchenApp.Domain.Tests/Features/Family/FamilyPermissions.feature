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
    And shop management is denied
    And push notifications are allowed

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
    And shop management is allowed
    And push notifications are allowed

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
    And shop management is allowed
    And push notifications are allowed

  Scenario: A missing member receives no permissions
    Given no signed-in family member can be resolved
    When family permissions are resolved
    Then every family capability is denied

  Scenario: External guardians manage only their own child tasks
    Given the signed-in member is a "guardian external"
    When family permissions are resolved
    Then content management is denied
    And child-task management is allowed
    And child-task review is allowed
    And task-assignment visibility is allowed
    And family-goal visibility is denied
    And guardian-goal visibility is denied
    And house visibility is denied
    And shop management is allowed
    And push notifications are denied
