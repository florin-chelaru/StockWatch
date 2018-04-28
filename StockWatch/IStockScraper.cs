using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StockPredictor;

namespace StockWatch
{
  public interface IStockScraper
  {
    IDictionary<string, IList<Entry>> GetCachedTimeseries(
      ICollection<string> symbols, DirectoryInfo inDir);

    Task<Entry> GetQuote(string symbol, string market = "nasdaq");

    Task<IDictionary<string, Entry>> GetQuotes(
      ICollection<string> symbols, string market = "nasdaq");

    Task<IList<Entry>> GetTimeseries(string symbol, DateTime start,
      DateTime? end = null);

    Task<IDictionary<string, IList<Entry>>> GetTimeseries(
      ICollection<string> symbols, DateTime start, DateTime? end = null);

    Task<IList<Entry>> GetSmallTimeseries(string symbol, DateTime start,
      DateTime end);

    Task<IList<Entry>> GetTimeseriesByYear(string symbol, int year);
  }
}