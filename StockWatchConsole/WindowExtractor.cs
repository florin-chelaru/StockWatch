using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockWatchData.Models;

namespace StockWatchConsole
{
  public class WindowExtractor
  {
    private readonly IStockWatchDataContextFactory dataContextFactory;

    public WindowExtractor(IStockWatchDataContextFactory dataContextFactory)
    {
      this.dataContextFactory = dataContextFactory;
    }

    public List<Window> Extract(string symbol, int pastSize, int futureSize)
    {
      var db = dataContextFactory.DataContext;

      var quotes = db.DailyQuotes.Where(q => q.Symbol == symbol).ToList();
      quotes.Sort((q1, q2) => string.CompareOrdinal(q1.Day, q2.Day));

      List<Window> windows = new List<Window>();
      for (int i = 0; i < quotes.Count - pastSize - futureSize; i += pastSize)
      {
        DailyQuote[] past = new DailyQuote[pastSize];
        DailyQuote[] future = new DailyQuote[futureSize];
        quotes.CopyTo(i, past, 0, pastSize);
        quotes.CopyTo(i+pastSize, future, 0, futureSize);

        DailyQuote dayOne = past[past.Length - 1];

        var window = new Window
        {
          Symbol = symbol,
          PastSize = pastSize,
          FutureSize = futureSize,
          DayOne = dayOne.Day,
          PastDays = (from q in past select q.Day).ToArray(),
          PastValues = (from q in past select q.Close / dayOne.Close - 1m).ToArray(),
          FutureDays =  (from q in future select q.Day).ToArray(),
          FutureValues = (from q in future select q.Close / dayOne.Close - 1m).ToArray()
        };
        windows.Add(window);
      }

      return windows;
    }

    public List<Window> ExtractAllWindows(ICollection<string> symbols, int pastSize, int futureSize)
    {
      List<Window> allWindows = new List<Window>();
      int i = 0;
      foreach (var symbol in symbols)
      {
        Console.WriteLine($"Computing windows for {i++}th symbol: {symbol}");
        var windows = Extract(symbol, pastSize, futureSize);
        allWindows.AddRange(windows);
      }

      return allWindows;
    }

    public IList<Window> ExtractAllWindows(int pastSize, int futureSize)
    {
      var db = dataContextFactory.DataContext;

      var symbols = db.Symbols.Select(s => s.Id).ToList();
      return ExtractAllWindows(symbols, pastSize, futureSize);
    }
  }
}
