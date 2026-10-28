using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

[assembly: ExcludeFromCodeCoverage]

namespace LadirchenApp.Domain.Tests.Hooks;

public static class GlobalHooks
{
  [Before(TestSession)]
  public static Task BeforeTestSession(TestSessionContext context)
  {
    return Task.CompletedTask;
  }

  [After(TestSession)]
  public static Task AfterTestSession(TestSessionContext context)
  {
    return Task.CompletedTask;
  }
}
