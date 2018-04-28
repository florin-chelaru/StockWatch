using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Threading.Tasks;
using Common;
using Newtonsoft.Json.Linq;

namespace AlphaVantageApi
{
  public class AlphaVantage
  {
    private const string ApiUrl = "https://www.alphavantage.co";
    private const string ApiKeyArg = "apiKey";
    private const string FunctionArg = "function";
    private const string SymbolArg = "symbol";
    private const string IntervalArg = "interval";
    private const string OutputSizeArg = "outputsize";

    private const string TimeseriesIntradayFunction = "TIME_SERIES_INTRADAY";
//    private const string TimeseriesDailyFunction = "TIME_SERIES_DAILY";

    private readonly string apiKey;
    private readonly IWebRequestFactory webRequestFactory;

    public AlphaVantage(string apiKey, IWebRequestFactory webRequestFactory)
    {
      this.apiKey = apiKey;
      this.webRequestFactory = webRequestFactory;
    }

    public async Task<TimeseriesIntradayResponse> TimeseriesIntraday(string symbol,
      string interval = Intervals.OneMin,
      string outputSize = OutputSizes.Compact)
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

    public string TimeseriesDaily()
    {
      return null;
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

    public sealed class TimeseriesIntradayResponse
    {
      public string Symbol { get; }
      public ImmutableDictionary<string, string> Metadata { get; }
      public ImmutableList<Tick> Timeseries { get; }

      private string prettyString;
      private readonly int hashCode;

      public TimeseriesIntradayResponse(string symbol, ImmutableDictionary<string, string> metadata,
        ImmutableList<Tick> timeseries)
      {
        Symbol = symbol;
        Metadata = metadata;
        Timeseries = timeseries;

        hashCode = GenerateHashCode();
      }

      public static TimeseriesIntradayResponse FromJson(string symbol, string json)
      {
        var jObject = JObject.Parse(json);
        var metadata = ImmutableDictionary.CreateBuilder<string, string>();
        if (jObject.TryGetValue("Meta Data", out var jMetadataToken) &&
            jMetadataToken is JObject jMetadata)
        {
          foreach (var entry in jMetadata.Properties())
          {
            metadata[entry.Name] = (entry.Value as JValue)?.ToString(CultureInfo.InvariantCulture);
          }
        }

        var timeseries = ImmutableList.CreateBuilder<Tick>();
        var jTimeseriesToken = (from property in jObject.Properties()
          where property.Name.StartsWith("Time Series")
          select property.Value).FirstOrDefault();
        if (jTimeseriesToken != null && jTimeseriesToken is JObject jTimeseries)
        {
          foreach (var property in jTimeseries.Properties())
          {
            var tick = Tick.FromJsonProperty(property);
            timeseries.Add(tick);
          }
        }

        return new TimeseriesIntradayResponse(symbol, metadata.ToImmutable(),
          timeseries.ToImmutable());
      }

      public override bool Equals(object obj)
      {
        return obj is TimeseriesIntradayResponse response &&
               Symbol == response.Symbol &&
               EqualityUtil.DictionariesAreEqual(Metadata, response.Metadata) &&
               EqualityUtil.ListsAreEqual(Timeseries, response.Timeseries);
      }

      private int GenerateHashCode()
      {
        var code = -1422593982;
        code = code * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Symbol);
        code = code * -1521134295 +
               EqualityComparer<ImmutableDictionary<string, string>>.Default.GetHashCode(Metadata);
        code = code * -1521134295 +
               EqualityComparer<ImmutableList<Tick>>.Default.GetHashCode(Timeseries);
        return code;
      }

      public override int GetHashCode()
      {
        return hashCode;
      }

      private string GeneratePrettyString()
      {
        return
          $"{nameof(Symbol)}: {Symbol}, {nameof(Metadata)}: {Metadata.ToPrettyString()}, {nameof(Timeseries)}: {Timeseries.ToPrettyString()}";
      }

      public override string ToString()
      {
        return prettyString ?? (prettyString = GeneratePrettyString());
      }
    }

    public sealed class Tick
    {
      public DateTime Time { get; }
      public double Open { get; }
      public double Close { get; }
      public double High { get; }
      public double Low { get; }
      public ulong Volume { get; }

      public Tick(DateTime time, double open, double close, double high, double low, ulong volume)
      {
        Time = time;
        Open = open;
        Close = close;
        High = high;
        Low = low;
        Volume = volume;
      }

      public static Tick FromJsonProperty(JProperty jProperty)
      {
        string time = jProperty.Name;
        var jTick = jProperty.Value as JObject;
        string open = ExtractValue(jTick, "open");
        string close = ExtractValue(jTick, "close");
        string high = ExtractValue(jTick, "high");
        string low = ExtractValue(jTick, "low");
        string volume = ExtractValue(jTick, "volume");

        return new Tick(DateTime.Parse(time), double.Parse(open), double.Parse(close),
          double.Parse(high), double.Parse(low), ulong.Parse(volume));
      }

      private static string ExtractValue(JObject jTick, string filter)
      {
        if (jTick == null)
        {
          return string.Empty;
        }

        return (from property in jTick.Properties()
                 where property.Name.Contains(filter)
                 select (property.Value as JValue)?.Value.ToString()).FirstOrDefault() ??
               string.Empty;
      }

      public override bool Equals(object obj)
      {
        return obj is Tick tick &&
               Time == tick.Time &&
               Math.Abs(Open - tick.Open) < double.Epsilon &&
               Math.Abs(Close - tick.Close) < double.Epsilon &&
               Math.Abs(High - tick.High) < double.Epsilon &&
               Math.Abs(Low - tick.Low) < double.Epsilon &&
               Volume == tick.Volume;
      }

      public override int GetHashCode()
      {
        var hashCode = 2130425085;
        hashCode = hashCode * -1521134295 + Time.GetHashCode();
        hashCode = hashCode * -1521134295 + Open.GetHashCode();
        hashCode = hashCode * -1521134295 + Close.GetHashCode();
        hashCode = hashCode * -1521134295 + High.GetHashCode();
        hashCode = hashCode * -1521134295 + Low.GetHashCode();
        hashCode = hashCode * -1521134295 + Volume.GetHashCode();
        return hashCode;
      }

      public override string ToString()
      {
        return
          $"{nameof(Time)}: {Time}, {nameof(Open)}: {Open}, {nameof(Close)}: {Close}, {nameof(High)}: {High}, {nameof(Low)}: {Low}, {nameof(Volume)}: {Volume}";
      }
    }
  }
}