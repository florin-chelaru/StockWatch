using System;

namespace StockWatchData.Models
{
  public interface IStockWatchDataContextFactory
  {
    StockWatchDataContext DataContext { get; }
  }
}
