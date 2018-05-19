namespace StockWatchData.Models
{
  public class SqlServerStockWatchDataContextFactory : IStockWatchDataContextFactory
  {
    private static readonly StockWatchDataContext SqlServerDataContext = new StockWatchDataContext();

    public StockWatchDataContext DataContext => SqlServerDataContext;
  }
}