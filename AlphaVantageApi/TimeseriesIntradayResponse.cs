using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Common;
using Newtonsoft.Json.Linq;

namespace AlphaVantageApi
{
  public sealed class TimeseriesIntradayResponse
  {
    public string Symbol { get; }
    public ImmutableDictionary<string, string> Metadata { get; }
    public ImmutableList<Tick> Timeseries { get; }

    public TimeseriesIntradayResponse(string symbol, ImmutableDictionary<string, string> metadata,
      ImmutableList<Tick> timeseries)
    {
      Symbol = symbol;
      Metadata = metadata;
      Timeseries = timeseries;
    }

    public static TimeseriesIntradayResponse FromJson(string symbol, string json)
    {
      var jObject = JObject.Parse(json);
      ImmutableDictionary<string, string> metadata;
      if (jObject.TryGetValue("Meta Data", out var jMetadataToken) &&
          jMetadataToken is JObject jMetadata)
      {
        metadata = jMetadata.ToObject<ImmutableDictionary<string, string>>();
      }
      else
      {
        metadata = ImmutableDictionary<string, string>.Empty;
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

      return new TimeseriesIntradayResponse(symbol, metadata, timeseries.ToImmutable());
    }

    public override bool Equals(object obj)
    {
      return obj is TimeseriesIntradayResponse response &&
             Symbol == response.Symbol &&
             EqualityUtil.DictionariesAreEqual(Metadata, response.Metadata) &&
             EqualityUtil.ListsAreEqual(Timeseries, response.Timeseries);
    }

    public override int GetHashCode()
    {
      var code = -1422593982;
      code = code * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Symbol);
      code = code * -1521134295 +
             EqualityComparer<ImmutableDictionary<string, string>>.Default.GetHashCode(Metadata);
      code = code * -1521134295 +
             EqualityComparer<ImmutableList<Tick>>.Default.GetHashCode(Timeseries);
      return code;
    }

    public override string ToString()
    {
      return
        $"{nameof(Symbol)}: {Symbol}, {nameof(Metadata)}: {Metadata.ToPrettyString()}, {nameof(Timeseries)}: {Timeseries.ToPrettyString()}";
    }
  }
}