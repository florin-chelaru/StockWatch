using System.Threading.Tasks;

namespace AlphaVantageApi
{
  public interface IAlphaVantage
  {
    Task<TimeseriesIntradayResponse> TimeseriesIntraday(string symbol,
      string interval = Intervals.OneMin, string outputSize = OutputSizes.Compact);

    Task<TimeseriesDailyResponse> TimeseriesDaily(string symbol, string outputSize = OutputSizes.Compact);
  }
}