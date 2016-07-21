using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWatch
{
  class ConsoleLogger : ILogger
  {
    public void Error(string message, string title = null)
    {
      Console.Error.WriteLine("[Error] {0}", title ?? message);
      if (title != null) { Console.Error.WriteLine(message); }
    }

    public void Info(string message, string title = null)
    {
      Console.WriteLine("[Info] {0}", title ?? message);
      if (title != null) { Console.WriteLine(message); }
    }

    public void Warn(string message, string title = null)
    {
      Console.Error.WriteLine("[Warn] {0}", title ?? message);
      if (title != null) { Console.Error.WriteLine(message); }
    }
  }
}
