Feature: Family energy from personal Ladi stages
  Each family member participating in the weekly family goal contributes energy according to their personal Ladi stage.
  Family energy is the rounded average of these stage contributions.
  The energy value controls the visual state of the house independently from the stored house level.

  Scenario: Each personal Ladi stage has a fixed energy contribution [stageId=idle-ladi; energy=0]
    Given a participating family member has the personal Ladi stage "idle-ladi"
    When the member's contribution to family energy is determined
    Then the member contributes 0 percent energy

  Scenario: Each personal Ladi stage has a fixed energy contribution [stageId=starter-ladi; energy=20]
    Given a participating family member has the personal Ladi stage "starter-ladi"
    When the member's contribution to family energy is determined
    Then the member contributes 20 percent energy

  Scenario: Each personal Ladi stage has a fixed energy contribution [stageId=garden-ladi; energy=40]
    Given a participating family member has the personal Ladi stage "garden-ladi"
    When the member's contribution to family energy is determined
    Then the member contributes 40 percent energy

  Scenario: Each personal Ladi stage has a fixed energy contribution [stageId=sun-ladi; energy=60]
    Given a participating family member has the personal Ladi stage "sun-ladi"
    When the member's contribution to family energy is determined
    Then the member contributes 60 percent energy

  Scenario: Each personal Ladi stage has a fixed energy contribution [stageId=aurora-ladi; energy=80]
    Given a participating family member has the personal Ladi stage "aurora-ladi"
    When the member's contribution to family energy is determined
    Then the member contributes 80 percent energy

  Scenario: Each personal Ladi stage has a fixed energy contribution [stageId=super-ladi; energy=100]
    Given a participating family member has the personal Ladi stage "super-ladi"
    When the member's contribution to family energy is determined
    Then the member contributes 100 percent energy

  Scenario: Family energy averages the Ladi stages of all participants
    Given the participating family members have these personal Ladi stages:
      | member   | stageId     |
      | child    | super-ladi  |
      | sibling  | garden-ladi |
      | guardian | sun-ladi    |
    When family energy is calculated
    Then family energy is 67 percent

  Scenario: Non-participating guardians do not contribute to family energy
    Given a guardian does not participate in the weekly family goal
    And the guardian has a personal Ladi stage
    When family energy is calculated
    Then that guardian's Ladi stage is ignored

  Scenario: Pets do not contribute to family energy
    Given the family has pets
    When family energy is calculated
    Then pets are ignored

  Scenario: A family without participants uses the default energy
    Given no family member participates in the weekly family goal
    When family energy is calculated
    Then family energy is 60 percent

  Scenario: Family energy does not recalculate personal participation
    Given every participant's personal Ladi stage has already been determined
    When family energy is calculated
    Then the house-energy policy uses only the participants' Ladi stages
    And it does not recalculate streak, completion or average rating

  Scenario: Extra contributions can change family energy only by changing a Ladi stage
    Given an approved extra contribution improves a participant's effective completion
    When the participant's personal Ladi stage changes from "garden-ladi" to "sun-ladi"
    Then the participant's energy contribution changes from 40 percent to 60 percent
    And the extra contribution adds no separate amount directly to family energy

  Scenario: Energy determines the house visual level [energy=0; visualLevel=1]
    Given family energy is 0 percent
    When the house visual level is determined
    Then the visual level is 1

  Scenario: Energy determines the house visual level [energy=19; visualLevel=1]
    Given family energy is 19 percent
    When the house visual level is determined
    Then the visual level is 1

  Scenario: Energy determines the house visual level [energy=20; visualLevel=2]
    Given family energy is 20 percent
    When the house visual level is determined
    Then the visual level is 2

  Scenario: Energy determines the house visual level [energy=39; visualLevel=2]
    Given family energy is 39 percent
    When the house visual level is determined
    Then the visual level is 2

  Scenario: Energy determines the house visual level [energy=40; visualLevel=3]
    Given family energy is 40 percent
    When the house visual level is determined
    Then the visual level is 3

  Scenario: Energy determines the house visual level [energy=64; visualLevel=3]
    Given family energy is 64 percent
    When the house visual level is determined
    Then the visual level is 3

  Scenario: Energy determines the house visual level [energy=65; visualLevel=4]
    Given family energy is 65 percent
    When the house visual level is determined
    Then the visual level is 4

  Scenario: Energy determines the house visual level [energy=84; visualLevel=4]
    Given family energy is 84 percent
    When the house visual level is determined
    Then the visual level is 4

  Scenario: Energy determines the house visual level [energy=85; visualLevel=5]
    Given family energy is 85 percent
    When the house visual level is determined
    Then the visual level is 5

  Scenario: Energy determines the house visual level [energy=100; visualLevel=5]
    Given family energy is 100 percent
    When the house visual level is determined
    Then the visual level is 5

  Scenario: A successful family week requires at least Sun-level average energy [energy=59; result=rejected]
    Given family energy is 59 percent
    When a family member who can manage the family attempts to complete the family week
    Then the week completion is rejected

  Scenario: A successful family week requires at least Sun-level average energy [energy=60; result=accepted]
    Given family energy is 60 percent
    When a family member who can manage the family attempts to complete the family week
    Then the week completion is accepted
