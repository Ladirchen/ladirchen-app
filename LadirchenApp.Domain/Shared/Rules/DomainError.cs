using System;

namespace LadirchenApp.Domain.Shared;

public enum DomainError
{
  Forbidden, NotFound, Duplicate, InvalidState, InvalidValue, InsufficientFunds, Closed, LastAdministrator
}