using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Threading.Tasks;
using Common;

namespace AlphaVantageApi
{
  public class AlphaVantage
  {
    private const string ApiUrl = "https://www.alphavantage.co";
    private const string ApiKeyArg = "apikey";
    private const string FunctionArg = "function";
    private const string SymbolArg = "symbol";
    private const string IntervalArg = "interval";
    private const string OutputSizeArg = "outputsize";

    private const string TimeseriesIntradayFunction = "TIME_SERIES_INTRADAY";
    private const string TimeseriesDailyFunction = "TIME_SERIES_DAILY";

    private readonly string apiKey;
    private readonly IWebRequestFactory webRequestFactory;

    public AlphaVantage(string apiKey, IWebRequestFactory webRequestFactory)
    {
      this.apiKey = apiKey;
      this.webRequestFactory = webRequestFactory;
    }

    public async Task<TimeseriesIntradayResponse> TimeseriesIntraday(string symbol,
      string interval = Intervals.OneMin, string outputSize = OutputSizes.Compact)
    {
      var args = ImmutableDictionary.CreateBuilder<string, string>();
      args[ApiKeyArg] = apiKey;
      args[FunctionArg] = TimeseriesIntradayFunction;
      args[SymbolArg] = symbol;
      args[IntervalArg] = interval;
      args[OutputSizeArg] = outputSize;
      string json = await CallApi(args.ToImmutable());
      return TimeseriesIntradayResponse.FromJson(symbol, json);
    }

    public async Task<TimeseriesDailyResponse> TimeseriesDaily(string symbol, string outputSize = OutputSizes.Compact)
    {
      var args = ImmutableDictionary.CreateBuilder<string, string>();
      args[ApiKeyArg] = apiKey;
      args[FunctionArg] = TimeseriesDailyFunction;
      args[SymbolArg] = symbol;
      args[OutputSizeArg] = outputSize;

      string json = await CallApi(args.ToImmutable());
      return TimeseriesDailyResponse.FromJson(symbol, json);
    }

    private async Task<string> CallApi(ImmutableDictionary<string, string> args)
    {
      var query = BuildApiQuery(args);
      var webReq = webRequestFactory.Create(query);

      var noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.BypassCache);
      webReq.CachePolicy = noCachePolicy;
      using (var response = await webReq.GetResponseAsync())
      {
        var responseStream = response?.GetResponseStream();
        if (responseStream == null)
        {
          throw new AlphaVantageApiException($"Null response stream for query '{query}'.");
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
  }
}