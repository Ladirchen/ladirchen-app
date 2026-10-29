Feature: House appearance from family energy
  Family energy changes the atmosphere around the house without changing its stored progression level.
  The selected house theme and stored house level determine the house structure.

  Scenario: Family energy determines the house atmosphere [energy=0; atmosphere=rainy]
    Given family energy is 0 percent
    When the family house is displayed
    Then the house atmosphere is "rainy"

  Scenario: Family energy determines the house atmosphere [energy=39; atmosphere=rainy]
    Given family energy is 39 percent
    When the family house is displayed
    Then the house atmosphere is "rainy"

  Scenario: Family energy determines the house atmosphere [energy=40; atmosphere=cloudy]
    Given family energy is 40 percent
    When the family house is displayed
    Then the house atmosphere is "cloudy"

  Scenario: Family energy determines the house atmosphere [energy=69; atmosphere=cloudy]
    Given family energy is 69 percent
    When the family house is displayed
    Then the house atmosphere is "cloudy"

  Scenario: Family energy determines the house atmosphere [energy=70; atmosphere=sunny]
    Given family energy is 70 percent
    When the family house is displayed
    Then the house atmosphere is "sunny"

  Scenario: Family energy determines the house atmosphere [energy=100; atmosphere=sunny]
    Given family energy is 100 percent
    When the family house is displayed
    Then the house atmosphere is "sunny"

  Scenario: Changing family energy preserves the selected house structure
    Given the house has a selected theme and a stored progression level
    When family energy changes
    Then the energy-dependent appearance is updated
    But the selected theme and stored progression level remain unchanged
