using System;

namespace AlphaVantageApi
{
  public class AlphaVantageApiException : Exception
  {
    public AlphaVantageApiException(string message) : base(message)
    {
    }

    public AlphaVantageApiException(string message, Exception innerException) : base(message, innerException)
    {
    }
  }
}
