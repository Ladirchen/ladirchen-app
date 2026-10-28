using Build.Tasks;

using Cake.Frosting;

namespace Build;

public static class Program
{
  public static int Main(string[] args)
  {
    return new CakeHost().UseContext<BuildContext>().Run(args);
  }
}

[TaskName("build-all")]
[IsDependentOn(typeof(BuildWebhostTask))]
[IsDependentOn(typeof(BuildDockerImageTask))]
public class BuildAllTask : FrostingTask;

[TaskName("test-all")]
[IsDependentOn(typeof(TestDomainTask))]
[IsDependentOn(typeof(TestVitestTask))]
public class TestAllTask : FrostingTask;

[TaskName("test-n-build-all")]
[IsDependentOn(typeof(TestAllTask))]
[IsDependentOn(typeof(BuildAllTask))]
public class TestNBuildAllTask : FrostingTask;
