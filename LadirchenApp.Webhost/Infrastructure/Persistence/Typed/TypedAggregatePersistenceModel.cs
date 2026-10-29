using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

internal static partial class TypedAggregatePersistenceModel
{
  public static void Configure(ModelBuilder modelBuilder)
  {
    ConfigureFamily(modelBuilder);
    ConfigureFamilyEconomy(modelBuilder);
    ConfigureFamilyProgression(modelBuilder);
    ConfigureHouseCustomization(modelBuilder);
  }
}
