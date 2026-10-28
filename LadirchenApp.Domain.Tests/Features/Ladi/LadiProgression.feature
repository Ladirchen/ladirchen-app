Feature: Personal Ladi progression
  Each family member participating in the weekly family goal has a personal Ladi stage.
  The family member's personal participation determines one of six ordered Ladi stages.
  The participation value uses the family member's personal streak, effective completion and average basic rating.
  Effective completion is approved basic energy plus approved extra energy divided by the assigned basic-energy target.
  Effective completion is capped at 100 percent.
  With the default configuration, the participation value is:
  1 + streak days multiplied by 0.5 + completion ratio multiplied by 3.5 + rating ratio multiplied by 2.5.
  The result is capped at the maximum participation value of 8.
  Six equal proportional bands between zero and 8 select the ordered Ladi stage.
  Stage selection uses the unrounded capped value.
  Other family members' participation, the family streak and the house level do not affect the personal Ladi stage.

  Scenario: A weekly Ladi evaluation belongs to one family member and ISO week
    Given a family member's personal streak, completion and average basic rating for an ISO week
    When the personal Ladi evaluation is created
    Then it identifies the family member and ISO week
    And it contains the source participation values
    And it contains one capped participation value
    And it contains one Ladi stage and its ordered position

  Scenario: A closed week's Ladi evaluation is immutable
    Given a family member has a completed personal Ladi evaluation for a closed ISO week
    When later contributions change the family member's current Ladi stage
    Then the closed week's participation value and Ladi stage remain unchanged

  Scenario: An open week's Ladi preview follows current personal participation
    Given the current ISO week is still open
    When the family member's personal streak, completion or average basic rating changes
    Then the current Ladi preview is recalculated from the updated personal participation
    But no closed weekly Ladi evaluation is changed

  Scenario: Approved extra energy can improve the personal Ladi stage
    Given the family member has no personal streak and no rated basic contribution
    And the family member has 100 assigned basic energy
    And 70 basic energy is approved
    And <extraEnergy> extra energy is approved
    When the family member's personal Ladi stage is calculated
    Then the effective completion is <completion> percent
    And the personal Ladi stage is "<stageId>"
    Examples:
      | extraEnergy | completion | stageId     |
      | 0           | 70         | garden-ladi |
      | 20          | 90         | sun-ladi    |

  Scenario: The default weights calculate one personal participation value
    Given the family member has <streakDays> personal streak days
    And the family member's effective completion is <completion> percent
    And the family member's basic contributions have an average rating of <averageRating> stars
    When the family member's participation value is calculated with the default weights
    Then the uncapped participation value is <uncappedValue>
    And the capped participation value is <cappedValue>
    Examples:
      | streakDays | completion | averageRating | uncappedValue | cappedValue |
      | 0          | 0          | 0             | 1             | 1           |
      | 0          | 60         | 0             | 3.1           | 3.1         |
      | 1          | 50         | 5             | 5.75          | 5.75        |
      | 7          | 100        | 5             | 10.5          | 8           |
      | 0          | 33.33      | 2.5           | 3.41655       | 3.41655     |

  Scenario: Equal participation bands determine the ordered Ladi stage
    Given the maximum participation value is 8
    And there are 6 ordered Ladi stages
    And the family member's capped participation value is <participationValue>
    When the family member's personal Ladi stage is determined
    Then the personal Ladi stage is "<stageId>" at position <stageNumber>
    Examples:
      | participationValue | stageId      | stageNumber |
      | 1                  | idle-ladi    | 1           |
      | 1.34               | starter-ladi | 2           |
      | 3.1                | garden-ladi  | 3           |
      | 4.01               | sun-ladi     | 4           |
      | 5.75               | aurora-ladi  | 5           |
      | 8                  | super-ladi   | 6           |

  Scenario: Personal participation determines the Ladi stage
    Given the default participation configuration
    And the family member has <streakDays> personal streak days
    And the family member's effective completion is <completion> percent
    And the family member's basic contributions have an average rating of <averageRating> stars
    When the family member's capped participation value is calculated
    Then the personal Ladi stage is "<stageId>"
    Examples:
      | streakDays | completion | averageRating | stageId      |
      | 0          | 0          | 0             | idle-ladi    |
      | 0          | 60         | 0             | garden-ladi  |
      | 1          | 50         | 5             | aurora-ladi  |
      | 7          | 100        | 5             | super-ladi   |
      | 0          | 33.33      | 2.5           | garden-ladi  |

  Scenario: The maximum participation value always selects Super Ladi
    Given the family member reaches or exceeds the configured maximum participation value
    When the family member's personal Ladi stage is determined
    Then the personal Ladi stage is "super-ladi"

  Scenario: Each participating family member receives an independent Ladi stage
    Given two participating family members have different personal streaks, completion and average ratings
    When their personal Ladi stages are determined
    Then each stage uses only the corresponding family member's participation

  Scenario: The same evaluated Ladi stage supplies every dependent policy
    Given a family member's weekly Ladi evaluation contains "sun-ladi" at position 4
    When personal savings interest and family energy are determined
    Then the interest policy uses "sun-ladi" at position 4
    And the house-energy policy uses "sun-ladi" at position 4
    And neither policy recalculates personal participation
