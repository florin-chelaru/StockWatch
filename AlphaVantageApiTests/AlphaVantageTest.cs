using Common;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AlphaVantageApi
{
  [TestFixture]
  public class AlphaVantageTest
  {
    [Test]
    public void TimeseriesIntraday_Succeeds()
    {
      // arrange
      Stream timeseriesIntradayStream = Assembly.GetExecutingAssembly()
        .GetManifestResourceStream("AlphaVantageApi.TestData.TimeseriesIntraday.json");
      var mockWebRequestFactory = CreateMockWebRequestFactory(timeseriesIntradayStream);

      // act
      var alphaVantage = new AlphaVantage("some-api-key", mockWebRequestFactory.Object);
      var response = alphaVantage.TimeseriesIntraday("msft").Result;

      // assert
      var expectedMetadata =
        new Dictionary<string, string> {{"2. Symbol", "MSFT"}}.ToImmutableDictionary();
      var expectedTimeseries = new List<Tick>
      {
        new Tick(DateTime.Parse("2018-04-26 16:00:00"), 94.59, 94.26, 94.63, 94.17, 5988325),
        new Tick(DateTime.Parse("2018-04-26 15:59:00"), 94.69, 94.60, 94.71, 94.57, 305727)
      }.ToImmutableList();

      var expectedResponse =
        new TimeseriesIntradayResponse("msft", expectedMetadata, expectedTimeseries);

      var expectedQuery =
        "https://www.alphavantage.co/query?symbol=msft&interval=1min&function=TIME_SERIES_INTRADAY&apikey=some-api-key&outputsize=compact";

      Assert.That(response, Is.EqualTo(expectedResponse));

      mockWebRequestFactory.Verify(
        c => c.Create(It.Is<string>(query => QueriesAreEquivalent(query, expectedQuery))),
        Times.Once);

      timeseriesIntradayStream?.Close();
    }

    [Test]
    public void TimeseriesDaily_Succeeds()
    {
      // arrange
      var timeseriesDailyStream = Assembly.GetExecutingAssembly()
        .GetManifestResourceStream("AlphaVantageApi.TestData.TimeseriesDaily.json");
      var mockWebRequestFactory = CreateMockWebRequestFactory(timeseriesDailyStream);

      // act
      var alphaVantage = new AlphaVantage("some-api-key", mockWebRequestFactory.Object);
      var response = alphaVantage.TimeseriesDaily("msft").Result;

      // assert
      var expectedMetadata =
        new Dictionary<string, string> {{"2. Symbol", "MSFT"}}.ToImmutableDictionary();
      
      var expectedTimeseries = new List<Tick>
      {
        new Tick(DateTime.Parse("2018-04-26"), 93.55, 94.26, 95.15, 93.10, 41044569),
        new Tick(DateTime.Parse("2018-04-25"), 93.30, 92.31, 93.30, 90.28, 33729257)
      }.ToImmutableList();

      var expectedResponse =
        new TimeseriesDailyResponse("msft", expectedMetadata, expectedTimeseries);

      var expectedQuery =
        "https://www.alphavantage.co/query?symbol=msft&function=TIME_SERIES_DAILY&apikey=some-api-key&outputsize=compact";

      Assert.That(response, Is.EqualTo(expectedResponse));

      mockWebRequestFactory.Verify(
        c => c.Create(It.Is<string>(query => QueriesAreEquivalent(query, expectedQuery))),
        Times.Once);

      timeseriesDailyStream?.Close();
    }

    private static Mock<IWebRequestFactory> CreateMockWebRequestFactory(Stream responseStream)
    {
      var mockWebResponse = new Mock<IWebResponse>();
      mockWebResponse.Setup(c => c.GetResponseStream()).Returns(responseStream);

      var mockWebRequest = new Mock<IWebRequest>();
      mockWebRequest.Setup(c => c.GetResponseAsync())
        .Returns(Task.FromResult(mockWebResponse.Object));

      var mockWebRequestFactory = new Mock<IWebRequestFactory>();
      mockWebRequestFactory.Setup(c => c.Create(It.IsAny<string>())).Returns(mockWebRequest.Object);

      return mockWebRequestFactory;
    }

    private static bool QueriesAreEquivalent(string actual, string expected)
    {
      var expectedUri = new Uri(expected);
      var actualUri = new Uri(actual);

      if (actualUri.Host != expectedUri.Host || actualUri.LocalPath != expectedUri.LocalPath)
      {
        return false;
      }

      var actualArgs = ExtractQueryArgs(actualUri);
      var expectedArgs = ExtractQueryArgs(expectedUri);

      CollectionAssert.AreEquivalent(expectedArgs, actualArgs);

      return true;
    }

    private static ImmutableSortedDictionary<string, string> ExtractQueryArgs(Uri uri)
    {
      return (from arg in uri.Query.Trim('?').Split('&')
        let parts = arg.Split('=')
        select new KeyValuePair<string, string>(parts[0], parts[1])).ToImmutableSortedDictionary();
    }
  }
}