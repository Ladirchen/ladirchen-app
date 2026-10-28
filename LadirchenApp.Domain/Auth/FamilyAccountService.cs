using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Auth;

public sealed class FamilyAccountService
{
  private readonly List<FamilyAccount> _accounts = new();

  public FamilySession? ActiveSession { get; private set; }

  public FamilyAccount Register(
    string familyName,
    string guardianName,
    string username,
    string password,
    string confirmation,
    DateTimeOffset now
  )
  {
    Rules.Require(Rules.Text(familyName).Length >= 2, DomainError.InvalidValue, "The family name is too short.");
    Rules.Require(Rules.Text(guardianName).Length >= 2, DomainError.InvalidValue, "The guardian name is too short.");
    Rules.Require(Rules.Text(username).Length >= 3, DomainError.InvalidValue, "The username is too short.");
    Rules.Require(password.Length >= 6, DomainError.InvalidValue, "The password is too short.");
    Rules.Require(password == confirmation, DomainError.InvalidValue, "The passwords do not match.");
    Rules.Require(_accounts.All(item => item.Username != username), DomainError.Duplicate, "The username is already registered.");
    var administratorId = new FamilyMemberId($"guardian-{_accounts.Count + 1}");
    var family = new FamilyAggregate(new FamilyId(Guid.NewGuid()), administratorId, guardianName, new IanaTimeZone("Europe/Zurich"));
    var account = new FamilyAccount(username, password, family, administratorId);
    _accounts.Add(account);
    ActiveSession = new FamilySession(family, administratorId);

    return account;
  }

  public FamilySession? SignIn(string username, string password)
  {
    var account = _accounts.Find(item => item.Username == username && item.Password == password);
    ActiveSession = account is null ? null : new FamilySession(account.Family, account.MemberId);

    return ActiveSession;
  }

  public void AddAccount(FamilyAccount account)
  {
    ArgumentNullException.ThrowIfNull(account);
    _accounts.Add(account);
  }

  public void SignOut()
  {
    ActiveSession = null;
  }
}
