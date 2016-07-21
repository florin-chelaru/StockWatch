using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWatch
{
  public interface ILogger
  {
    void Info(string message, string title = null);

    void Warn(string message, string title = null);

    void Error(string message, string title = null);
  }
}
