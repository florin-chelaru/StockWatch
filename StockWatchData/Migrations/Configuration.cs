using System.Data.Entity.Migrations;

namespace StockWatchData.Migrations
{
  internal sealed class
    Configuration : DbMigrationsConfiguration<StockWatchDataContext>
  {
    public Configuration()
    {
      AutomaticMigrationsEnabled = false;
    }

    protected override void Seed(StockWatchDataContext context)
    {
      //  This method will be called after migrating to the latest version.

      //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
      //  to avoid creating duplicate seed data.
    }
  }
}