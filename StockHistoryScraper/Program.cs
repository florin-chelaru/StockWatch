using Newtonsoft.Json.Linq;
using StockPredictor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StockHistoryScraper
{
  class Program
  {
    static async Task<IList<Entry>> GetStockYear(string symbol, int year)
    {
      Console.WriteLine("Getting: {0}, {1}", symbol, year);
      var start = new DateTime(year, 1, 1);
      var end = new DateTime(year, 12, 31);
      var query = string.Format("select * from yahoo.finance.historicaldata where symbol = \"{0}\" and startDate = \"{1}\" and endDate = \"{2}\"",
            symbol,  start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));
      var url = string.Format("https://query.yahooapis.com/v1/public/yql?format=json&diagnostics=false&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys&callback=&q={0}", Uri.EscapeUriString(query));

      var webReq = WebRequest.Create(url);
      using (var response = await webReq.GetResponseAsync())
      {
        using (var reader = new StreamReader(response.GetResponseStream()))
        {
          var text = reader.ReadToEnd().Trim();
          var obj = JObject.Parse(text);

          var r = obj["query"]["results"] as JObject;

          if (r == null)
          {
            Console.WriteLine("Finished: {0}, {1}", symbol, year);
            return new List<Entry>();
          }

          var results = r?["quote"] as JArray;

          if (results == null)
          {
            Console.WriteLine("Finished: {0}, {1}", symbol, year);
            return new List<Entry>();
          }

          var history = new List<Entry>();

          foreach (var result in results)
          {
            Entry entry = result.ToObject<Entry>();
            history.Add(entry);
          }

          //history.Sort((e1, e2) => DateTime.Compare(e1.Date, e2.Date));

          Console.WriteLine("Finished: {0}, {1}", symbol, year);

          return history;
        }
      }        
    }

    static async Task<IList<Entry>> GetStockHistory(string symbol, DateTime start, DateTime? end = null)
    {
      DateTime e = end ?? DateTime.Now;
      var startYear = start.Year;
      var endYear = e.Year;

      var yearTasks = new Dictionary<int, Task<IList<Entry>>>();
      for (var year = startYear; year <= endYear; ++year)
      {
        yearTasks[year] = GetStockYear(symbol, year);
      }

      List<Entry> all = new List<Entry>();
      foreach (var p in yearTasks)
      {
        var entries = await p.Value;
        all.AddRange(entries.Where(entry => entry.Date >= start && entry.Date <= e));
      }
      all.Sort((e1, e2) => DateTime.Compare(e1.Date, e2.Date));
      return all;
    }

    static async Task<IDictionary<string, IList<Entry>>> GetStocksHistories(IEnumerable<string> symbols, DateTime start, DateTime? end = null)
    {
      var tasks = new Dictionary<string, Task<IList<Entry>>>();
      foreach (var symbol in symbols)
      {
        try
        {
          tasks[symbol] = GetStockHistory(symbol, start, end);
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error for {0} {1}-{2}: {3}", symbol, start.Year, end != null ? end.Value.Year : DateTime.Now.Year, ex.Message);
        }
      }

      IDictionary<string, IList<Entry>> ret = new Dictionary<string, IList<Entry>>();
      foreach (var p in tasks)
      {
        try
        {
          ret[p.Key] = await p.Value;
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error for {0} {1}-{2}: {3}", p.Key, start.Year, end != null ? end.Value.Year : DateTime.Now.Year, ex.Message);
        }
      }

      return ret;
    }

    static void CheckStrategy(IDictionary<string, IList<Entry>> tests, Func<IList<Entry>, int, bool> strategy, int ngramSize = 2)
    {
      var count = 0;
      var total = 0;
      var correct = 0;
      var totalGainPc = 0.0;
      foreach (var test in tests.Values)
      {
        count += test.Count - ngramSize;
        for (var i = 0; i < test.Count - ngramSize - 1; ++i)
        {
          if (strategy(test, i))
          {
            ++total;

            if (test[i + ngramSize].Change > 0)
            {
              ++correct;
            }

            totalGainPc += test[i + ngramSize].ChangePercent;
          }
        }
      }

      Console.WriteLine("{0} {1} {2} | {3} {4} | {5}", correct, total, count, (double)correct / total, (double)total / count, totalGainPc * 100.0);
    }

    static void Main(string[] args)
    {
      var start = new DateTime(2016, 1, 1);
      var end = new DateTime(2016, 12, 31);
      var outDir = @"c:\Documents\work\stock-prediction\test";

      var symbols = new string[] { "amzn", "aapl", "tsla", "goog", "googl", "msft", "fb", "nflx", "baba", "yhoo" };
      //var symbols = new string[] { "fb", "baba"};
      //var symbols = new string[] { "amzn" };
      var t = GetStocksHistories(symbols, start, end);
      t.Wait();
      var histories = t.Result;

      Console.WriteLine("Writing to files:");
      foreach (var p in histories)
      {
        var fileName = string.Format("{0}-{1}-{2}.csv", p.Key, start.Year, end.Year);
        Console.WriteLine(fileName);
        using (var writer = new StreamWriter(Path.Combine(outDir, fileName)))
        {
          Entry last = null;
          foreach (var e in p.Value)
          {
            if (last != null)
            {
              e.Change = e.Close - last.Close;
              e.ChangePercent = e.Change / last.Close;
            }
            else
            {
              e.Change = e.Close - e.Open;
              e.ChangePercent = e.Change / e.Open;
            }
            last = e;
            writer.WriteLine("{0}{1}", e.ToCsv(), Math.Min(2, Math.Max(-2, e.ChangePercent < 0 ? Math.Floor(e.ChangePercent * 100.0) : Math.Ceiling(e.ChangePercent * 100.0))));
          }
        }
      }

      //var inDir = @"c:\Documents\work\stock-prediction\test";
      //var start = new DateTime(2016, 1, 1);
      //var end = new DateTime(2016, 12, 31);
      //var symbols = new string[] { "amzn", "aapl", "tsla", "goog", "googl", "msft", "fb", "nflx", "baba", "yhoo" };
      //IDictionary<string, IList<Entry>> histories = new Dictionary<string, IList<Entry>>();

      //foreach (var symbol in symbols)
      //{
      //  var fileName = string.Format("{0}-{1}-{2}.csv", symbol, start.Year, end.Year);
      //  var entries = new List<Entry>(Entry.FromCsvFile(Path.Combine(inDir, fileName)));
      //  histories[symbol] = entries;
      //}

      //var ngramSize = 3;
      //Func<Entry, int> entryHash = (e) => (int)Math.Min(2, Math.Max(-2, e.ChangePercent < 0 ? Math.Floor(e.ChangePercent * 100.0) : Math.Ceiling(e.ChangePercent * 100.0)));
      //CheckStrategy(histories, (entries, i) =>
      //{
      //  var e1 = entries[i];
      //  var e2 = entries[i + 1];
      //  var e3 = entries[i + 2];

      //  //return (entryHash(e1) < 0 && entryHash(e2) == -2 && entryHash(e3) == -2);
      //  return (entryHash(e1) == -2 && entryHash(e2) == -2 && entryHash(e3) == -2);
      //}, ngramSize);
    }
  }
}
