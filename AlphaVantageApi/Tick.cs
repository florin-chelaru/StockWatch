using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace AlphaVantageApi
{
  public sealed class Tick
  {
    public DateTime Time { get; }
    public decimal Open { get; }
    public decimal Close { get; }
    public decimal High { get; }
    public decimal Low { get; }
    public ulong Volume { get; }

    public Tick(DateTime time, decimal open, decimal close, decimal high, decimal low, ulong volume)
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

      return new Tick(DateTime.Parse(time), decimal.Parse(open), decimal.Parse(close),
        decimal.Parse(high), decimal.Parse(low), ulong.Parse(volume));
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
             Open == tick.Open &&
             Close == tick.Close &&
             High == tick.High &&
             Low == tick.Low &&
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