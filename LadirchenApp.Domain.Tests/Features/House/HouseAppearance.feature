Feature: House appearance from family energy
  Family energy changes the atmosphere around the house without changing its stored progression level.
  The selected house theme and stored house level determine the house structure.

  Scenario: Family energy determines the house atmosphere
    Given family energy is <energy> percent
    When the family house is displayed
    Then the house atmosphere is "<atmosphere>"
    Examples:
      | energy | atmosphere |
      | 0      | rainy      |
      | 39     | rainy      |
      | 40     | cloudy     |
      | 69     | cloudy     |
      | 70     | sunny      |
      | 100    | sunny      |

  Scenario: Changing family energy preserves the selected house structure
    Given the house has a selected theme and a stored progression level
    When family energy changes
    Then the energy-dependent appearance is updated
    But the selected theme and stored progression level remain unchanged
