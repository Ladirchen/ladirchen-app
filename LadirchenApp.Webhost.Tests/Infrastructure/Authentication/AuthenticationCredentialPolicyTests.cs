using System;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Infrastructure;

namespace LadirchenApp.Webhost.Tests.Infrastructure.Authentication;

public sealed class AuthenticationCredentialPolicyTests
{
  [Test]
  public async Task UsernamesCannotOverlapWithEmailIdentifiers()
  {
    await Assert.That(() => AuthenticationCredentialPolicy.NormalizeUsername(
      "member@example.com"
    )).Throws<ArgumentException>();
  }

  [Test]
  public async Task NewPasswordsRequireAtLeastTwelveCharacters()
  {
    await Assert.That(() => AuthenticationCredentialPolicy.ValidatePassword(
      "eleven-char"
    )).Throws<ArgumentException>();
    await Assert.That(() => AuthenticationCredentialPolicy.ValidatePassword(
      "twelve-chars"
    )).ThrowsNothing();
  }

  [Test]
  public async Task DummyPasswordHashAcceptsWhitespaceCredentials()
  {
    await Assert.That(() => AuthenticationCredentialPolicy.PerformDummyPasswordHash(
      "   "
    )).ThrowsNothing();
  }

  [Test]
  public async Task EmailAddressesCannotExceedThePersistenceLimit()
  {
    var oversizedEmail = $"{new string('a', AuthenticationSecurityPolicy.MaximumEmailLength)}@example.com";

    await Assert.That(() => AuthenticationCredentialPolicy.NormalizeEmail(
      oversizedEmail
    )).Throws<ArgumentException>();
  }

  [Test]
  public async Task MalformedEmailAddressesAreReportedAsPublicInputErrors()
  {
    await Assert.That(() => AuthenticationCredentialPolicy.NormalizeEmail(
      "member@invalid"
    )).Throws<ArgumentException>();
  }
}