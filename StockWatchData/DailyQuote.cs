using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockWatchData
{
  public class DailyQuote
  {
    [Key]
    [Column(Order = 0)]
    [StringLength(50)]
    public string Symbol { get; set; }

    [Key]
    [Column(Order = 1)]
    [StringLength(50)]
    public string Day { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime Date { get; set; }

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

    [StringLength(50)]
    public string CollectionFunction { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Symbol SymbolObject { get; set; }

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