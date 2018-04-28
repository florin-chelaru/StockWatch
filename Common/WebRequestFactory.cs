using System.Net;

namespace Common
{
  public class WebRequestFactory : IWebRequestFactory
  {
    public IWebRequest Create(string uri)
    {
      return new WebRequestWrapper(WebRequest.Create(uri));
    }
  }
}