using System;
using System.Text.RegularExpressions;

namespace LadirchenApp.Domain.Shared;

public sealed record TransactionId : DomainIdentifier
{
  public TransactionId(string value) : base(value) { }
}