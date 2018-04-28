using System;
using System.IO;

namespace Common
{
  public interface IWebResponse : IDisposable
  {
    Stream GetResponseStream();
  }
}
