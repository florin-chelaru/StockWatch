using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;

namespace Common
{
  internal class WebRequestWrapper : IWebRequest
  {
    private readonly WebRequest request;

    public WebRequestWrapper(WebRequest request)
    {
      this.request = request;
    }

    public RequestCachePolicy CachePolicy
    {
      get => request.CachePolicy;
      set => request.CachePolicy = value;
    }

    public IWebResponse GetResponse()
    {
      return new WebResponseWrapper(request.GetResponse());
    }

    public async Task<IWebResponse> GetResponseAsync()
    {
      var response = await request.GetResponseAsync();
      return new WebResponseWrapper(response);
    }
  }
}
