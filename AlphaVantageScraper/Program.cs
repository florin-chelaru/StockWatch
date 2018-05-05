using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlphaVantageApi;
using Common;
using StockWatchData;

namespace AlphaVantageScraper
{
  class Program
  {
    static async Task WriteIfNewSymbol(TimeseriesDailyResponse response)
    {
      using (var db = new StockWatchDataContext())
      {
        var symbolObj =
          (from symbol in db.Symbols where symbol.Id == response.Symbol select symbol)
          .FirstOrDefault();
        if (symbolObj == null)
        {
          db.Symbols.Add(symbolObj = new Symbol {Id = response.Symbol});
        }

        DateTime maxDate = DateTime.MinValue;
        if (symbolObj.DailyQuotes?.Any() ?? false)
        {
          maxDate = symbolObj.DailyQuotes.Max(quote => quote.Date);
        }

        var quotes = (from tick in response.Timeseries
          where tick.Time > maxDate
          select new DailyQuote
          {
            Day = tick.Time.ToString("yyyy-MM-dd"),
            Date = tick.Time,
            Open = tick.Open,
            Close = tick.Close,
            High = tick.High,
            Low = tick.Low,
            Volume = (long) tick.Volume,
            CollectionFunction = "TIME_SERIES_DAILY",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
          }).ToList();
        quotes.Sort((q1, q2) => DateTime.Compare(q1.Date, q2.Date));

        for (int i = 1; i < quotes.Count; ++i)
        {
          var today = quotes[i];
          var yesterday = quotes[i - 1];

          today.OpenChangePercent = (today.Open / yesterday.Open) * 100m - 100m;
          today.CloseChangePercent = (today.Close / yesterday.Close) * 100m - 100m;
          today.HighChangePercent = (today.High / yesterday.High) * 100m - 100m;
          today.LowChangePercent = (today.Low / yesterday.Low) * 100m - 100m;
          today.VolumeChangePercent = ((decimal) today.Volume / yesterday.Volume) * 100m - 100m;
        }

        quotes.ForEach(quote => symbolObj.DailyQuotes.Add(quote));
        await db.SaveChangesAsync();
      }
    }

    static async Task Scrape(string apiKey, IEnumerable<string> symbols)
    {
      var stockApi = new AlphaVantage(apiKey, new WebRequestFactory());
      foreach (var symbol in symbols)
      {
        var response = await stockApi.TimeseriesDaily(symbol, OutputSizes.Full);
        var oneSecond = Task.Delay(1000);
        await WriteIfNewSymbol(response);
        await oneSecond;
      }
    }

    static void Main(string[] args)
    {
//      var task =  Scrape("", new[] { "msft" });
//      task.Wait();
    }
  }
}