using System;
using System.IO;
using System.Net;

namespace Common
{
  // From https://stackoverflow.com/questions/9823039/is-it-possible-to-mock-out-a-net-httpwebresponse
  internal class WebResponseWrapper : IWebResponse
  {
    private WebResponse response;

    public WebResponseWrapper(WebResponse response)
    {
      this.response = response;
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (response != null)
        {
          ((IDisposable) response).Dispose();
          response = null;
        }
      }
    }

    public Stream GetResponseStream()
    {
      return response.GetResponseStream();
    }
  }
}