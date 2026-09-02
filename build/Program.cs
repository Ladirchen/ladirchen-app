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
