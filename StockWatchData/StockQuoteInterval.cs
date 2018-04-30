namespace StockWatchData
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  public partial class StockQuoteInterval
  {
    [Key]
    [Column(Order = 0)]
    [StringLength(63)]
    public string Symbol { get; set; }

    [Key]
    [Column(Order = 1)]
    public DateTime StartTime { get; set; }

    [Key]
    [Column(Order = 2)]
    public DateTime EndTime { get; set; }

    public decimal Open { get; set; }

    public decimal Close { get; set; }

    public decimal High { get; set; }

    public decimal Low { get; set; }

    public long Volume { get; set; }

    public CollectionFunction CollectionFunction { get; set; }
  }
}