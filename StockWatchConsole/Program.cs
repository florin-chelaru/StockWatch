using System;
using StockWatchData;

namespace StockWatchConsole
{
  class Program
  {
    static void Main(string[] args)
    {
      //      var watcher = new StockWatch.StockWatch(@"c:\Documents\work\stock-prediction\train aapl amzn baba fb goog msft nflx tsla yhoo znga ebay intc gpro".Split(' '));
      //var watcher = new StockWatch.StockWatch(@"c:\Documents\work\stock-prediction\train aapl amzn".Split(' '));

      //      watcher.StartStub();

      // Do some testing

      //      Thread.Sleep(1000 * 60 * 60 * 4);

      //      watcher.StopStub();
      //      StockWatch.Data.Bla x;
      //      StockWatchData.Entities entities;
      //      entities.
//      StockWatchDb db;
      using (var db = new StockWatchDataContext())
      {
        db.StockQuoteIntervals.Add(new StockQuoteInterval
        {
          Symbol = "testsymbol",
          Market = "testmarket",
          CollectionFunction = "TEST",
          Open = 10.0m,
          Close = 20.0m,
          High = 25.0m,
          Low = 5.0m,
          Volume = 100L,
          StartTime = DateTime.Parse("2018-04-28 9:00:00"),
          EndTime = DateTime.Parse("2018-04-28 16:00:00"),
          CreatedAt = DateTimeOffset.UtcNow,
          UpdatedAt = DateTimeOffset.UtcNow
        });
        db.SaveChanges();
      }
    }
  }
}
