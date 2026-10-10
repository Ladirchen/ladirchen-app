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

  Scenario Outline: Cancellation and rejection release reserved funds
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

  Scenario: Reserved funds cannot be deposited into goals
    Given a core economy
    And the child has 100 Ladirchen
    And the child creates a "Private" goal targeting 200 Ladirchen
    And a published reward costs 80 Ladirchen with stock 1
    And the child requests the reward
    When the child tries to deposit 30 Ladirchen
    Then the economy command fails with "InsufficientFunds"
    And the goal contains 5 Ladirchen
    And the rejected command left the economy unchanged

  Scenario: Reserved funds cannot be gifted
    Given a core economy
    And the child has 100 Ladirchen
    And a published reward costs 80 Ladirchen with stock 1
    And the child requests the reward
    When the child tries to gift 30 Ladirchen
    Then the economy command fails with "InsufficientFunds"
    And the sibling wallet contains 0 Ladirchen
    And the rejected command left the economy unchanged

  Scenario Outline: The family timezone controls shop hours
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
