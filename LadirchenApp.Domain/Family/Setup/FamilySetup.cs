using System.Collections.Generic;
using System.Linq;

namespace LadirchenApp.Domain.Family.Setup;

public sealed class FamilySetup
{
  private readonly List<string> _children = new();
  private readonly List<string> _guardians = new();
  private readonly List<string> _pets = new();

  public IReadOnlyList<string> Children
  {
    get
    {
      return _children.AsReadOnly();
    }
  }

  public IReadOnlyList<string> Guardians
  {
    get
    {
      return _guardians.AsReadOnly();
    }
  }

  public IReadOnlyList<string> Pets
  {
    get
    {
      return _pets.AsReadOnly();
    }
  }

  public bool CanComplete
  {
    get
    {
      return _children.Any(item => !string.IsNullOrWhiteSpace(item))
        && _guardians.Any(item => !string.IsNullOrWhiteSpace(item));
    }
  }

  public void AddChild(string name)
  {
    _children.Add(name);
  }

  public void AddGuardian(string name)
  {
    _guardians.Add(name);
  }

  public void AddPet(string name)
  {
    _pets.Add(name);
  }
}
