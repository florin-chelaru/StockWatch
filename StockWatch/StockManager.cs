using StockPredictor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWatch
{
  class StockManager
  {
    public int NgramSize { get; set; }

    public string Symbol { get; set; }
    
    public IList<Entry> RecentHistory { get; set; }

    //public IStockAdviser Adviser { get; set; }

    /// <summary>
    /// symbol, Ngram
    /// </summary>
    public IEnumerable<Action<string, Ngram>> Watchers { get; set; }

    public void Add(Entry e)
    {
      var last = RecentHistory.LastOrDefault();
      var newDay = last == null || last.Date.Day != e.Date.Day;
      if (newDay)
      {
        RecentHistory.Add(e);
      }
      else
      {
        RecentHistory[RecentHistory.Count - 1] = e;
      }

      if (RecentHistory.Count >= NgramSize)
      {
        var entries = new Entry[NgramSize];
        for (var j = 0; j < NgramSize; ++j)
        {
          entries[j] = RecentHistory[RecentHistory.Count - NgramSize + j];
        }
        var ngram = new Ngram(entries);

        if (Watchers != null)
        {
          foreach (var watcher in Watchers)
          {
            watcher(Symbol, ngram);
          }
        }
      }
    }
  }
}
