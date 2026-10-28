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

  Scenario: An accessory purchase is rejected when it is not allowed
    Given a house accessory is "<accessoryState>"
    And the viewer is "<viewer>"
    And the available balance is "<balanceState>"
    When the viewer attempts to purchase the accessory
    Then ownership and balance remain unchanged
    Examples:
      | accessoryState | viewer   | balanceState |
      | unowned        | guardian | sufficient   |
      | owned          | child    | sufficient   |
      | unowned        | child    | insufficient |

  Scenario: Purchasing a furniture set grants every accessory in the set
    Given a child can afford an unowned furniture set
    And the house meets the set's minimum level
    When the child purchases the furniture set
    Then the set price is deducted from the child's wallet
    And every accessory in the set is owned
    And every accessory in the set is stored unequipped

  Scenario: A furniture set purchase respects level, ownership and available balance
    Given a furniture set is "<ownership>"
    And its minimum house level is "<levelState>"
    And the child's available balance is "<balanceState>"
    When the child attempts to purchase the furniture set
    Then the purchase is <result>
    Examples:
      | ownership | levelState | balanceState | result   |
      | unowned   | met        | sufficient   | accepted |
      | unowned   | locked     | sufficient   | rejected |
      | owned     | met        | sufficient   | rejected |
      | unowned   | met        | insufficient | rejected |

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

  Scenario: A room design cannot be purchased without its requirements
    Given the room design purchase requirement "<missingRequirement>" is not met
    When the child attempts to purchase the room design
    Then ownership, selection and balance remain unchanged
    Examples:
      | missingRequirement |
      | minimum house level |
      | owned house theme   |
      | available balance   |

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

  Scenario: Indoor and outdoor furniture remain in compatible areas
    Given an owned stored furniture item is intended for "<placement>" use
    When a family member requests placement in "<requestedArea>"
    Then the furniture is placed in "<actualArea>"
    Examples:
      | placement | requestedArea | actualArea        |
      | outside   | living room   | garden            |
      | inside    | garden        | its assigned room |

  Scenario: A family member can restore a movable entity's initial placement
    Given a family member has moved a movable house entity
    When a family member resets the entity's placement
    Then its initial area, position and scale are restored

  Scenario: Owning every accessory in a furniture set means owning the set
    Given the family owns every accessory belonging to a furniture set
    When ownership of the furniture set is determined
    Then the furniture set is owned

  Scenario: The house catalog separates regular and special house items
    Given a house accessory belongs to the "<category>" category
    When a family member views the house shop
    Then the house item is shown in the "<catalogArea>" catalog area
    Examples:
      | category  | catalogArea |
      | furniture | inside      |
      | garden    | outside     |
      | special   | special     |

  Scenario: Moved house-item coordinates are rounded and bounded
    Given an owned movable house item can be arranged in an unlocked area
    When a family member moves it to x <requestedX> and y <requestedY>
    Then its stored x coordinate is <storedX>
    And its stored y coordinate is <storedY>
    Examples:
      | requestedX | requestedY | storedX | storedY |
      | 40.126     | 70.124     | 40.13   | 70.12   |
      | -10        | 2          | 4       | 8       |
      | 120        | 110        | 96      | 94      |

  Scenario: The New badge follows its local calendar-date schedule
    Given a house item has a New-badge schedule from "2026-09-10" through "2026-09-23"
    And the family local date and time is "<localDateTime>"
    When the house catalog determines the item's New badge
    Then the New badge is <visibility>
    Examples:
      | localDateTime       | visibility |
      | 2026-09-09T23:59:59 | hidden     |
      | 2026-09-10T00:00:00 | visible    |
      | 2026-09-23T23:59:59 | visible    |
      | 2026-09-24T00:00:00 | hidden     |

  Scenario: A house item without a New-badge schedule has no New badge
    Given a house item has no New-badge schedule
    When the house catalog determines the item's New badge
    Then the New badge is hidden
