Feature: House progression and level-based unlocks
  The house level represents the family's shared progression.
  It unlocks house content and visual extensions, but does not directly change savings interest.
  House levels are stored from 0 to 4 and displayed as levels 1 to 5.

  Scenario: Each house level has a stage and room capacity [storedLevel=0; displayedLevel=1; stage=Starter Home; roomCount=2]
    Given the stored house level is 0
    When the house progression is displayed
    Then the displayed house level is 1
    And the house stage is "Starter Home"
    And 2 rooms are available

  Scenario: Each house level has a stage and room capacity [storedLevel=1; displayedLevel=2; stage=Family Home; roomCount=4]
    Given the stored house level is 1
    When the house progression is displayed
    Then the displayed house level is 2
    And the house stage is "Family Home"
    And 4 rooms are available

  Scenario: Each house level has a stage and room capacity [storedLevel=2; displayedLevel=3; stage=Garden Home; roomCount=7]
    Given the stored house level is 2
    When the house progression is displayed
    Then the displayed house level is 3
    And the house stage is "Garden Home"
    And 7 rooms are available

  Scenario: Each house level has a stage and room capacity [storedLevel=3; displayedLevel=4; stage=Tower Home; roomCount=10]
    Given the stored house level is 3
    When the house progression is displayed
    Then the displayed house level is 4
    And the house stage is "Tower Home"
    And 10 rooms are available

  Scenario: Each house level has a stage and room capacity [storedLevel=4; displayedLevel=5; stage=Dream Home; roomCount=12]
    Given the stored house level is 4
    When the house progression is displayed
    Then the displayed house level is 5
    And the house stage is "Dream Home"
    And 12 rooms are available

  Scenario: Rooms become available at their minimum house level [minimumLevel=0; room=Living Room]
    Given the stored house level is 0
    When the available house rooms are determined
    Then the "Living Room" room is available

  Scenario: Rooms become available at their minimum house level [minimumLevel=0; room=Kitchen]
    Given the stored house level is 0
    When the available house rooms are determined
    Then the "Kitchen" room is available

  Scenario: Rooms become available at their minimum house level [minimumLevel=1; room=Bathroom]
    Given the stored house level is 1
    When the available house rooms are determined
    Then the "Bathroom" room is available

  Scenario: Rooms become available at their minimum house level [minimumLevel=1; room=Children Room]
    Given the stored house level is 1
    When the available house rooms are determined
    Then the "Children Room" room is available

  Scenario: Rooms become available at their minimum house level [minimumLevel=2; room=Play Room]
    Given the stored house level is 2
    When the available house rooms are determined
    Then the "Play Room" room is available

  Scenario: Rooms become available at their minimum house level [minimumLevel=2; room=Nursery]
    Given the stored house level is 2
    When the available house rooms are determined
    Then the "Nursery" room is available

  Scenario: Rooms become available at their minimum house level [minimumLevel=2; room=Pet Room]
    Given the stored house level is 2
    When the available house rooms are determined
    Then the "Pet Room" room is available

  Scenario: Rooms become available at their minimum house level [minimumLevel=3; room=Bedroom]
    Given the stored house level is 3
    When the available house rooms are determined
    Then the "Bedroom" room is available

  Scenario: Rooms become available at their minimum house level [minimumLevel=3; room=Wardrobe Room]
    Given the stored house level is 3
    When the available house rooms are determined
    Then the "Wardrobe Room" room is available

  Scenario: Rooms become available at their minimum house level [minimumLevel=3; room=Gamer Room]
    Given the stored house level is 3
    When the available house rooms are determined
    Then the "Gamer Room" room is available

  Scenario: Rooms become available at their minimum house level [minimumLevel=4; room=Creative Room]
    Given the stored house level is 4
    When the available house rooms are determined
    Then the "Creative Room" room is available

  Scenario: Rooms become available at their minimum house level [minimumLevel=4; room=Makeup Room]
    Given the stored house level is 4
    When the available house rooms are determined
    Then the "Makeup Room" room is available

  Scenario: Completing a successful week advances the shared house progression
    Given the family energy is at least 60 percent
    And the stored house level is 2
    When a family member who can manage the family closes the family week as successful
    Then the stored house level is 3
    And the new house stage is revealed

  Scenario: A successful week cannot advance beyond the maximum house level
    Given the family energy is at least 60 percent
    And the stored house level is 4
    When a family member who can manage the family closes the family week as successful
    Then the stored house level remains 4

  Scenario: Insufficient family energy does not advance the house
    Given the family energy is below 60 percent
    When a family member who can manage the family attempts to close the family week as successful
    Then the week completion is rejected
    And the house progression remains unchanged

  Scenario: A family week below 60 percent energy lowers the house level
    Given the family energy is below 60 percent
    And the stored house level is 3
    When a family member who can manage the family closes the family week as failed
    Then the stored house level is 2
    And the lower house stage is revealed

  Scenario: A failed week cannot lower the house below the minimum level
    Given the family energy is below 60 percent
    And the stored house level is 0
    When a family member who can manage the family closes the family week as failed
    Then the stored house level remains 0

  Scenario: House content remains locked below its minimum level [currentLevel=1; contentType=furniture set; requiredLevel=2]
    Given the stored house level is 1
    And the furniture set requires house level 2
    When the child attempts to use the locked furniture set
    Then the action is rejected

  Scenario: House content remains locked below its minimum level [currentLevel=2; contentType=room design; requiredLevel=3]
    Given the stored house level is 2
    And the room design requires house level 3
    When the child attempts to use the locked room design
    Then the action is rejected

  Scenario: House content remains locked below its minimum level [currentLevel=3; contentType=house room; requiredLevel=4]
    Given the stored house level is 3
    And the house room requires house level 4
    When the child attempts to use the locked house room
    Then the action is rejected

  Scenario: Furniture cannot be moved into a locked room
    Given the stored house level is 2
    And a house room requires level 3
    When furniture is moved or placed into that room
    Then the furniture placement is rejected

  Scenario: Higher house levels reveal visual house extensions [houseLevel=1; extension=Upper Floor]
    Given the stored house level is 1
    When the family house is displayed
    Then the "Upper Floor" extension is visible

  Scenario: Higher house levels reveal visual house extensions [houseLevel=2; extension=Side Extension]
    Given the stored house level is 2
    When the family house is displayed
    Then the "Side Extension" extension is visible

  Scenario: Higher house levels reveal visual house extensions [houseLevel=3; extension=Tower]
    Given the stored house level is 3
    When the family house is displayed
    Then the "Tower" extension is visible

  Scenario: Higher house levels reveal visual house extensions [houseLevel=4; extension=Front Balcony]
    Given the stored house level is 4
    When the family house is displayed
    Then the "Front Balcony" extension is visible
