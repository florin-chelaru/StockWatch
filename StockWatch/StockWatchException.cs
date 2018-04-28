using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWatch
{
  class StockWatchException : Exception
  {
    public StockWatchException(string message) : base(message)
    {
    }

    public StockWatchException(string message, Exception innerException) : base(message,
      innerException)
    {
    }
  }
}