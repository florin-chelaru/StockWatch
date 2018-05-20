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

    private async Task UpdateDatabase(TimeseriesDailyResponse response, string group = null)
    {
      var db = DataContextFactory.DataContext;
      var symbol = response.Symbol;
      var symbolObj = await db.Symbols.FindAsync(symbol);
      if (symbolObj == null)
      {
        db.Symbols.Add(symbolObj = new Symbol {Id = symbol});
      }

      if (group != null)
      {
        var groupObj = await db.Groups.FindAsync(group);
        if (groupObj == null)
        {
          db.Groups.Add(new Group {Id = group});
        }

        var groupMembership = await db.SymbolGroupMemberships.FindAsync(symbol, group);
        if (groupMembership == null)
        {
          db.SymbolGroupMemberships.Add(new SymbolGroupMembership {Symbol = symbol, Group = group});
        }

        symbolObj.AddTag(group);
      }

      var latestDay = await (from quote in db.DailyQuotes
        where quote.Symbol == symbol
        select quote.Day).MaxAsync();
      DateTime latestDate = string.IsNullOrEmpty(latestDay)
        ? DateTime.MinValue
        : DateTime.Parse(latestDay);

      var quotes = (from tick in response.Timeseries
        where tick.Time >= latestDate
        select new DailyQuote
        {
          Symbol = symbol,
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

      quotes.Where(quote => quote.Date > latestDate).Select(quote => quote).ToList()
        .ForEach(quote =>
        {
          db.DailyQuotes.Add(quote);
          Console.WriteLine($"Wrote quote: {quote}");
        });
      await db.SaveChangesAsync();
    }

    public async Task Scrape(IEnumerable<string> symbols, string group = null)
    {
      var db = DataContextFactory.DataContext;
      var symbolList = symbols.ToList();
      foreach (var symbol in symbolList)
      {
        var symbolObj = await db.Symbols.FindAsync(symbol);
        if (symbolObj != null)
        {
          continue;
        }

        var response = await AlphaVantageApi.TimeseriesDaily(symbol, OutputSizes.Full);
        var oneSecond = Task.Delay(1000);
        await UpdateDatabase(response, group);
        await oneSecond;
      }

      if (group != null)
      {
        await db.AddSymbolsToGroup(symbolList, group);
      }
    }
  }
}