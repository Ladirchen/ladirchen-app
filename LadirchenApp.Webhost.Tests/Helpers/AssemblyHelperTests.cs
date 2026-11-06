using System.Reflection;
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
}
