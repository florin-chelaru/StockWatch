using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWatch
{
  class CompositeLogger : ILogger
  {
    IEnumerable<ILogger> loggers;

    public CompositeLogger(IEnumerable<ILogger> loggers)
    {
      this.loggers = loggers;
    }

    public void Error(string message, string title = null)
    {
      foreach (var logger in loggers)
      {
        logger.Error(message, title);
      }
    }

    public void Info(string message, string title = null)
    {
      foreach (var logger in loggers)
      {
        logger.Info(message, title);
      }
    }

    public void Warn(string message, string title = null)
    {
      foreach (var logger in loggers)
      {
        logger.Warn(message, title);
      }
    }
  }
}
