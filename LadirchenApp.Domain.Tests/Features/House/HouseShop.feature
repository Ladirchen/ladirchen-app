Feature: House shop
  Children can spend available Ladirchen on house items from the house catalog.
  House items include accessories, furniture sets, house themes and room designs.
  Ownership, house-level requirements and furniture mobility control how purchased house items can be used.

  Scenario: Purchasing an accessory stores it as owned and unequipped
    Given a child can afford an unowned house accessory
    When the child purchases the accessory
    Then its price is deducted from the child's wallet
    And the accessory is owned
    And the accessory is stored unequipped

  Scenario: An accessory purchase is rejected when it is not allowed [accessoryState=unowned; viewer=guardian; balanceState=sufficient]
    Given a house accessory is "unowned"
    And the viewer is "guardian"
    And the available balance is "sufficient"
    When the viewer attempts to purchase the accessory
    Then ownership and balance remain unchanged

  Scenario: An accessory purchase is rejected when it is not allowed [accessoryState=owned; viewer=child; balanceState=sufficient]
    Given a house accessory is "owned"
    And the viewer is "child"
    And the available balance is "sufficient"
    When the viewer attempts to purchase the accessory
    Then ownership and balance remain unchanged

  Scenario: An accessory purchase is rejected when it is not allowed [accessoryState=unowned; viewer=child; balanceState=insufficient]
    Given a house accessory is "unowned"
    And the viewer is "child"
    And the available balance is "insufficient"
    When the viewer attempts to purchase the accessory
    Then ownership and balance remain unchanged

  Scenario: Purchasing a furniture set grants every accessory in the set
    Given a child can afford an unowned furniture set
    And the house meets the set's minimum level
    When the child purchases the furniture set
    Then the set price is deducted from the child's wallet
    And every accessory in the set is owned
    And every accessory in the set is stored unequipped

  Scenario: A furniture set purchase respects level, ownership and available balance [ownership=unowned; levelState=met; balanceState=sufficient; result=accepted]
    Given a furniture set is "unowned"
    And its minimum house level is "met"
    And the child's available balance is "sufficient"
    When the child attempts to purchase the furniture set
    Then the purchase is accepted

  Scenario: A furniture set purchase respects level, ownership and available balance [ownership=unowned; levelState=locked; balanceState=sufficient; result=rejected]
    Given a furniture set is "unowned"
    And its minimum house level is "locked"
    And the child's available balance is "sufficient"
    When the child attempts to purchase the furniture set
    Then the purchase is rejected

  Scenario: A furniture set purchase respects level, ownership and available balance [ownership=owned; levelState=met; balanceState=sufficient; result=rejected]
    Given a furniture set is "owned"
    And its minimum house level is "met"
    And the child's available balance is "sufficient"
    When the child attempts to purchase the furniture set
    Then the purchase is rejected

  Scenario: A furniture set purchase respects level, ownership and available balance [ownership=unowned; levelState=met; balanceState=insufficient; result=rejected]
    Given a furniture set is "unowned"
    And its minimum house level is "met"
    And the child's available balance is "insufficient"
    When the child attempts to purchase the furniture set
    Then the purchase is rejected

  Scenario: Purchasing a house theme owns and selects the theme
    Given a child can afford an unowned house theme
    When the child purchases the theme
    Then the theme price is deducted from the child's wallet
    And the theme is owned and selected
    And the theme's room designs are owned and selected for their rooms

  Scenario: Only an owned house theme can be selected
    Given the family does not own a house theme
    When a family member attempts to select it
    Then the selected house theme remains unchanged

  Scenario: Selecting an owned house theme applies all of its room designs
    Given the family owns a house theme
    When a family member selects the theme
    Then the house theme is selected
    And every room design belonging to the theme is owned
    And each of those designs is selected for its room

  Scenario: Purchasing a room design requires its theme and house level
    Given a child can afford an unowned room design
    And the family owns the design's house theme
    And the house meets the design's minimum level
    When the child purchases the room design
    Then the design price is deducted from the child's wallet
    And the design is owned and selected for its room
    And its house theme becomes selected

  Scenario: A room design cannot be purchased without its requirements [missingRequirement=minimum house level]
    Given the room design purchase requirement "minimum house level" is not met
    When the child attempts to purchase the room design
    Then ownership, selection and balance remain unchanged

  Scenario: A room design cannot be purchased without its requirements [missingRequirement=owned house theme]
    Given the room design purchase requirement "owned house theme" is not met
    When the child attempts to purchase the room design
    Then ownership, selection and balance remain unchanged

  Scenario: A room design cannot be purchased without its requirements [missingRequirement=available balance]
    Given the room design purchase requirement "available balance" is not met
    When the child attempts to purchase the room design
    Then ownership, selection and balance remain unchanged

  Scenario: Only an owned and level-eligible room design can be selected
    Given a room design is not owned or requires a higher house level
    When a family member attempts to select the room design
    Then the selected room design remains unchanged

  Scenario: Family members can freely arrange movable house entities
    Given the family owns a movable house entity
    And its target area is unlocked
    When a family member moves the entity into that area
    Then the house layout contains the new placement

  Scenario: Fixed furniture cannot be moved, stored or placed
    Given an owned furniture item has fixed mobility
    When a family member attempts to move, store or place it
    Then the furniture placement remains unchanged

  Scenario: An equipped movable furniture item can be stored
    Given an owned movable furniture item is equipped
    When a family member stores it
    Then the furniture item is unequipped

  Scenario: Stored movable furniture can be placed only in an unlocked compatible area
    Given an owned movable furniture item is stored
    When a family member places it in an unlocked compatible area
    Then the item is equipped in that area
    And its placement uses the area's default position

  Scenario: Indoor and outdoor furniture remain in compatible areas [placement=outside; requestedArea=living room; actualArea=garden]
    Given an owned stored furniture item is intended for "outside" use
    When a family member requests placement in "living room"
    Then the furniture is placed in "garden"

  Scenario: Indoor and outdoor furniture remain in compatible areas [placement=inside; requestedArea=garden; actualArea=its assigned room]
    Given an owned stored furniture item is intended for "inside" use
    When a family member requests placement in "garden"
    Then the furniture is placed in "its assigned room"

  Scenario: A family member can restore a movable entity's initial placement
    Given a family member has moved a movable house entity
    When a family member resets the entity's placement
    Then its initial area, position and scale are restored

  Scenario: Owning every accessory in a furniture set means owning the set
    Given the family owns every accessory belonging to a furniture set
    When ownership of the furniture set is determined
    Then the furniture set is owned

  Scenario: The house catalog separates regular and special house items [category=furniture; catalogArea=inside]
    Given a house accessory belongs to the "furniture" category
    When a family member views the house shop
    Then the house item is shown in the "inside" catalog area

  Scenario: The house catalog separates regular and special house items [category=garden; catalogArea=outside]
    Given a house accessory belongs to the "garden" category
    When a family member views the house shop
    Then the house item is shown in the "outside" catalog area

  Scenario: The house catalog separates regular and special house items [category=special; catalogArea=special]
    Given a house accessory belongs to the "special" category
    When a family member views the house shop
    Then the house item is shown in the "special" catalog area

  Scenario: Catalog entries contain the facts required for a purchase decision
    Given a family member views the house catalog
    When a house item is presented
    Then its identifier, price, catalog area and minimum house level are provided
    And its ownership and purchase availability are provided for the current family

  Scenario: Moved house-item coordinates are rounded and bounded [requestedX=40.126; requestedY=70.124; storedX=40.13; storedY=70.12]
    Given an owned movable house item can be arranged in an unlocked area
    When a family member moves it to x 40.126 and y 70.124
    Then its stored x coordinate is 40.13
    And its stored y coordinate is 70.12

  Scenario: Moved house-item coordinates are rounded and bounded [requestedX=-10; requestedY=2; storedX=3; storedY=52]
    Given an owned movable house item can be arranged in an unlocked area
    When a family member moves it to x -10 and y 2
    Then its stored x coordinate is 3
    And its stored y coordinate is 52

  Scenario: Moved house-item coordinates are rounded and bounded [requestedX=120; requestedY=110; storedX=97; storedY=94]
    Given an owned movable house item can be arranged in an unlocked area
    When a family member moves it to x 120 and y 110
    Then its stored x coordinate is 97
    And its stored y coordinate is 94

  Scenario: Family members and pets remain on the house floor
    Given a family member is moved to x -10 and y 2
    When the authoritative house layout is calculated
    Then the normalized house position is x 7 and y 62

  Scenario: The Ladi snaps to its living-room perch
    Given the Ladi is moved inside the living-room perch area
    When the authoritative house layout is calculated
    Then the normalized house position is x 18 and y 30

  Scenario: Family members and pets cannot overlap equipped furniture
    Given an equipped sofa occupies the requested family-member position
    When the authoritative house layout is validated
    Then the house placement is rejected

  Scenario: The New badge follows its local calendar-date schedule [localDateTime=2026-09-09T23:59:59; visibility=hidden]
    Given a house item has a New-badge schedule from "2026-09-10" through "2026-09-23"
    And the family local date and time is "2026-09-09T23:59:59"
    When the house catalog determines the item's New badge
    Then the New badge is hidden

  Scenario: The New badge follows its local calendar-date schedule [localDateTime=2026-09-10T00:00:00; visibility=visible]
    Given a house item has a New-badge schedule from "2026-09-10" through "2026-09-23"
    And the family local date and time is "2026-09-10T00:00:00"
    When the house catalog determines the item's New badge
    Then the New badge is visible

  Scenario: The New badge follows its local calendar-date schedule [localDateTime=2026-09-23T23:59:59; visibility=visible]
    Given a house item has a New-badge schedule from "2026-09-10" through "2026-09-23"
    And the family local date and time is "2026-09-23T23:59:59"
    When the house catalog determines the item's New badge
    Then the New badge is visible

  Scenario: The New badge follows its local calendar-date schedule [localDateTime=2026-09-24T00:00:00; visibility=hidden]
    Given a house item has a New-badge schedule from "2026-09-10" through "2026-09-23"
    And the family local date and time is "2026-09-24T00:00:00"
    When the house catalog determines the item's New badge
    Then the New badge is hidden

  Scenario: A house item without a New-badge schedule has no New badge
    Given a house item has no New-badge schedule
    When the house catalog determines the item's New badge
    Then the New badge is hidden
