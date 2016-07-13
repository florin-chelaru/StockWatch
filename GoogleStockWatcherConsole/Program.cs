using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleStockWatcherConsole
{
  class Program
  {
    static void Main(string[] args)
    {
      var watcher = new GoogleStockWatcher.GoogleStockWatcher();

      watcher.Start();

      // Do some testing

      //await Task.Delay(120000);
      Thread.Sleep(120000);

      watcher.Stop();
    }
  }
}
