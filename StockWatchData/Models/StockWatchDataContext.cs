﻿using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading.Tasks;
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
      modelBuilder.Entity<DailyQuote>(entity =>
      {
        entity.HasKey(e => new {e.Symbol, e.Day});

        entity.Property(e => e.Symbol).HasMaxLength(50);

        entity.Property(e => e.Day).HasMaxLength(50);

        entity.Property(e => e.Open).HasColumnType("decimal(18, 5)");
        entity.Property(e => e.Close).HasColumnType("decimal(18, 5)");
        entity.Property(e => e.High).HasColumnType("decimal(18, 5)");
        entity.Property(e => e.Low).HasColumnType("decimal(18, 5)");

        entity.Property(e => e.CloseChangePercent).HasColumnType("decimal(18, 5)");

        entity.Property(e => e.CollectionFunction).HasMaxLength(50);

        entity.Property(e => e.Date)
          .HasColumnType("datetime")
          .HasComputedColumnSql("(CONVERT([datetime],[Day],(120)))");

        entity.Property(e => e.HighChangePercent).HasColumnType("decimal(18, 5)");

        entity.Property(e => e.LowChangePercent).HasColumnType("decimal(18, 5)");

        entity.Property(e => e.OpenChangePercent).HasColumnType("decimal(18, 5)");

        entity.Property(e => e.VolumeChangePercent).HasColumnType("decimal(18, 5)");

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
      });
    }

    public async Task<TEntity> RemoveIfExistsAsync<TEntity>(params object[] keyValues)
      where TEntity : class
    {
      var entity = await FindAsync<TEntity>(keyValues);
      if (entity == null)
      {
        return null;
      }

      Remove(entity);
      return entity;
    }

    public TEntity RemoveIfExists<TEntity>(params object[] key) where TEntity : class
    {
      return RemoveIfExistsAsync<TEntity>(key).Result;
    }

    public void RemoveRangeIfExists<TEntity>(ICollection<object[]> keys) where TEntity : class
    {
      foreach (var key in keys)
      {
        RemoveIfExists<TEntity>(key);
      }
    }

    public void RemoveRangeIfExists(ICollection<DailyQuote> quotes)
    {
      var keys = (from quote in quotes select new object[] {quote.Symbol, quote.Day}).ToList();
      RemoveRangeIfExists<DailyQuote>(keys);
    }
  }
}