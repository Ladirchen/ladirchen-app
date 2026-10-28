Feature: Savings-goal vouchers and family consent
  Reaching a savings target converts the completed goal into a voucher for the full saved amount.
  The starter bonus becomes part of the voucher only after the target has been reached.
  A personal voucher belongs to its goal owner.
  A family voucher requires the consent of every family member participating in that goal.

  Scenario: Reaching a personal savings target creates a personal voucher
    Given a personal savings goal is one Ladirchen below its target
    When the owner deposits the final Ladirchen
    Then the savings goal is completed
    And a voucher for the full goal balance is issued to the owner
    And the personal starter bonus is included in the voucher

  Scenario: An incomplete savings goal cannot create a voucher
    Given a personal or family savings goal has not reached its target
    When a family member attempts to create its voucher
    Then no voucher is created
    And the starter bonus remains locked

  Scenario: A family goal defines who participates
    Given the family has children and guardians
    When a family savings goal is created for "<participation>"
    Then the goal participants are "<participants>"
    Examples:
      | participation             | participants                         |
      | the entire family         | all active children and guardians    |
      | children only             | all active children                   |
      | selected family members   | only the selected children and guardians |

  Scenario: Selected family-goal participants may combine children and guardians
    Given a family savings goal uses selected family members
    When one child and one guardian are selected
    Then both are participants in the family goal
    And every non-selected family member is not a participant

  Scenario: A completed family goal creates a voucher awaiting unanimous participant consent
    Given a family savings goal has reached its target
    And the participating members are one child, one sibling and one guardian
    When the family voucher is created
    Then the voucher contains the full goal balance including the family starter bonus
    And the voucher is awaiting consent from all three participants
    And the voucher cannot yet be redeemed

  Scenario: Every participant must consent before a family voucher can be redeemed
    Given a completed family voucher has three participating family members
    And two participants have consented
    When the final participant consents
    Then the family voucher becomes redeemable

  Scenario: Missing participant consent prevents family-voucher redemption
    Given a completed family voucher has three participating family members
    And only two participants have consented
    When redemption is attempted
    Then the redemption is rejected
    And the voucher remains unredeemed

  Scenario: A non-participant cannot consent for a family voucher
    Given a completed family voucher belongs to selected family-goal participants
    When a non-participating family member attempts to consent
    Then the recorded participant consents remain unchanged

  Scenario: A savings voucher can be redeemed only once
    Given every required participant has consented to a completed savings voucher
    When the voucher is redeemed
    And redemption is attempted again
    Then the second redemption is rejected
    And the voucher records exactly one redemption
