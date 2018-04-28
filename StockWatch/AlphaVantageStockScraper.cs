using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using StockPredictor;

namespace StockWatch
{
  class AlphaVantageStockScraper
  {
    private const string ApiUrl = "https://www.alphavantage.co";
    private const string FunctionArg = "function";

    private static readonly ILogger Logger = new ConsoleLogger();

    private readonly string apiKey;

    public AlphaVantageStockScraper(string apiKey)
    {
      this.apiKey = apiKey;
    }

    private async Task<string> CallApi(ImmutableDictionary<string, string> args)
    {
      var argsWithApiKey = args.ToBuilder();
      argsWithApiKey["apiKey"] = apiKey;
      var query = BuildApiQuery(argsWithApiKey.ToImmutable());
      var webReq = WebRequest.Create(query);

      var noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.BypassCache);
      webReq.CachePolicy = noCachePolicy;
      using (var response = await webReq.GetResponseAsync())
      {
        var responseStream = response?.GetResponseStream();
        if (responseStream == null)
        {
          throw new StockWatchException($"Null response stream for query '{query}'.");
        }

        using (var reader = new StreamReader(responseStream))
        {
          return reader.ReadToEnd().Trim();
        }
      }
    }

    private static string BuildApiQuery(ImmutableDictionary<string, string> args)
    {
      var concatenatedArgs =
        string.Join("&", from entry in args select $"{entry.Key}={entry.Value}");
      return $"{ApiUrl}/query?{concatenatedArgs}";
    }

    public Task<Entry> GetQuote(string symbol)
    {
      throw new NotImplementedException();
    }

    public Task<IDictionary<string, Entry>> GetQuotes(
      ICollection<string> symbols, string market = "nasdaq")
    {
      throw new NotImplementedException();
    }

    public Task<IList<Entry>> GetTimeseries(string symbol, DateTime start,
      DateTime? end = null)
    {
      throw new NotImplementedException();
    }

    public Task<IDictionary<string, IList<Entry>>> GetTimeseries(
      ICollection<string> symbols, DateTime start, DateTime? end = null)
    {
      throw new NotImplementedException();
    }

    public Task<IList<Entry>> GetSmallTimeseries(string symbol, DateTime start,
      DateTime end)
    {
      throw new NotImplementedException();
    }

    public Task<IList<Entry>> GetTimeseriesByYear(string symbol, int year)
    {
      throw new NotImplementedException();
    }

    public sealed class ApiFunction
    {
      public static readonly ApiFunction TimeSeriesIntraday =
        new ApiFunction("TIME_SERIES_INTRADAY");

      public string Name { get; }

      private ApiFunction(string name)
      {
        Name = name;
      }
    }
  }
}