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
      var watcher = new StockWatch.StockWatch();

      watcher.Start();

      // Do some testing

      //await Task.Delay(120000);
      Thread.Sleep(120000);

      watcher.Stop();
    }
  }
}
