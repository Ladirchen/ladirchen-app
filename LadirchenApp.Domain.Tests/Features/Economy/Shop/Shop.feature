Feature: Shop reservations and redemption
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
    When the administrator approves the reward
    And the administrator tries to approve the reward again
    Then the child wallet contains 20 Ladirchen
    And the wallet has 0 reserved and 20 available Ladirchen
    And the reward has stock 1 and status "Available"
    And the economy command fails with "InvalidState"
    And the rejected command left the economy unchanged

  Scenario: Cancellation and rejection release reserved funds
    Given a core economy
    And the child has 100 Ladirchen
    And a published reward costs 80 Ladirchen with stock 1
    And the child requests the reward
    When the request is "<decision>"
    Then the wallet has 0 reserved and 100 available Ladirchen
    And the reward has stock 1 and status "Available"
    Examples:
      | decision |
      | cancelled |
      | rejected  |
      | deleted   |

  Scenario: The family timezone controls shop hours
    Given a core economy
    And the child has 100 Ladirchen
    And a published reward costs 80 Ladirchen with stock 1
    And the local time is "<time>"
    When the child tries to request the reward
    Then the wallet has <reserved> reserved and <available> available Ladirchen
    Examples:
      | time                | reserved | available |
      | 2026-10-10T17:59:00  | 80       | 20        |
      | 2026-10-10T18:00:00  | 0        | 100       |

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
    When the administrator tries to change the price
    Then the economy command fails with "InvalidState"
    And the rejected command left the economy unchanged

  Scenario: Only content managers can change the reward catalog
    Given a family member cannot manage content
    When the member attempts to add, hide or delete a shop reward
    Then the reward catalog remains unchanged

  Scenario: A reward must be visible and within its publication dates
    Given a reward has visibility "<visibility>" and availability "<availability>"
    When the child attempts to request the reward
    Then the request is <result>
    Examples:
      | visibility | availability        | result   |
      | hidden     | current date        | rejected |
      | visible    | before start date   | rejected |
      | visible    | after end date      | rejected |
      | visible    | on start date       | accepted |
      | visible    | on end date         | accepted |

  Scenario: A reward request requires stock and sufficient available funds
    Given an available reward has stock <stock> and costs <price> Ladirchen
    And the child has <available> available Ladirchen
    When the child attempts to request the reward
    Then the request is <result>
    Examples:
      | stock | price | available | result   |
      | 0     | 10    | 100       | rejected |
      | 1     | 80    | 20        | rejected |
      | 1     | 80    | 80        | accepted |

  Scenario: Approval is rejected after the local shop cutoff
    Given a child has a pending reward request
    And the family local time is 18:00
    When an administrator attempts to approve the request
    Then the request and wallet remain unchanged
