using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlphaVantageApi;
using Microsoft.EntityFrameworkCore;
using StockWatchData.Models;

namespace AlphaVantageScraper
{
  public class AlphaVantageScraper
  {
    private IAlphaVantage AlphaVantageApi { get; }

    private IStockWatchDataContextFactory DataContextFactory { get; }

    public AlphaVantageScraper(IAlphaVantage alphaVantageApi, IStockWatchDataContextFactory dataContextFactory)
    {
      AlphaVantageApi = alphaVantageApi;
      DataContextFactory = dataContextFactory;
    }

    private async Task UpdateDatabase(TimeseriesDailyResponse response)
    {
      using (var db = DataContextFactory.CreateDataContext())
      {
        var symbolObj =
          (from symbol in db.Symbols.Include(s => s.DailyQuotes)
            where symbol.Id == response.Symbol
            select symbol)
          .FirstOrDefault();
        if (symbolObj == null)
        {
          db.Symbols.Add(symbolObj = new Symbol {Id = response.Symbol});
        }

        DateTime? maxDate = DateTime.MinValue;
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
        // Date will never actually be null, since it's a computed column.
        // ReSharper disable PossibleInvalidOperationException
        quotes.Sort((q1, q2) => DateTime.Compare(q1.Date.Value, q2.Date.Value));
        // ReSharper restore PossibleInvalidOperationException

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

    public async Task Scrape(IEnumerable<string> symbols)
    {
      foreach (var symbol in symbols)
      {
        var response = await AlphaVantageApi.TimeseriesDaily(symbol, OutputSizes.Full);
        var oneSecond = Task.Delay(1000);
        await UpdateDatabase(response);
        await oneSecond;
      }
    }
  }
}