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

  Scenario: Approved extra energy can improve the personal Ladi stage [extraEnergy=0; completion=70; stageId=garden-ladi]
    Given the family member has no personal streak and no rated basic contribution
    And the family member has 100 assigned basic energy
    And 70 basic energy is approved
    And 0 extra energy is approved
    When the family member's personal Ladi stage is calculated
    Then the effective completion is 70 percent
    And the personal Ladi stage is "garden-ladi"

  Scenario: Approved extra energy can improve the personal Ladi stage [extraEnergy=20; completion=90; stageId=sun-ladi]
    Given the family member has no personal streak and no rated basic contribution
    And the family member has 100 assigned basic energy
    And 70 basic energy is approved
    And 20 extra energy is approved
    When the family member's personal Ladi stage is calculated
    Then the effective completion is 90 percent
    And the personal Ladi stage is "sun-ladi"

  Scenario: The default weights calculate one personal participation value [streakDays=0; completion=0; averageRating=0; uncappedValue=1; cappedValue=1]
    Given the family member has 0 personal streak days
    And the family member's effective completion is 0 percent
    And the family member's basic contributions have an average rating of 0 stars
    When the family member's participation value is calculated with the default weights
    Then the uncapped participation value is 1
    And the capped participation value is 1

  Scenario: The default weights calculate one personal participation value [streakDays=0; completion=60; averageRating=0; uncappedValue=3.1; cappedValue=3.1]
    Given the family member has 0 personal streak days
    And the family member's effective completion is 60 percent
    And the family member's basic contributions have an average rating of 0 stars
    When the family member's participation value is calculated with the default weights
    Then the uncapped participation value is 3.1
    And the capped participation value is 3.1

  Scenario: The default weights calculate one personal participation value [streakDays=1; completion=50; averageRating=5; uncappedValue=5.75; cappedValue=5.75]
    Given the family member has 1 personal streak days
    And the family member's effective completion is 50 percent
    And the family member's basic contributions have an average rating of 5 stars
    When the family member's participation value is calculated with the default weights
    Then the uncapped participation value is 5.75
    And the capped participation value is 5.75

  Scenario: The default weights calculate one personal participation value [streakDays=7; completion=100; averageRating=5; uncappedValue=10.5; cappedValue=8]
    Given the family member has 7 personal streak days
    And the family member's effective completion is 100 percent
    And the family member's basic contributions have an average rating of 5 stars
    When the family member's participation value is calculated with the default weights
    Then the uncapped participation value is 10.5
    And the capped participation value is 8

  Scenario: The default weights calculate one personal participation value [streakDays=0; completion=33.33; averageRating=2.5; uncappedValue=3.41655; cappedValue=3.41655]
    Given the family member has 0 personal streak days
    And the family member's effective completion is 33.33 percent
    And the family member's basic contributions have an average rating of 2.5 stars
    When the family member's participation value is calculated with the default weights
    Then the uncapped participation value is 3.41655
    And the capped participation value is 3.41655

  Scenario: Equal participation bands determine the ordered Ladi stage [participationValue=1; stageId=idle-ladi; stageNumber=1]
    Given the maximum participation value is 8
    And there are 6 ordered Ladi stages
    And the family member's capped participation value is 1
    When the family member's personal Ladi stage is determined
    Then the personal Ladi stage is "idle-ladi" at position 1

  Scenario: Equal participation bands determine the ordered Ladi stage [participationValue=1.34; stageId=starter-ladi; stageNumber=2]
    Given the maximum participation value is 8
    And there are 6 ordered Ladi stages
    And the family member's capped participation value is 1.34
    When the family member's personal Ladi stage is determined
    Then the personal Ladi stage is "starter-ladi" at position 2

  Scenario: Equal participation bands determine the ordered Ladi stage [participationValue=3.1; stageId=garden-ladi; stageNumber=3]
    Given the maximum participation value is 8
    And there are 6 ordered Ladi stages
    And the family member's capped participation value is 3.1
    When the family member's personal Ladi stage is determined
    Then the personal Ladi stage is "garden-ladi" at position 3

  Scenario: Equal participation bands determine the ordered Ladi stage [participationValue=4.01; stageId=sun-ladi; stageNumber=4]
    Given the maximum participation value is 8
    And there are 6 ordered Ladi stages
    And the family member's capped participation value is 4.01
    When the family member's personal Ladi stage is determined
    Then the personal Ladi stage is "sun-ladi" at position 4

  Scenario: Equal participation bands determine the ordered Ladi stage [participationValue=5.75; stageId=aurora-ladi; stageNumber=5]
    Given the maximum participation value is 8
    And there are 6 ordered Ladi stages
    And the family member's capped participation value is 5.75
    When the family member's personal Ladi stage is determined
    Then the personal Ladi stage is "aurora-ladi" at position 5

  Scenario: Equal participation bands determine the ordered Ladi stage [participationValue=8; stageId=super-ladi; stageNumber=6]
    Given the maximum participation value is 8
    And there are 6 ordered Ladi stages
    And the family member's capped participation value is 8
    When the family member's personal Ladi stage is determined
    Then the personal Ladi stage is "super-ladi" at position 6

  Scenario: Personal participation determines the Ladi stage [streakDays=0; completion=0; averageRating=0; stageId=idle-ladi]
    Given the default participation configuration
    And the family member has 0 personal streak days
    And the family member's effective completion is 0 percent
    And the family member's basic contributions have an average rating of 0 stars
    When the family member's capped participation value is calculated
    Then the personal Ladi stage is "idle-ladi"

  Scenario: Personal participation determines the Ladi stage [streakDays=0; completion=60; averageRating=0; stageId=garden-ladi]
    Given the default participation configuration
    And the family member has 0 personal streak days
    And the family member's effective completion is 60 percent
    And the family member's basic contributions have an average rating of 0 stars
    When the family member's capped participation value is calculated
    Then the personal Ladi stage is "garden-ladi"

  Scenario: Personal participation determines the Ladi stage [streakDays=1; completion=50; averageRating=5; stageId=aurora-ladi]
    Given the default participation configuration
    And the family member has 1 personal streak days
    And the family member's effective completion is 50 percent
    And the family member's basic contributions have an average rating of 5 stars
    When the family member's capped participation value is calculated
    Then the personal Ladi stage is "aurora-ladi"

  Scenario: Personal participation determines the Ladi stage [streakDays=7; completion=100; averageRating=5; stageId=super-ladi]
    Given the default participation configuration
    And the family member has 7 personal streak days
    And the family member's effective completion is 100 percent
    And the family member's basic contributions have an average rating of 5 stars
    When the family member's capped participation value is calculated
    Then the personal Ladi stage is "super-ladi"

  Scenario: Personal participation determines the Ladi stage [streakDays=0; completion=33.33; averageRating=2.5; stageId=garden-ladi]
    Given the default participation configuration
    And the family member has 0 personal streak days
    And the family member's effective completion is 33.33 percent
    And the family member's basic contributions have an average rating of 2.5 stars
    When the family member's capped participation value is calculated
    Then the personal Ladi stage is "garden-ladi"

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
