namespace StockWatchData.Models
{
  public interface IStockWatchDataContextFactory
  {
    StockWatchDataContext CreateDataContext();
  }
}
