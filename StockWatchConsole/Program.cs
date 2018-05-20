using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using StockWatchData.Models;
using TensorFlow;

namespace StockWatchConsole
{
  class Program
  {
    static void Main(string[] args)
    {
      //      var watcher = new StockWatch.StockWatch(@"c:\Documents\work\stock-prediction\train aapl amzn baba fb goog msft nflx tsla yhoo znga ebay intc gpro".Split(' '));
      //var watcher = new StockWatch.StockWatch(@"c:\Documents\work\stock-prediction\train aapl amzn".Split(' '));

      //      watcher.StartStub();

      //      Console.WriteLine(TFCore.Version);
      //      //      Console.WriteLine(TFCore.);
      //      var session = new TFSession();
      //      session.
      var dbFactory = new SqlServerStockWatchDataContextFactory();
      WindowExtractor extractor = new WindowExtractor(dbFactory);
      var db = dbFactory.DataContext;
//      var windows = extractor.ExtractAllWindows(20, 50);
      var nasdaq = db.Symbols.Where(s => s.Tags.Contains("{nasdaq}")).Select(s => s.Id).ToList();
//      var symbol = "amzn";
      var symbol = "nasdaq";
//      var windows = extractor.ExtractAllWindows(new[] {symbol}, 20, 50);
      var windows = extractor.ExtractAllWindows(nasdaq, 20, 50);

      Console.WriteLine($"{windows.Count} windows:\n");
//      windows.Sort((w1, w2) => w1.Bucket - w2.Bucket);
//      foreach (var window in windows)
//      {
//        var pastValues = string.Join(",", window.PastValues.Select(v => $"{decimal.Round(v * 100m, 3)}%"));
//        Console.WriteLine($"{window.Symbol},{window.DayOne},{window.BucketLabel},{pastValues}");
//      }

      Console.WriteLine($"{symbol}");
      foreach (var bucket in Window.BucketLabels)
      {
        var c = windows.Count(w => w.MaxBucket == bucket.Key);
        Console.WriteLine($"{decimal.Round((decimal) c / windows.Count * 100m, 3)}%");
      }

      Console.WriteLine($"\n{symbol}");
      foreach (var bucket in Window.BucketLabels)
      {
        var c = windows.Count(w => w.MinBucket == bucket.Key);
        Console.WriteLine($"{decimal.Round((decimal) c / windows.Count * 100m, 3)}%");
      }

      Console.WriteLine($"\n{symbol}");
      foreach (var bucket in Window.BucketLabels)
      {
        var c = windows.Count(w => w.MedianBucket == bucket.Key);
        Console.WriteLine($"{decimal.Round((decimal) c / windows.Count * 100m, 3)}%");
      }
    }
  }
}