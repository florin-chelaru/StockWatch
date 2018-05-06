using Microsoft.EntityFrameworkCore;

namespace StockWatchData.Models
{
  public class StockWatchDataContext : DbContext
  {
    public virtual DbSet<DailyQuote> DailyQuotes { get; set; }
    public virtual DbSet<Group> Groups { get; set; }
    public virtual DbSet<SymbolGroupMembership> SymbolGroupMemberships { get; set; }
    public virtual DbSet<Symbol> Symbols { get; set; }

    public StockWatchDataContext()
    {
    }

    public StockWatchDataContext(DbContextOptions<StockWatchDataContext> options)
      : base(options)
    {
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      if (!optionsBuilder.IsConfigured)
      {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
        optionsBuilder.UseSqlServer(
          @"Server=FLORIN-MAC\SQLEXPRESS;Database=stockwatch2;Trusted_Connection=True;");
      }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      // TODO: Check whether this is needed
//      var dbSets = GetType().GetProperties()
//        .Where(p => p.PropertyType.Name == "DbSet`1")
//        .Select(p => new
//        {
//          PropertyName = p.Name,
//          EntityType = p.PropertyType.GenericTypeArguments.Single()
//        })
//        .ToArray();
//
//      foreach (var type in modelBuilder.Model.GetEntityTypes())
//      {
//        var dbset = dbSets.SingleOrDefault(s => s.EntityType == type.ClrType);
//        if (dbset != null)
//        {
//          type.Relational().TableName = dbset.PropertyName;
//        }
//      }

      modelBuilder.Entity<DailyQuote>(entity =>
      {
        entity.HasKey(e => new {e.Symbol, e.Day});

        entity.Property(e => e.Symbol).HasMaxLength(50);

        entity.Property(e => e.Day).HasMaxLength(50);

        entity.Property(e => e.CloseChangePercent).HasColumnType("decimal(18, 3)");

        entity.Property(e => e.CollectionFunction).HasMaxLength(50);

        entity.Property(e => e.Date)
          .HasColumnType("datetime")
          .HasComputedColumnSql("(CONVERT([datetime],[Day],(120)))");

        entity.Property(e => e.HighChangePercent).HasColumnType("decimal(18, 3)");

        entity.Property(e => e.LowChangePercent).HasColumnType("decimal(18, 3)");

        entity.Property(e => e.OpenChangePercent).HasColumnType("decimal(18, 3)");

        entity.Property(e => e.VolumeChangePercent).HasColumnType("decimal(18, 3)");

        entity.HasOne(d => d.SymbolNavigation)
          .WithMany(p => p.DailyQuotes)
          .HasForeignKey(d => d.Symbol)
          .HasConstraintName("FK_DailyQuotes_Symbols");
      });

      modelBuilder.Entity<Group>(entity =>
      {
        entity.Property(e => e.Id)
          .HasMaxLength(50)
          .ValueGeneratedNever();
      });

      modelBuilder.Entity<SymbolGroupMembership>(entity =>
      {
        entity.HasKey(e => new {e.Symbol, e.Group});

        entity.Property(e => e.Symbol).HasMaxLength(50);

        entity.Property(e => e.Group).HasMaxLength(50);

        entity.HasOne(d => d.GroupNavigation)
          .WithMany(p => p.SymbolGroupMemberships)
          .HasForeignKey(d => d.Group)
          .HasConstraintName("FK_SymbolGroupMemberships_Groups");

        entity.HasOne(d => d.SymbolNavigation)
          .WithMany(p => p.SymbolGroupMemberships)
          .HasForeignKey(d => d.Symbol)
          .HasConstraintName("FK_SymbolGroupMemberships_Symbols");
      });

      modelBuilder.Entity<Symbol>(entity =>
      {
        entity.Property(e => e.Id)
          .HasMaxLength(50)
          .ValueGeneratedNever();

        entity.Property(e => e.Market).HasMaxLength(50);
//        entity.HasMany(e => e.DailyQuotes).WithOne();
      });
    }
  }
}