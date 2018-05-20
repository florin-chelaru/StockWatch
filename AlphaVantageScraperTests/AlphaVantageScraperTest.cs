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
    private const string Group = "mygroup";
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

    #endregion

    private DbConnection dbConnection;

    private Mock<IAlphaVantage> alphaVantageApi;
    private Mock<IStockWatchDataContextFactory> dataContextFactory;
    private StockWatchDataContext db;

    private AlphaVantageScraper scraper;

    private static readonly Symbol Symbol = new Symbol { Id = Msft, DailyQuotes = DailyQuotes, Tags = $"{{{Group}}}" };

    [SetUp]
    public void SetUp()
    {
      dbConnection = new SqliteConnection("DataSource=:memory:");
      dbConnection.Open();
      var dbOptions = new DbContextOptionsBuilder<StockWatchDataContext>()
        .UseSqlite(dbConnection)
        .EnableSensitiveDataLogging()
        .Options;

      // Create the schema in the database
      db = new StockWatchDataContext(dbOptions);
      db.Database.EnsureCreated();

      alphaVantageApi = new Mock<IAlphaVantage>();
      alphaVantageApi.Setup(api => api.TimeseriesDaily(Msft, OutputSizes.Full))
        .Returns(Task.FromResult(CreateDailyResponse()));

      dataContextFactory = new Mock<IStockWatchDataContextFactory>();
      dataContextFactory.Setup(factory => factory.DataContext).Returns(db);

      scraper = new AlphaVantageScraper(alphaVantageApi.Object, dataContextFactory.Object);
    }

    [TearDown]
    public void TearDown()
    {
      dbConnection.Close();
    }

    [Test]
    public async Task ScrapeTest_ToEmptyDb_Success()
    {
      // Clear database
      db.Database.EnsureDeleted();
      db.Database.EnsureCreated();

      // Act
      await scraper.Scrape(new[] {Msft}, Group);

      // Assert
      AssertAllRecordsPresent();
    }

    [Test]
    public async Task ScrapeTest_ToPopulatedDb_Success()
    {
      // Clear database
      db.Database.EnsureDeleted();
      db.Database.EnsureCreated();

      // Insert a record in the database.
      var existingDailyQuote = DailyQuotes[1];
      db.Symbols.Add(new Symbol {Id = Msft});
      db.DailyQuotes.Add(existingDailyQuote);
      db.SaveChanges();

      // Act
      await scraper.Scrape(new[] {Msft}, Group);

      // Assert
      AssertAllRecordsPresent();
    }

    [Test]
    public async Task ScrapeTest_ToFullDb_Success()
    {
      // Clear database
      db.Database.EnsureDeleted();
      db.Database.EnsureCreated();

      db.Symbols.Add(new Symbol {Id = Msft});
      db.DailyQuotes.AddRange(DailyQuotes);
      db.SaveChanges();

      // Act
      await scraper.Scrape(new[] {Msft}, Group);

      // Assert
      AssertAllRecordsPresent();
    }

    private TimeseriesDailyResponse CreateDailyResponse()
    {
      var metadata =
        new Dictionary<string, string> {{"2. Symbol", Msft.ToUpperInvariant()}}
          .ToImmutableDictionary();

      var timeseries = new List<Tick>
      {
        new Tick(DateTime.Parse("2018-04-26"), 93.55m, 94.26m, 95.15m, 93.10m, 41044569),
        new Tick(DateTime.Parse("2018-04-25"), 93.30m, 92.31m, 93.30m, 90.28m, 33729257)
      }.ToImmutableList();

      return new TimeseriesDailyResponse(Msft, metadata, timeseries);
    }

    private void AssertAllRecordsPresent()
    {
      // Check that the database contains the objects.
      var symbols = db.Symbols.Include(s => s.DailyQuotes).Include(s => s.SymbolGroupMemberships)
        .ToList();
      CollectionAssert.AreEqual(new[] {Symbol}, symbols);

      var actualDailyQuotes = symbols[0].DailyQuotes.ToList();
      actualDailyQuotes.Sort((q1, q2) => String.CompareOrdinal(q2.Day, q1.Day));
      CollectionAssert.AreEquivalent(DailyQuotes, actualDailyQuotes);

      var actualGroups = symbols[0].Groups.Select(g => g.Id).ToArray();
      CollectionAssert.AreEqual(new[] {Group}, actualGroups);
    }
  }
}