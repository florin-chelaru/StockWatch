using System.Net.Cache;
using System.Threading.Tasks;

namespace Common
{
  public interface IWebRequest
  {
    RequestCachePolicy CachePolicy { get; set; }

    IWebResponse GetResponse();

    Task<IWebResponse> GetResponseAsync();
  }
}
