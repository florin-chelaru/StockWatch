using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using AlphaVantageApi;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using StockWatchData.Models;

namespace AlphaVantageScraper
{
  [TestFixture]
  public class AlphaVantageScraperTest
  {
    private const string Msft = "msft";
    private static readonly TimeseriesDailyResponse TimeseriesDailyResponse = CreateDailyResponse();

    private static readonly Symbol Symbol = new Symbol
    {
      Id = Msft,
      DailyQuotes = new List<DailyQuote>
      {
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
          OpenChangePercent = ChangePercent(93.55m, 93.30m),
          CloseChangePercent = ChangePercent(94.26m, 92.31m),
          HighChangePercent = ChangePercent(95.15m, 93.30m),
          LowChangePercent = ChangePercent(93.10m, 90.28m),
          VolumeChangePercent = ChangePercent(41044569m, 33729257m),
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
        }
      }
    };

    private DbConnection dbConnection;
    private DbContextOptions<StockWatchDataContext> options;

    [SetUp]
    public void SetUp()
    {
      // In-memory database only exists while the connection is open
      dbConnection = new SqliteConnection("DataSource=:memory:");
      dbConnection.Open();

      options = new DbContextOptionsBuilder<StockWatchDataContext>()
        .UseSqlite(dbConnection)
        .Options;

      // Create the schema in the database
      using (var context = new StockWatchDataContext(options))
      {
        context.Database.EnsureCreated();
      }
    }

    [TearDown]
    public void TearDown()
    {
      dbConnection.Close();
    }

    [Test]
    public void ScrapeTest()
    {
      var alphaVantageApi = new Mock<IAlphaVantage>();
      alphaVantageApi.Setup(api => api.TimeseriesDaily(Msft, OutputSizes.Full))
        .Returns(Task.FromResult(TimeseriesDailyResponse));

      var dataContextFactory = new Mock<IStockWatchDataContextFactory>();
      dataContextFactory.Setup(factory => factory.CreateDataContext())
        .Returns(new StockWatchDataContext(options));

      var scraper = new AlphaVantageScraper(alphaVantageApi.Object, dataContextFactory.Object);

      scraper.Scrape(new[] {Msft}).Wait();

      var expectedSymbols = new[] {Symbol};
      var expectedDailyQuotes = Symbol.DailyQuotes.ToList();
      expectedDailyQuotes.Sort((q1, q2) => String.CompareOrdinal(q1.Day, q2.Day));

      // Check that the database contains the objects.
      using (var db = new StockWatchDataContext(options))
      {
        var symbols = db.Symbols.Include(s => s.DailyQuotes).ToList();
        CollectionAssert.AreEqual(expectedSymbols, symbols);

        var actualDailyQuotes = symbols[0].DailyQuotes.ToList();
        actualDailyQuotes.Sort((q1, q2) => String.CompareOrdinal(q1.Day, q2.Day));
        actualDailyQuotes.ForEach(q =>
        {
          q.OpenChangePercent = decimal.Round(q.OpenChangePercent, 3);
          q.CloseChangePercent = decimal.Round(q.CloseChangePercent, 3);
          q.HighChangePercent = decimal.Round(q.HighChangePercent, 3);
          q.LowChangePercent = decimal.Round(q.LowChangePercent, 3);
          q.VolumeChangePercent = decimal.Round(q.VolumeChangePercent, 3);
        });
        CollectionAssert.AreEquivalent(expectedDailyQuotes, actualDailyQuotes);
      }
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

    private static decimal ChangePercent(decimal current, decimal previous)
    {
      return decimal.Round((current / previous) * 100m - 100m, 3);
    }
  }
}