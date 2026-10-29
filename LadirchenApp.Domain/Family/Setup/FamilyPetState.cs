using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Family.Setup;

public sealed record FamilyPetState
{
  public FamilyPetState(
    FamilyPetId id,
    string name,
    FamilyPetKind kind,
    string? avatar = null,
    string? color = null
  )
  {
    ArgumentNullException.ThrowIfNull(id);
    Id = id;
    Name = Rules.Text(name);
    Kind = Rules.Defined(kind);
    Avatar = NormalizeOptionalText(avatar);
    Color = NormalizeOptionalText(color);
  }

  public FamilyPetId Id { get; }
  public string Name { get; }
  public FamilyPetKind Kind { get; }
  public string? Avatar { get; }
  public string? Color { get; }

  private static string? NormalizeOptionalText(string? value)
  {
    var normalized = value?.Trim();

    return string.IsNullOrEmpty(normalized) ? null : normalized;
  }
}
