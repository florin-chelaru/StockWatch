using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StockPredictor;

namespace StockWatch
{
  public class StockScraper
  {
    readonly ILogger logger;

    public StockScraper(ILogger logger = null)
    {
      this.logger = logger ?? new ConsoleLogger();
    }

    /// <summary>
    /// This will not work for large ranges of time, per Yahoo API restriction
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<IList<Entry>> GetStockSmallHistory(string symbol,
      DateTime start, DateTime end)
    {
      //var start = new DateTime(year, 1, 1);
      //var end = new DateTime(year, 12, 31);
      var startStr = start.ToString("yyyy-MM-dd");
      var endStr = end.ToString("yyyy-MM-dd");
      var query = string.Format(
        "select * from yahoo.finance.historicaldata where symbol = \"{0}\" and startDate = \"{1}\" and endDate = \"{2}\"",
        symbol, startStr, endStr);
      var url = string.Format(
        "https://query.yahooapis.com/v1/public/yql?format=json&diagnostics=false&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys&callback=&q={0}",
        Uri.EscapeUriString(query));

      logger.Info(string.Format("Getting: {0}, {1}-{2}", symbol, startStr,
        endStr));

      var webReq = WebRequest.Create(url);
      HttpRequestCachePolicy noCachePolicy =
        new HttpRequestCachePolicy(HttpRequestCacheLevel.BypassCache);
      webReq.CachePolicy = noCachePolicy;
      using (var response = await webReq.GetResponseAsync())
      {
        using (var reader = new StreamReader(response.GetResponseStream()))
        {
          var text = reader.ReadToEnd().Trim();
          var obj = JObject.Parse(text);

          if (!(obj["query"]["results"] is JObject r))
          {
            logger.Info(string.Format("Finished: {0}, {1}-{2}", symbol,
              startStr, endStr));
            return new List<Entry>();
          }

          var results = r["quote"] as JArray;

          if (results == null)
          {
            logger.Info(string.Format("Finished: {0}, {1}-{2}", symbol,
              startStr, endStr));
            return new List<Entry>();
          }

          var history = new List<Entry>();

          foreach (var result in results)
          {
            Entry entry = result.ToObject<Entry>();
            history.Add(entry);
          }

          history.Sort((e1, e2) => DateTime.Compare(e1.Date, e2.Date));
          for (var i = 0; i < history.Count; ++i)
          {
            var entry = history[i];
            entry.Change = i == 0
              ? entry.Close - entry.Open
              : entry.Close - history[i - 1].Close;
            entry.ChangePercent = i == 0
              ? entry.Change / entry.Open
              : entry.Change / history[i - 1].Close;
          }

          logger.Info(string.Format("Finished: {0}, {1}-{2}", symbol, startStr,
            endStr));

          return history;
        }
      }
    }

    public async Task<IList<Entry>> GetStockYear(string symbol, int year)
    {
      var start = new DateTime(year, 1, 1);
      var end = new DateTime(year, 12, 31);
      var query = string.Format(
        "select * from yahoo.finance.historicaldata where symbol = \"{0}\" and startDate = \"{1}\" and endDate = \"{2}\"",
        symbol, start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));
      var url = string.Format(
        "https://query.yahooapis.com/v1/public/yql?format=json&diagnostics=false&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys&callback=&q={0}",
        Uri.EscapeUriString(query));

      logger.Info(string.Format("Getting: {0}, {1}", symbol, year));

      var webReq = WebRequest.Create(url);
      HttpRequestCachePolicy noCachePolicy =
        new HttpRequestCachePolicy(HttpRequestCacheLevel.BypassCache);
      webReq.CachePolicy = noCachePolicy;
      using (var response = await webReq.GetResponseAsync())
      {
        using (var reader = new StreamReader(response.GetResponseStream()))
        {
          var text = reader.ReadToEnd().Trim();
          var obj = JObject.Parse(text);

          var r = obj["query"]["results"] as JObject;

          if (r == null)
          {
            logger.Info(string.Format("Finished: {0}, {1}", symbol, year));
            return new List<Entry>();
          }

          if (!(r["quote"] is JArray results))
          {
            logger.Info(string.Format("Finished: {0}, {1}", symbol, year));
            return new List<Entry>();
          }

          var history = new List<Entry>();

          foreach (var result in results)
          {
            Entry entry = result.ToObject<Entry>();
            history.Add(entry);
          }

          history.Sort((e1, e2) => DateTime.Compare(e1.Date, e2.Date));
          for (var i = 0; i < history.Count; ++i)
          {
            var entry = history[i];
            entry.Change = i == 0
              ? entry.Close - entry.Open
              : entry.Close - history[i - 1].Close;
            entry.ChangePercent = i == 0
              ? entry.Change / entry.Open
              : entry.Change / history[i - 1].Close;
          }

          logger.Info(string.Format("Finished: {0}, {1}", symbol, year));

          return history;
        }
      }
    }

    public async Task<IList<Entry>> GetStockHistory(string symbol,
      DateTime start, DateTime? end = null)
    {
      DateTime e = end ?? DateTime.Now;
      var startYear = start.Year;
      var endYear = e.Year;

      List<Entry> all = new List<Entry>();

      for (var year = startYear; year <= endYear; ++year)
      {
        bool error;

        do
        {
          try
          {
            var entries = await GetStockYear(symbol, year);
            all.AddRange(entries.Where(entry =>
              entry.Date >= start && entry.Date <= e));
            error = false;
          }
          catch (WebException ex)
          {
            error = true;
            using (var reader =
              new StreamReader(ex.Response.GetResponseStream()))
            {
              var text = reader.ReadToEnd().Trim();
              logger.Warn(string.Format(
                "Failed to get history for {0}, {1}. Trying again. Details: {2}",
                symbol, year, text));
            }

            await Task.Delay(5000);
          }
        } while (error);

        await Task.Delay(1000);
      }

      all.Sort((e1, e2) => DateTime.Compare(e1.Date, e2.Date));
      return all;
    }

    public async Task<IDictionary<string, IList<Entry>>> GetStocksHistories(
      IEnumerable<string> symbols, DateTime start, DateTime? end = null)
    {
      var tasks = new Dictionary<string, Task<IList<Entry>>>();
      foreach (var symbol in symbols)
      {
        tasks[symbol] = GetStockHistory(symbol, start, end);
      }

      IDictionary<string, IList<Entry>> ret =
        new Dictionary<string, IList<Entry>>();
      foreach (var p in tasks)
      {
        ret[p.Key] = await p.Value;
      }

      return ret;
    }

    public IDictionary<string, IList<Entry>> GetCachedStocksHistories(
      IEnumerable<string> symbols, DirectoryInfo inDir)
    {
      IDictionary<string, IList<Entry>> histories =
        new Dictionary<string, IList<Entry>>();
      foreach (var symbol in symbols)
      {
        var files = inDir.GetFiles(string.Format("{0}*.csv", symbol),
          SearchOption.TopDirectoryOnly);
        if (files.Length == 0)
        {
          continue;
        }

        var entries = Entry.FromCsvFile(files[0].FullName);
        Array.Sort(entries, (e1, e2) => DateTime.Compare(e1.Date, e2.Date));
        histories[symbol] = entries;
      }

      return histories;
    }

    public async Task<Entry> GetRealTimeQuote(string symbol,
      string market = "nasdaq")
    {
      var webReq = WebRequest.Create(string.Format(
        @"http://finance.google.com/finance/info?client=ig&q={1}:{0}", symbol,
        market));

      using (var response = await webReq.GetResponseAsync())
      {
        using (var reader = new StreamReader(response.GetResponseStream()))
        {
          var text = reader.ReadToEnd().Replace("//", "").Trim();
          var obj = JArray.Parse(text);
          StockQuote quote = obj[0].ToObject<StockQuote>();

          return quote.ToEntry();
        }
      }
    }

    public async Task<IDictionary<string, Entry>> GetRealTimeQuotes(
      IEnumerable<string> symbols, string market = "nasdaq")
    {
      var tasks = new Dictionary<string, Task<Entry>>();
      foreach (var symbol in symbols)
      {
        tasks[symbol] = GetRealTimeQuote(symbol, market);
      }

      var ret = new Dictionary<string, Entry>();
      foreach (var p in tasks)
      {
        ret[p.Key] = await p.Value;
      }

      return ret;
    }
  }
}