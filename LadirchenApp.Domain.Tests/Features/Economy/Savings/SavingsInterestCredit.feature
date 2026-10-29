Feature: Savings interest credit
  Interest credit is awarded only to an eligible savings balance.
  It preserves the minimum credit without exceeding the savings target.

  Scenario: Credits preserve the minimum and target cap [saved=0; target=100; rate=8; credit=0]
    When interest on 0 saved toward 100 at 8 percent is calculated
    Then the interest credit is 0 Ladirchen

  Scenario: Credits preserve the minimum and target cap [saved=100; target=100; rate=8; credit=0]
    When interest on 100 saved toward 100 at 8 percent is calculated
    Then the interest credit is 0 Ladirchen

  Scenario: Credits preserve the minimum and target cap [saved=10; target=100; rate=0; credit=1]
    When interest on 10 saved toward 100 at 0 percent is calculated
    Then the interest credit is 1 Ladirchen

  Scenario: Credits preserve the minimum and target cap [saved=30; target=100; rate=5; credit=2]
    When interest on 30 saved toward 100 at 5 percent is calculated
    Then the interest credit is 2 Ladirchen

  Scenario: Credits preserve the minimum and target cap [saved=99.5; target=100; rate=8; credit=0.5]
    When interest on 99.5 saved toward 100 at 8 percent is calculated
    Then the interest credit is 0.5 Ladirchen
