using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using AlphaVantageApi;
using Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using StockWatchData.Models;

namespace AlphaVantageScraper
{
  [TestFixture]
  public class AlphaVantageScraperTest
  {
    #region Fields

    private const string Msft = "msft";
    private static readonly TimeseriesDailyResponse TimeseriesDailyResponse = CreateDailyResponse();

    private static readonly ImmutableList<DailyQuote> DailyQuotes = ImmutableList.Create(
      new DailyQuote
      {
        Symbol = Msft,
        Day = "2018-04-26",
        Date = DateTime.Parse("2018-04-26"),
        Open = 93.55m,
        Close = 94.26m,
        High = 95.15m,
        Low = 93.10m,
        Volume = 41044569,
        OpenChangePercent = 93.55m.ComputeChangeRatio(93.30m),
        CloseChangePercent = 94.26m.ComputeChangeRatio(92.31m),
        HighChangePercent = 95.15m.ComputeChangeRatio(93.30m),
        LowChangePercent = 93.10m.ComputeChangeRatio(90.28m),
        VolumeChangePercent = 41044569m.ComputeChangeRatio(33729257m),
        CollectionFunction = "TIME_SERIES_DAILY"
      },
      new DailyQuote
      {
        Symbol = Msft,
        Day = "2018-04-25",
        Date = DateTime.Parse("2018-04-25"),
        Open = 93.30m,
        Close = 92.31m,
        High = 93.30m,
        Low = 90.28m,
        Volume = 33729257,
        OpenChangePercent = 0m,
        CloseChangePercent = 0m,
        HighChangePercent = 0m,
        LowChangePercent = 0m,
        VolumeChangePercent = 0m,
        CollectionFunction = "TIME_SERIES_DAILY"
      });

    private Symbol symbol = new Symbol
    {
      Id = Msft,
      DailyQuotes = DailyQuotes
    };

    #endregion

    private DbConnection dbConnection;
    private DbContextOptions<StockWatchDataContext> dbOptions;
    private Mock<IAlphaVantage> alphaVantageApi;
    private Mock<IStockWatchDataContextFactory> dataContextFactory;
    private ServiceProvider serviceProvider;
    private IServiceScope scope;

    private AlphaVantageScraper scraper;

//    [SetUp]
//    public void SetUp()
//    {
//      symbol = new Symbol { Id = Msft, DailyQuotes = DailyQuotes };
//
//      // In-memory database only exists while the connection is open
//      dbConnection = new SqliteConnection("DataSource=:memory:");
//      dbConnection.Open();
//
//      dbOptions = new DbContextOptionsBuilder<StockWatchDataContext>()
//        .UseSqlite(dbConnection)
//        .EnableSensitiveDataLogging()
//        .Options;
//
//      // Create the schema in the database
//      using (var context = CreateDataContext())
//      {
//        context.Database.EnsureCreated();
//      }
//
//      alphaVantageApi = new Mock<IAlphaVantage>();
//      alphaVantageApi.Setup(api => api.TimeseriesDaily(Msft, OutputSizes.Full))
//        .Returns(Task.FromResult(TimeseriesDailyResponse));
//
//      dataContextFactory = new Mock<IStockWatchDataContextFactory>();
//      dataContextFactory.Setup(factory => factory.CreateDataContext())
//        .Returns(CreateDataContext);
//
//      scraper = new AlphaVantageScraper(alphaVantageApi.Object, dataContextFactory.Object);
//    }

    [SetUp]
    public void SetUp()
    {
      serviceProvider = new ServiceCollection()
        .AddEntityFrameworkSqlite()
        .AddDbContext<StockWatchDataContext>(options => options.UseSqlite("DataSource=:memory:"))
//        .AddScoped<DbContext, StockWatchDataContext>()
        .BuildServiceProvider();
      scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();


      // Create the schema in the database
//      using (var db = CreateDataContext())
//      {
//        db.Database.EnsureCreated();
//      }
      var db = CreateDataContext();
      db.Database.OpenConnection();
      db.Database.EnsureCreated();
      db.SaveChanges();

      alphaVantageApi = new Mock<IAlphaVantage>();
      alphaVantageApi.Setup(api => api.TimeseriesDaily(Msft, OutputSizes.Full))
        .Returns(Task.FromResult(TimeseriesDailyResponse));

      dataContextFactory = new Mock<IStockWatchDataContextFactory>();
      dataContextFactory.Setup(factory => factory.CreateDataContext())
        .Returns(CreateDataContext);

      scraper = new AlphaVantageScraper(alphaVantageApi.Object, dataContextFactory.Object);
    }

//    [TearDown]
//    public void TearDown()
//    {
//      // Delete the database
//      using (var context = CreateDataContext())
//      {
//        DetachAllEntities(context);
//        context.Database.EnsureDeleted();
//      }
//
//      dbConnection.Close();
//      //      using (var db = CreateDataContext()  )
//      //      {
//      //        db.Database.EnsureDeleted();
//      //      }
//    }

    [TearDown]
    public void TearDown()
    {
      using (var db = CreateDataContext())
      {
        using (var s = scope)
        {
        }
      }

      // Delete the database
      //      using (var db = CreateDataContext())
      //      {
      //        db.Database.EnsureDeleted();
      //      }

      //      serviceProvider.Dispose();
      serviceProvider = null;
    }

    [Test]
    public void SimpleTest()
    {
      var context = CreateDataContext();
      var symbols = context.Symbols;
    }

    [Test]
    public void ScrapeTest_ToEmptyDb_Success()
    {
      var symbols = CreateDataContext().Symbols.ToList();
      CreateDataContext().SaveChanges();
      // Act
      scraper.Scrape(new[] {Msft}).Wait();

      // Assert
      AssertAllRecordsPresent();
    }

    [Test]
    public void ScrapeTest_ToPopulatedDb_Success()
    {
      // Insert a record in the database.
      var db = CreateDataContext();
      //using (var db = CreateDataContext())
      //{
//        DetachAllEntities(db);
      var existingDailyQuote = DailyQuotes[1];
      var existingSymbol = new Symbol
      {
        Id = Msft,
        DailyQuotes = new List<DailyQuote> {existingDailyQuote}
      };
      db.Symbols.Add(existingSymbol);
      db.SaveChanges();
      //}
      var symbols = db.Symbols;

      // Act
      scraper.Scrape(new[] {Msft}).Wait();

      // Assert
      AssertAllRecordsPresent();
    }

    [Test]
    public void ScrapeTest_ToFullDb_Success()
    {
      // Insert all records in the database.
//      using (var db = CreateDataContext())
      var db = CreateDataContext();
//      {
      db.Symbols.Add(symbol);
      db.SaveChanges();
//      }

      var symbols = db.Symbols.ToList();

      // Act
      scraper.Scrape(new[] {Msft}).Wait();

      // Assert
      AssertAllRecordsPresent();
    }

    private StockWatchDataContext CreateDataContext()
    {
      //      return new StockWatchDataContext(dbOptions);
      return serviceProvider.GetRequiredService<StockWatchDataContext>();
//      return scope.ServiceProvider.GetRequiredService<StockWatchDataContext>();
    }

    private static TimeseriesDailyResponse CreateDailyResponse()
    {
      var metadata =
        new Dictionary<string, string> {{"2. Symbol", "MSFT"}}.ToImmutableDictionary();

      var timeseries = new List<Tick>
      {
        new Tick(DateTime.Parse("2018-04-26"), 93.55m, 94.26m, 95.15m, 93.10m, 41044569),
        new Tick(DateTime.Parse("2018-04-25"), 93.30m, 92.31m, 93.30m, 90.28m, 33729257)
      }.ToImmutableList();

      return
        new TimeseriesDailyResponse(Msft, metadata, timeseries);
    }

    private void AssertAllRecordsPresent()
    {
      // Check that the database contains the objects.
      var db = CreateDataContext();
//      using (var db = CreateDataContext())
//      {
      var symbols = db.Symbols.Include(s => s.DailyQuotes).ToList();
      CollectionAssert.AreEqual(new[] {symbol}, symbols);

      var actualDailyQuotes = symbols[0].DailyQuotes.ToList();
      actualDailyQuotes.Sort((q1, q2) => String.CompareOrdinal(q2.Day, q1.Day));
      CollectionAssert.AreEquivalent(DailyQuotes, actualDailyQuotes);
//      }
    }

//    public void DetachAllEntities(DbContext db)
//    {
//      var ent = db.ChangeTracker.Entries().ToList();
//      var changedEntriesCopy = db.ChangeTracker.Entries()
//        .Where(e => e.State == EntityState.Added ||
//                    e.State == EntityState.Modified ||
//                    e.State == EntityState.Deleted)
//        .ToList();
//      foreach (var entity in changedEntriesCopy)
//      {
//        db.Entry(entity.Entity).State = EntityState.Detached;
//      }
//    }
  }
}