Feature: Family reward shop
  Family members with content-management permission create and maintain family rewards.
  Children may reserve published family rewards before the local shop cutoff.
  Approval captures the reserved price, while cancellation, rejection or deletion releases the reservation.

  Scenario: A request reserves funds without debiting them
    Given a core economy
    And the child has 100 Ladirchen
    And a published reward costs 80 Ladirchen with stock 2
    When the child requests the reward
    Then the child wallet contains 100 Ladirchen
    And the wallet has 80 reserved and 20 available Ladirchen

  Scenario: Approval captures the price and decrements stock once
    Given a core economy
    And the child has 100 Ladirchen
    And a published reward costs 80 Ladirchen with stock 2
    And the child requests the reward
    When the guardian administrator approves the reward
    And the guardian administrator tries to approve the reward again
    Then the child wallet contains 20 Ladirchen
    And the wallet has 0 reserved and 20 available Ladirchen
    And the reward has stock 1 and status "Available"
    And the economy command fails with "InvalidState"
    And the rejected command left the economy unchanged

  Scenario: Cancellation, rejection and deletion release reserved funds [decision=cancelled]
    Given a core economy
    And the child has 100 Ladirchen
    And a published reward costs 80 Ladirchen with stock 1
    And the child requests the reward
    When the request is "cancelled"
    Then the wallet has 0 reserved and 100 available Ladirchen
    And the reward has stock 1 and status "Available"

  Scenario: Cancellation, rejection and deletion release reserved funds [decision=rejected]
    Given a core economy
    And the child has 100 Ladirchen
    And a published reward costs 80 Ladirchen with stock 1
    And the child requests the reward
    When the request is "rejected"
    Then the wallet has 0 reserved and 100 available Ladirchen
    And the reward has stock 1 and status "Available"

  Scenario: Cancellation, rejection and deletion release reserved funds [decision=deleted]
    Given a core economy
    And the child has 100 Ladirchen
    And a published reward costs 80 Ladirchen with stock 1
    And the child requests the reward
    When the request is "deleted"
    Then the wallet has 0 reserved and 100 available Ladirchen
    And the reward has stock 1 and status "Available"

  Scenario: The family timezone controls shop hours [time=2026-10-10T17:59:00; reserved=80; available=20]
    Given a core economy
    And the child has 100 Ladirchen
    And a published reward costs 80 Ladirchen with stock 1
    And the local time is "2026-10-10T17:59:00"
    When the child tries to request the reward
    Then the wallet has 80 reserved and 20 available Ladirchen

  Scenario: The family timezone controls shop hours [time=2026-10-10T18:00:00; reserved=0; available=100]
    Given a core economy
    And the child has 100 Ladirchen
    And a published reward costs 80 Ladirchen with stock 1
    And the local time is "2026-10-10T18:00:00"
    When the child tries to request the reward
    Then the wallet has 0 reserved and 100 available Ladirchen

  Scenario: Another child cannot cancel a reservation
    Given a core economy
    And the child has 100 Ladirchen
    And a published reward costs 80 Ladirchen with stock 1
    And the child requests the reward
    When the sibling tries to cancel the request
    Then the economy command fails with "Forbidden"
    And the rejected command left the economy unchanged

  Scenario: Pending offers cannot change their price
    Given a core economy
    And the child has 100 Ladirchen
    And a published reward costs 80 Ladirchen with stock 1
    And the child requests the reward
    When the guardian administrator tries to change the price
    Then the economy command fails with "InvalidState"
    And the rejected command left the economy unchanged

  Scenario: Only family members who can manage content can change the reward catalog
    Given a family member cannot manage content
    When the member attempts to add, hide or delete a shop reward
    Then the reward catalog remains unchanged

  Scenario: A reward must be visible and within its publication dates [visibility=hidden; availability=current date; result=rejected]
    Given a reward has visibility "hidden" and availability "current date"
    When the child attempts to request the reward
    Then the request is rejected

  Scenario: A reward must be visible and within its publication dates [visibility=visible; availability=before start date; result=rejected]
    Given a reward has visibility "visible" and availability "before start date"
    When the child attempts to request the reward
    Then the request is rejected

  Scenario: A reward must be visible and within its publication dates [visibility=visible; availability=after end date; result=rejected]
    Given a reward has visibility "visible" and availability "after end date"
    When the child attempts to request the reward
    Then the request is rejected

  Scenario: A reward must be visible and within its publication dates [visibility=visible; availability=on start date; result=accepted]
    Given a reward has visibility "visible" and availability "on start date"
    When the child attempts to request the reward
    Then the request is accepted

  Scenario: A reward must be visible and within its publication dates [visibility=visible; availability=on end date; result=accepted]
    Given a reward has visibility "visible" and availability "on end date"
    When the child attempts to request the reward
    Then the request is accepted

  Scenario: A reward request requires stock and sufficient available funds [stock=0; price=10; available=100; result=rejected]
    Given an available reward has stock 0 and costs 10 Ladirchen
    And the child has 100 available Ladirchen
    When the child attempts to request the reward
    Then the request is rejected

  Scenario: A reward request requires stock and sufficient available funds [stock=1; price=80; available=20; result=rejected]
    Given an available reward has stock 1 and costs 80 Ladirchen
    And the child has 20 available Ladirchen
    When the child attempts to request the reward
    Then the request is rejected

  Scenario: A reward request requires stock and sufficient available funds [stock=1; price=80; available=80; result=accepted]
    Given an available reward has stock 1 and costs 80 Ladirchen
    And the child has 80 available Ladirchen
    When the child attempts to request the reward
    Then the request is accepted

  Scenario: Approval is rejected after the local shop cutoff
    Given a child has a pending reward request
    And the family local time is 18:00
    When a guardian administrator attempts to approve the request
    Then the request and wallet remain unchanged

  Scenario: Only children can request family rewards
    Given a published reward is available and affordable
    When a guardian attempts to request the reward
    Then no reward request is created
    And no funds are reserved

  Scenario: Redeeming the final item exhausts its stock
    Given a published reward has stock 1
    And a child has a pending request for that reward
    When a family member who can manage content approves the request before the local cutoff
    Then the reward has stock 0 and status "Redeemed"
    And the reward cannot be requested again

  Scenario: Family reward shop access follows the signed-in family role [role=child; access=allowed]
    Given the signed-in family member is a "child"
    When the family member opens the family reward shop
    Then access to the family reward shop is allowed

  Scenario: Family reward shop access follows the signed-in family role [role=guardian administrator; access=allowed]
    Given the signed-in family member is a "guardian administrator"
    When the family member opens the family reward shop
    Then access to the family reward shop is allowed

  Scenario: Family reward shop access follows the signed-in family role [role=guardian supporter; access=denied]
    Given the signed-in family member is a "guardian supporter"
    When the family member opens the family reward shop
    Then access to the family reward shop is denied

  Scenario: Children see only currently published rewards
    Given the catalog contains visible, hidden, scheduled and expired rewards
    When a child views the family reward shop
    Then only currently published rewards are displayed

  Scenario: Family members who can manage content see every catalog reward
    Given the catalog contains visible, hidden, scheduled and expired rewards
    When a family member who can manage content views the family reward shop
    Then all catalog rewards are displayed with their publication status

  Scenario: Hiding a reward preserves its pending request
    Given a child has a pending reward request with reserved funds
    When a family member who can manage content hides the reward
    Then the pending request remains unchanged
    And the funds remain reserved until approval, rejection, cancellation or deletion

  Scenario: A new reward requires complete valid catalog data [invalidData=a blank icon]
    Given a family member who can manage content enters reward data with "a blank icon"
    When the family member who can manage content attempts to add the reward
    Then the reward is not added to the catalog

  Scenario: A new reward requires complete valid catalog data [invalidData=a blank title]
    Given a family member who can manage content enters reward data with "a blank title"
    When the family member who can manage content attempts to add the reward
    Then the reward is not added to the catalog

  Scenario: A new reward requires complete valid catalog data [invalidData=a blank description]
    Given a family member who can manage content enters reward data with "a blank description"
    When the family member who can manage content attempts to add the reward
    Then the reward is not added to the catalog

  Scenario: A new reward requires complete valid catalog data [invalidData=blank conditions]
    Given a family member who can manage content enters reward data with "blank conditions"
    When the family member who can manage content attempts to add the reward
    Then the reward is not added to the catalog

  Scenario: A new reward requires complete valid catalog data [invalidData=a price below 1 Ladirchen]
    Given a family member who can manage content enters reward data with "a price below 1 Ladirchen"
    When the family member who can manage content attempts to add the reward
    Then the reward is not added to the catalog

  Scenario: A new reward requires complete valid catalog data [invalidData=a quantity below 1]
    Given a family member who can manage content enters reward data with "a quantity below 1"
    When the family member who can manage content attempts to add the reward
    Then the reward is not added to the catalog

  Scenario: A new reward requires complete valid catalog data [invalidData=a scheduled publication without a start date]
    Given a family member who can manage content enters reward data with "a scheduled publication without a start date"
    When the family member who can manage content attempts to add the reward
    Then the reward is not added to the catalog

  Scenario: A new reward requires complete valid catalog data [invalidData=a limited publication without an end date]
    Given a family member who can manage content enters reward data with "a limited publication without an end date"
    When the family member who can manage content attempts to add the reward
    Then the reward is not added to the catalog

  Scenario: A new reward requires complete valid catalog data [invalidData=a start date after the end date]
    Given a family member who can manage content enters reward data with "a start date after the end date"
    When the family member who can manage content attempts to add the reward
    Then the reward is not added to the catalog

  Scenario: A reward belongs to one supported category [category=time]
    Given a family member who can manage content creates a valid reward in category "time"
    When the reward is added to the catalog
    Then its category is "time"

  Scenario: A reward belongs to one supported category [category=activity]
    Given a family member who can manage content creates a valid reward in category "activity"
    When the reward is added to the catalog
    Then its category is "activity"

  Scenario: A reward belongs to one supported category [category=allowance]
    Given a family member who can manage content creates a valid reward in category "allowance"
    When the reward is added to the catalog
    Then its category is "allowance"

  Scenario: A reward belongs to one supported category [category=gift]
    Given a family member who can manage content creates a valid reward in category "gift"
    When the reward is added to the catalog
    Then its category is "gift"

  Scenario: A reward belongs to one supported category [category=privilege]
    Given a family member who can manage content creates a valid reward in category "privilege"
    When the reward is added to the catalog
    Then its category is "privilege"

  Scenario: A reward belongs to one supported category [category=custom]
    Given a family member who can manage content creates a valid reward in category "custom"
    When the reward is added to the catalog
    Then its category is "custom"
