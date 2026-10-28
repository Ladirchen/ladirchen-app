using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed class PersonalStreakPolicy
{
  public int Calculate(int completedWeeklyStreaks, int weeklyTargetDays, int approvedCurrentWeekDays)
  {
    Rules.Require(
      completedWeeklyStreaks >= 0 && weeklyTargetDays > 0 && approvedCurrentWeekDays >= 0,
      DomainError.InvalidValue,
      "Streak inputs are invalid."
    );

    return completedWeeklyStreaks * weeklyTargetDays + approvedCurrentWeekDays;
  }
}
