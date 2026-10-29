using System;

namespace LadirchenApp.Application.Commands;

public sealed class FamilyCommandException : Exception
{
  public FamilyCommandException(string message)
    : base(message)
  {
  }
}
