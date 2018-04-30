namespace StockWatchData
{
  using System.Data.Entity;

  public partial class StockWatchDataContext : DbContext
  {
    public StockWatchDataContext()
        : base("name=StockWatchDataContext")
    {
    }

    public virtual DbSet<StockQuoteInterval> StockQuoteIntervals { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      modelBuilder.Entity<StockQuoteInterval>()
          .Property(e => e.Open)
          .HasPrecision(9, 2);

      modelBuilder.Entity<StockQuoteInterval>()
          .Property(e => e.Close)
          .HasPrecision(9, 2);

      modelBuilder.Entity<StockQuoteInterval>()
          .Property(e => e.High)
          .HasPrecision(9, 2);

      modelBuilder.Entity<StockQuoteInterval>()
          .Property(e => e.Low)
          .HasPrecision(9, 2);
    }
  }
}
