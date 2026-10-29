Feature: House progression and level-based unlocks
  The house level represents the family's shared progression.
  It unlocks house content and visual extensions, but does not directly change savings interest.
  House levels are stored from 0 to 4 and displayed as levels 1 to 5.

  Scenario: Each house level has a stage and room capacity
    Given the stored house level is <storedLevel>
    When the house progression is displayed
    Then the displayed house level is <displayedLevel>
    And the house stage is "<stage>"
    And <roomCount> rooms are available
    Examples:
      | storedLevel | displayedLevel | stage        | roomCount |
      | 0           | 1              | Starter Home | 2         |
      | 1           | 2              | Family Home  | 4         |
      | 2           | 3              | Garden Home  | 7         |
      | 3           | 4              | Tower Home   | 10        |
      | 4           | 5              | Dream Home   | 12        |

  Scenario: Rooms become available at their minimum house level
    Given the stored house level is <minimumLevel>
    When the available house rooms are determined
    Then the "<room>" room is available
    Examples:
      | minimumLevel | room          |
      | 0            | Living Room   |
      | 0            | Kitchen       |
      | 1            | Bathroom      |
      | 1            | Children Room |
      | 2            | Play Room     |
      | 2            | Nursery       |
      | 2            | Pet Room      |
      | 3            | Bedroom       |
      | 3            | Wardrobe Room |
      | 3            | Gamer Room    |
      | 4            | Creative Room |
      | 4            | Makeup Room   |

  Scenario: Completing a successful week advances the shared house progression
    Given the family energy is at least 60 percent
    And the stored house level is 2
    When an authorized user closes the family week as successful
    Then the stored house level is 3
    And the new house stage is revealed

  Scenario: A successful week cannot advance beyond the maximum house level
    Given the family energy is at least 60 percent
    And the stored house level is 4
    When an authorized user closes the family week as successful
    Then the stored house level remains 4

  Scenario: Insufficient family energy does not advance the house
    Given the family energy is below 60 percent
    When an authorized user attempts to close the family week as successful
    Then the week completion is rejected
    And the house progression remains unchanged

  Scenario: A family week below 60 percent energy lowers the house level
    Given the family energy is below 60 percent
    And the stored house level is 3
    When an authorized user closes the family week as failed
    Then the stored house level is 2
    And the lower house stage is revealed

  Scenario: A failed week cannot lower the house below the minimum level
    Given the family energy is below 60 percent
    And the stored house level is 0
    When an authorized user closes the family week as failed
    Then the stored house level remains 0

  Scenario: House content remains locked below its minimum level
    Given the stored house level is <currentLevel>
    And the <contentType> requires house level <requiredLevel>
    When the child attempts to use the locked <contentType>
    Then the action is rejected
    Examples:
      | currentLevel | contentType  | requiredLevel |
      | 1            | furniture set | 2           |
      | 2            | room design   | 3           |
      | 3            | house room     | 4           |

  Scenario: Furniture cannot be moved into a locked room
    Given the stored house level is 2
    And a house room requires level 3
    When furniture is moved or placed into that room
    Then the furniture placement is rejected

  Scenario: Higher house levels reveal visual house extensions
    Given the stored house level is <houseLevel>
    When the family house is displayed
    Then the "<extension>" extension is visible
    Examples:
      | houseLevel | extension       |
      | 1          | Upper Floor     |
      | 2          | Side Extension  |
      | 3          | Tower           |
      | 4          | Front Balcony   |
