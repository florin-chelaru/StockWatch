namespace Common
{
  public interface IWebRequestFactory
  {
    IWebRequest Create(string uri);
  }
}