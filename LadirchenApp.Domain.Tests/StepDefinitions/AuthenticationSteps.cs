using System;
using System.Threading.Tasks;

using LadirchenApp.Domain.Auth;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using Reqnroll;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

[Binding]
public sealed class AuthenticationSteps
{
  private readonly FamilyAccountService _accounts = new();
  private string _familyName = "Sun Family";
  private string _guardianName = "Alex";
  private string _username = "alex";
  private string _password = "secret";
  private string _confirmation = "secret";
  private FamilyAccount? _registeredAccount;

  [Given("the family name and guardian name contain at least {int} characters")]
  public void ValidNames(int minimum)
  {
    _familyName = new string('F', minimum);
    _guardianName = new string('G', minimum);
  }

  [Given("the username contains at least {int} characters")]
  public void ValidUsername(int minimum)
  {
    _username = new string('u', minimum);
  }

  [Given("the password contains at least {int} characters")]
  public void ValidPassword(int minimum)
  {
    _password = new string('p', minimum);
  }

  [Given("the password confirmation matches")]
  public void MatchingConfirmation()
  {
    _confirmation = _password;
  }

  [When("the visitor registers a family account")]
  public void Register()
  {
    _registeredAccount = _accounts.Register(
      _familyName,
      _guardianName,
      _username,
      _password,
      _confirmation,
      DateTimeOffset.UtcNow
    );
  }

  [Then("a new family is created")]
  public async Task FamilyCreated()
  {
    await Assert.That(_registeredAccount?.Family).IsNotNull();
  }

  [Then("the registering guardian is its first administrator")]
  public async Task FirstAdministrator()
  {
    await Assert.That(_registeredAccount?.Family.Members.Count).IsEqualTo(1);
    await Assert.That(_registeredAccount?.Family.Members[0].IsAdministrator).IsTrue();
  }

  [Then("that guardian is signed in")]
  public async Task GuardianSignedIn()
  {
    await Assert.That(_accounts.ActiveSession?.MemberId).IsEqualTo(_registeredAccount?.MemberId);
  }

  [Given("a local account belongs to a family member")]
  public void ExistingAccount()
  {
    var memberId = new FamilyMemberId("guardian");
    var family = new FamilyAggregate(new FamilyId(Guid.NewGuid()), memberId, "Alex", new IanaTimeZone("Europe/Zurich"));
    _registeredAccount = new FamilyAccount(_username, _password, family, memberId);
    _accounts.AddAccount(_registeredAccount);
  }

  [When("that member signs in with the correct username and password")]
  public void SignIn()
  {
    _accounts.SignIn(_username, _password);
  }

  [Then("the corresponding family is loaded")]
  public async Task FamilyLoaded()
  {
    await Assert.That(_accounts.ActiveSession?.Family).IsSameReferenceAs(_registeredAccount?.Family);
  }

  [Then("the corresponding member becomes the signed-in member")]
  public async Task MemberSignedIn()
  {
    await Assert.That(_accounts.ActiveSession?.MemberId).IsEqualTo(_registeredAccount?.MemberId);
  }

  [Given("no account matches the entered username and password")]
  public void InvalidCredentials()
  {
    _username = "missing";
    _password = "invalid";
  }

  [When("the visitor attempts to sign in")]
  public void AttemptSignIn()
  {
    _accounts.SignIn(_username, _password);
  }

  [Then("no family session is started")]
  public async Task NoSession()
  {
    await Assert.That(_accounts.ActiveSession).IsNull();
  }

  [Given("a family member is signed in")]
  public void SignedInMember()
  {
    ExistingAccount();
    SignIn();
  }

  [When("the member signs out")]
  public void SignOut()
  {
    _accounts.SignOut();
  }

  [Then("the active family session is cleared")]
  public async Task SessionCleared()
  {
    await Assert.That(_accounts.ActiveSession).IsNull();
  }
}
