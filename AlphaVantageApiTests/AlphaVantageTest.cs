using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common;
using Moq;
using NUnit.Framework;

namespace AlphaVantageApi
{
  [TestFixture]
  public class AlphaVantageTest
  {
    private Stream timeseriesIntradayStream;

    [SetUp]
    public void SetUp()
    {
      var assembly = Assembly.GetExecutingAssembly();
      timeseriesIntradayStream =
        assembly.GetManifestResourceStream("AlphaVantageApi.TestData.TimeseriesIntraday.json");
    }

    [TearDown]
    public void TearDown()
    {
      timeseriesIntradayStream.Close();
    }

    [Test]
    public void TimeseriesIntraday_Succeeds()
    {
      // arrange
      var mockWebResponse = new Mock<IWebResponse>();
      mockWebResponse.Setup(c => c.GetResponseStream()).Returns(timeseriesIntradayStream);

      var mockWebRequest = new Mock<IWebRequest>();
      mockWebRequest.Setup(c => c.GetResponseAsync()).Returns(Task.FromResult(mockWebResponse.Object));

      var mockWebRequestFactory = new Mock<IWebRequestFactory>();
      mockWebRequestFactory.Setup(c => c.Create(It.IsAny<string>())).Returns(mockWebRequest.Object);

      // act
      var alphaVantage = new AlphaVantage("some-api-key", mockWebRequestFactory.Object);
      var response = alphaVantage.TimeseriesIntraday("msft").Result;

      var metadata = new Dictionary<string, string> {{"2. Symbol", "MSFT"}}.ToImmutableDictionary();
      var timeseries = new List<AlphaVantage.Tick>
      {
        new AlphaVantage.Tick(DateTime.Parse("2018-04-26 16:00:00"), 94.59, 94.26, 94.63, 94.17, 5988325),
        new AlphaVantage.Tick(DateTime.Parse("2018-04-26 15:59:00"), 94.69, 94.60, 94.71, 94.57, 305727)
      }.ToImmutableList();

      var expectedResponse =
        new AlphaVantage.TimeseriesIntradayResponse("msft", metadata, timeseries);

      // assert
      
      Assert.AreEqual(expectedResponse, response);
    }
  }
}