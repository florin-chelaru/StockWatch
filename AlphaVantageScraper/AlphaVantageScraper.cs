using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlphaVantageApi;
using Common;
using Microsoft.EntityFrameworkCore;
using StockWatchData.Models;

namespace AlphaVantageScraper
{
  public class AlphaVantageScraper
  {
    private IAlphaVantage AlphaVantageApi { get; }

    private IStockWatchDataContextFactory DataContextFactory { get; }

    public AlphaVantageScraper(IAlphaVantage alphaVantageApi,
      IStockWatchDataContextFactory dataContextFactory)
    {
      AlphaVantageApi = alphaVantageApi;
      DataContextFactory = dataContextFactory;
    }

    private async Task UpdateDatabase(TimeseriesDailyResponse response)
    {
      var db = DataContextFactory.DataContext;
      //using (var db = DataContextFactory.CreateDataContext())
      //{
        var symbolObj =
          (from symbol in db.Symbols
            where symbol.Id == response.Symbol
            select symbol)
          .FirstOrDefault();
        if (symbolObj == null)
        {
          db.Symbols.Add(symbolObj = new Symbol {Id = response.Symbol});
        }

        var latestDay = (from quote in db.DailyQuotes
          where quote.Symbol == response.Symbol
          select quote.Day).Max();
        DateTime latestDate = string.IsNullOrEmpty(latestDay)
          ? DateTime.MinValue
          : DateTime.Parse(latestDay);

        var quotes = (from tick in response.Timeseries
          where tick.Time >= latestDate
          select new DailyQuote
          {
            Symbol = response.Symbol,
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

          today.OpenChangePercent = today.Open.ComputeChangeRatio(yesterday.Open);
          today.CloseChangePercent = today.Close.ComputeChangeRatio(yesterday.Close);
          today.HighChangePercent = today.High.ComputeChangeRatio(yesterday.High);
          today.LowChangePercent = today.Low.ComputeChangeRatio(yesterday.Low);
          today.VolumeChangePercent = ((decimal) today.Volume).ComputeChangeRatio(yesterday.Volume);
        }

      //        quotes.Where(quote => quote.Date > latestDate).Select(quote => quote).ToList()
      //          .ForEach(quote => symbolObj.DailyQuotes.Add(quote));
      quotes.Where(quote => quote.Date > latestDate).Select(quote => quote).ToList()
        .ForEach(quote => db.DailyQuotes.Add(quote));
      await db.SaveChangesAsync();
      //}
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