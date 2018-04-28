namespace StockWatchData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

  public partial class StockQuoteInterval
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(255)]
        public string Symbol { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime StartTime { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime EndTime { get; set; }

        [StringLength(255)]
        public string Market { get; set; }

        [Column(TypeName = "money")]
        public decimal Open { get; set; }

        [Column(TypeName = "money")]
        public decimal Close { get; set; }

        [Column(TypeName = "money")]
        public decimal High { get; set; }

        [Column(TypeName = "money")]
        public decimal Low { get; set; }

        public long Volume { get; set; }

        [StringLength(255)]
        public string CollectionFunction { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
