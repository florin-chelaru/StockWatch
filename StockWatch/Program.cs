using System.ServiceProcess;

namespace StockWatch
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static void Main(string[] args)
    {
      var servicesToRun = new ServiceBase[] { new StockWatch(args) };
      ServiceBase.Run(servicesToRun);
    }
  }
}
