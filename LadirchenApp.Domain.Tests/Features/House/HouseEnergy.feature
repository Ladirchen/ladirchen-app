Feature: Family energy from personal Ladi stages
  Each family member participating in the weekly family goal contributes energy according to their personal Ladi stage.
  Family energy is the rounded average of these stage contributions.
  The energy value controls the visual state of the house independently from the stored house level.

  Scenario: Each personal Ladi stage has a fixed energy contribution
    Given a participating family member has the personal Ladi stage "<stageId>"
    When the member's contribution to family energy is determined
    Then the member contributes <energy> percent energy
    Examples:
      | stageId      | energy |
      | idle-ladi    | 0      |
      | starter-ladi | 20     |
      | garden-ladi  | 40     |
      | sun-ladi     | 60     |
      | aurora-ladi  | 80     |
      | super-ladi   | 100    |

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

  Scenario: Energy determines the house visual level
    Given family energy is <energy> percent
    When the house visual level is determined
    Then the visual level is <visualLevel>
    Examples:
      | energy | visualLevel |
      | 0      | 1           |
      | 19     | 1           |
      | 20     | 2           |
      | 39     | 2           |
      | 40     | 3           |
      | 64     | 3           |
      | 65     | 4           |
      | 84     | 4           |
      | 85     | 5           |
      | 100    | 5           |

  Scenario: A successful family week requires at least Sun-level average energy
    Given family energy is <energy> percent
    When an authorized user attempts to complete the family week
    Then the week completion is <result>
    Examples:
      | energy | result   |
      | 59     | rejected |
      | 60     | accepted |
