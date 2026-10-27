Feature: Contribution participation evaluation
  Participation is evaluated separately for each family member.
  Assigned basic energy defines the completion target.
  Approved basic and extra energy contribute to effective completion, capped at 100 percent.
  Only basic ratings determine average rating, while approved contribution dates determine active days.

  Scenario: Extra ratings do not change the average basic rating
    Given a core economy
    And these approved contributions:
      | id    | kind  | stars | at                   |
      | basic | Basic | 5     | 2026-10-10T10:00:00Z |
      | extra | Extra | 1     | 2026-10-10T11:00:00Z |
    When participation for week 41 of 2026 is evaluated
    Then participation is 1 days, 100 percent and 5 stars

  Scenario: Approved extra energy improves effective completion
    Given a member has 100 assigned basic energy
    And 70 basic energy is approved
    And 20 extra energy is approved
    When the member's completion is evaluated
    Then completion is 90 percent
    And contributions assigned to other members are ignored

  Scenario: Extra energy cannot raise effective completion above 100 percent
    Given a member has 100 assigned basic energy
    And 100 basic energy is approved
    And 30 extra energy is approved
    When the member's completion is evaluated
    Then completion is 100 percent

  Scenario: Unapproved extra energy does not increase effective completion
    Given a member has 100 assigned basic energy
    And 70 basic energy is approved
    And an extra contribution with 20 energy is not approved
    When the member's completion is evaluated
    Then completion is 70 percent

  Scenario: A member without assigned basic contribution energy has default completion
    Given a member has no assigned basic contribution energy
    When the member's completion is evaluated
    Then completion is 60 percent

  Scenario: Average rating uses only the member's rated basic contributions
    Given multiple family members have rated basic and extra contributions
    When one member's average rating is evaluated
    Then only that member's rated basic contributions are averaged
    And unrated and extra contributions are ignored
