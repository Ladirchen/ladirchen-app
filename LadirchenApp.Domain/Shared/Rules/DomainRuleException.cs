using System;

namespace LadirchenApp.Domain.Shared;

public sealed class DomainRuleException : Exception
{
  public DomainRuleException(DomainError code, string message) : base(message)
  {
    Code = code;
  }

  public DomainError Code
  {
    get;
  }
}