using System;

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