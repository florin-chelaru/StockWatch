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
      var watcher = new StockWatch.StockWatch("amzn", "aapl", "goog", "tsla", "msft");

      watcher.StartStub();

      // Do some testing

      Thread.Sleep(1000 * 60 * 60 * 4);

      watcher.StopStub();
    }
  }
}
