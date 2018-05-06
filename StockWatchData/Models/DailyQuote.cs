using System;
using System.Collections.Generic;

namespace StockWatchData.Models
{
  public class DailyQuote
  {
    public string Symbol { get; set; }
    public string Day { get; set; }
    public DateTime? Date { get; set; }
    public decimal Open { get; set; }
    public decimal Close { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public long Volume { get; set; }
    public decimal OpenChangePercent { get; set; }
    public decimal CloseChangePercent { get; set; }
    public decimal HighChangePercent { get; set; }
    public decimal LowChangePercent { get; set; }
    public decimal VolumeChangePercent { get; set; }
    public string CollectionFunction { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Symbol SymbolNavigation { get; set; }

    public override bool Equals(object obj)
    {
      return obj is DailyQuote quote &&
             Symbol == quote.Symbol &&
             Day == quote.Day &&
             Open == quote.Open &&
             Close == quote.Close &&
             High == quote.High &&
             Low == quote.Low &&
             Volume == quote.Volume &&
             OpenChangePercent == quote.OpenChangePercent &&
             CloseChangePercent == quote.CloseChangePercent &&
             HighChangePercent == quote.HighChangePercent &&
             LowChangePercent == quote.LowChangePercent &&
             VolumeChangePercent == quote.VolumeChangePercent &&
             CollectionFunction == quote.CollectionFunction;
    }

    public override int GetHashCode()
    {
      var hashCode = -2054459482;
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Symbol);
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Day);
      hashCode = hashCode * -1521134295 + Open.GetHashCode();
      hashCode = hashCode * -1521134295 + Close.GetHashCode();
      hashCode = hashCode * -1521134295 + High.GetHashCode();
      hashCode = hashCode * -1521134295 + Low.GetHashCode();
      hashCode = hashCode * -1521134295 + Volume.GetHashCode();
      hashCode = hashCode * -1521134295 + OpenChangePercent.GetHashCode();
      hashCode = hashCode * -1521134295 + CloseChangePercent.GetHashCode();
      hashCode = hashCode * -1521134295 + HighChangePercent.GetHashCode();
      hashCode = hashCode * -1521134295 + LowChangePercent.GetHashCode();
      hashCode = hashCode * -1521134295 + VolumeChangePercent.GetHashCode();
      hashCode = hashCode * -1521134295 +
                 EqualityComparer<string>.Default.GetHashCode(CollectionFunction);
      return hashCode;
    }

    public override string ToString()
    {
      return
        $"{nameof(Symbol)}: {Symbol}, {nameof(Day)}: {Day}, {nameof(Open)}: {Open}, " +
        $"{nameof(Close)}: {Close}, {nameof(High)}: {High}, {nameof(Low)}: {Low}, " +
        $"{nameof(Volume)}: {Volume}, {nameof(CloseChangePercent)}: {CloseChangePercent}%, " +
        $"{nameof(CollectionFunction)}: {CollectionFunction}";
    }
  }
}