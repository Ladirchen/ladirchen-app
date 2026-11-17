using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Helpers;

namespace LadirchenApp.Webhost.Tests.Helpers;

public sealed class AssemblyHelperTests
{
  [Test]
  public async Task AboutInfoUsesTheSpecifiedAssemblyInformationalVersion()
  {
    var assembly = typeof(AssemblyHelper).Assembly;
    var expected = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

    var info = AssemblyHelper.GetAboutInfoFromAssembly(assembly);

    await Assert.That(info.Name).IsEqualTo("LadirchenApp.Webhost");
    await Assert.That(info.InformationalVersion).IsEqualTo(expected);
    await Assert.That(info.SemVer).IsEqualTo(expected.Split('+')[0]);
  }

  [Test]
  public async Task InformationalVersionComesFromTheRequestedAssembly()
  {
    var version = AssemblyHelper.GetInformationalVersion(typeof(object).Assembly);
    var expected = typeof(object).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    await Assert.That(version).IsEqualTo(expected);
  }

  [Test]
  public async Task AboutInfoExtractsGitBranchAndCommitFromTheEmbeddedVersion()
  {
    const string sha = "1234567890abcdef1234567890abcdef12345678";
    const string version = "1.2.3-beta.4";
    var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("VersionedWebhost"), AssemblyBuilderAccess.Run);
    var constructor = typeof(AssemblyInformationalVersionAttribute).GetConstructor([typeof(string)])!;
    assembly.SetCustomAttribute(new CustomAttributeBuilder(constructor, [$"{version}+Branch.release/1.2.Sha.{sha}"]));

    var info = AssemblyHelper.GetAboutInfoFromAssembly(assembly);

    await Assert.That(info.SemVer).IsEqualTo(version);
    await Assert.That(info.Branch).IsEqualTo("release/1.2");
    await Assert.That(info.Sha).IsEqualTo(sha);
  }
}
