using System.Data.Entity;

namespace StockWatchData
{
  public class StockWatchDataContext : DbContext
  {
    public StockWatchDataContext()
      : base("name=StockWatchDataContextLocal")
    {
    }

    public virtual DbSet<DailyQuote> DailyQuotes { get; set; }
    public virtual DbSet<Group> Groups { get; set; }
    public virtual DbSet<Symbol> Symbols { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      modelBuilder.Entity<DailyQuote>()
        .Property(e => e.OpenChangePercent)
        .HasPrecision(18, 3);

      modelBuilder.Entity<DailyQuote>()
        .Property(e => e.CloseChangePercent)
        .HasPrecision(18, 3);

      modelBuilder.Entity<DailyQuote>()
        .Property(e => e.HighChangePercent)
        .HasPrecision(18, 3);

      modelBuilder.Entity<DailyQuote>()
        .Property(e => e.LowChangePercent)
        .HasPrecision(18, 3);

      modelBuilder.Entity<DailyQuote>()
        .Property(e => e.VolumeChangePercent)
        .HasPrecision(18, 3);

      modelBuilder.Entity<Group>()
        .HasMany(e => e.Symbols)
        .WithMany(e => e.Groups)
        .Map(m => m.ToTable("SymbolGroupMembership").MapLeftKey("Group").MapRightKey("Symbol"));

      modelBuilder.Entity<Symbol>()
        .HasMany(e => e.DailyQuotes)
        .WithRequired(e => e.SymbolObject)
        .HasForeignKey(e => e.Symbol)
        .WillCascadeOnDelete(false);
    }
  }
}