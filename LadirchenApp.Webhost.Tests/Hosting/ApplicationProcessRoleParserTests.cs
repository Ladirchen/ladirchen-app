using System;
using System.Threading.Tasks;

namespace LadirchenApp.Webhost.Tests.Hosting;

public sealed class ApplicationProcessRoleParserTests
{
  [Test]
  public async Task NoRoleArgumentSelectsWebProcess()
  {
    var role = ApplicationProcessRoleParser.Parse(["--urls", "http://localhost:5000"]);

    await Assert.That(role).IsEqualTo(ApplicationProcessRole.Web);
  }

  [Test]
  public async Task DaemonArgumentSelectsDaemonAndIsNotForwardedToHostConfiguration()
  {
    string[] arguments = ["--daemon", "--environment", "Production"];

    var role = ApplicationProcessRoleParser.Parse(arguments);
    var hostArguments = ApplicationProcessRoleParser.RemoveRoleArguments(arguments);

    await Assert.That(role).IsEqualTo(ApplicationProcessRole.Daemon);
    await Assert.That(hostArguments).IsEquivalentTo(["--environment", "Production"]);
  }

  [Test]
  public async Task MigrationArgumentSelectsOneShotMigrationProcess()
  {
    var role = ApplicationProcessRoleParser.Parse(["--migrate"]);

    await Assert.That(role).IsEqualTo(ApplicationProcessRole.Migration);
  }

  [Test]
  public async Task ConflictingProcessRolesAreRejected()
  {
    await Assert.That(() =>
      ApplicationProcessRoleParser.Parse(["--daemon", "--migrate"])
    ).Throws<ArgumentException>();
  }
}
