using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockWatchConsole
{
  class Program
  {
    static void Main(string[] args)
    {
      var watcher = new StockWatch.StockWatch(@"c:\Documents\work\stock-prediction\train aapl amzn baba fb goog msft nflx tsla yhoo znga ebay intc gpro".Split(' '));
      //var watcher = new StockWatch.StockWatch(@"c:\Documents\work\stock-prediction\train aapl amzn".Split(' '));

      watcher.StartStub();

      // Do some testing

      Thread.Sleep(1000 * 60 * 60 * 4);

      watcher.StopStub();
    }
  }
}
