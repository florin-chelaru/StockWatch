namespace StockWatchData.Models
{
  public class SqlServerStockWatchDataContextFactory : IStockWatchDataContextFactory
  {
    public StockWatchDataContext CreateDataContext()
    {
      return new StockWatchDataContext();
    }
  }
}