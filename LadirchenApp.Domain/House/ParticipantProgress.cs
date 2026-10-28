namespace LadirchenApp.Domain.House;

public sealed class ParticipantProgress
{
  public ParticipantProgress(bool participates, int storedWeeklyStreak)
  {
    Participates = participates;
    StoredWeeklyStreak = storedWeeklyStreak;
  }

  public bool Participates { get; }
  public int StoredWeeklyStreak { get; set; }
}
