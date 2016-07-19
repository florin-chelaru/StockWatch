using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using StockPredictor;

namespace StockWatch
{
  public partial class StockWatch : ServiceBase
  {
    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

    EmailLogger emailLogger;
    
    const int NgramSize = 2;
    const int SellDelay = 3;
    const int EmergencySellDelay = 30;

    const double MaxStockInvestment = 5000.0;
    const double InvestAtATime = 1000.0;

    IDictionary<string, StockManager> stockManagers;

    Timer timer;

    public StockWatch()
    {
      InitializeComponent();

      eventLog = new EventLog();
      if (!EventLog.SourceExists("StockWatchSource"))
      {
        EventLog.CreateEventSource("StockWatchSource", "StockWatchLog");
      }
      eventLog.Source = "StockWatchSource";
      eventLog.Log = "StockWatchLog";

      emailLogger = new EmailLogger(eventLog,
        // new MailAddress("florin.chelaru@gmail.com", "Florin Chelaru"),
        new MailAddress("florin@twinfog.com", "StockWatch"),
        new MailAddress[] 
        {
          new MailAddress("florin.chelaru@gmail.com", "Florin Chelaru")
        });
    }

    #region Event Handlers

    protected override async void OnStart(string[] args)
    {
      ServiceStatus serviceStatus = new ServiceStatus();

      // Update the service state to Start Pending.
      Log("Service starting");
      serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
      serviceStatus.dwWaitHint = 100000;
      SetServiceStatus(ServiceHandle, ref serviceStatus);

      await Initialize(args);

      // Update the service state to Running.
      serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
      SetServiceStatus(this.ServiceHandle, ref serviceStatus);
      Log("Service Running");

      // Call the iterating function for the first time
      await Iterate();

      // Set up a timer to trigger periodically
      timer = new Timer();
      timer.Interval = 1000 * 60 * 60 * 1; // ms * s * m * h
      timer.Elapsed += async (sender, e) => await Iterate();
      timer.Start();
    }

    protected override void OnStop()
    {
      Log("Stopping");
      if (timer != null && timer.Enabled)
      {
        timer.Stop();
      }
    }

    protected override void OnPause()
    {
      base.OnPause();
    }

    protected override void OnContinue()
    {
      base.OnContinue();
    }

    protected override void OnCustomCommand(int command)
    {
      base.OnCustomCommand(command);
    }

    protected override void OnShutdown()
    {
      base.OnShutdown();
    }

    protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
    {
      return base.OnPowerEvent(powerStatus);
    }

    protected override void OnSessionChange(SessionChangeDescription changeDescription)
    {
      base.OnSessionChange(changeDescription);
    }

    #endregion

    async Task Initialize(string[] args)
    {
      if (args.Length == 0)
      {
        Log("No inputs for service");
        Stop();
      }

      try
      {
        var inputs = (from arg in args select arg.Split(',')).ToArray();
        stockManagers = new Dictionary<string, StockManager>();
        foreach (var input in inputs)
        {
          var mgr = new StockManager
          {
            NgramSize = NgramSize,
            Symbol = input[0],
            TrainingFilePath = input[1],
            Adviser = new BuyAdviser(Entry.FromCsvFile(input[1]), NgramSize),
            EmergencySellDelay = EmergencySellDelay,
            SellDelay = SellDelay,
            MaxInvestment = MaxStockInvestment,
            InvestAtATime = InvestAtATime
          };
          stockManagers[input[0]] = mgr;
        }

        await InitializeRecentHistory();
      }
      catch (Exception ex)
      {
        Log(ex);
      }
    }

    async Task InitializeRecentHistory()
    {
      int minDaysBack = NgramSize + 8; // Just to be on the safe side, we'll take more days than we need, to account for holidays.
      var responseTasks = new Dictionary<string, Task<WebResponse>>();
      foreach (var stock in stockManagers.Keys)
      {
        try
        {
          var today = DateTime.Now;
          var first = today.Subtract(TimeSpan.FromDays(minDaysBack));
          var query = string.Format("select * from yahoo.finance.historicaldata where symbol = \"{0}\" and startDate = \"{1}\" and endDate = \"{0}\"",
            stock, first.ToString("yyyy-MM-dd"), today.ToString("yyyy-MM-dd"));
          var url = string.Format("https://query.yahooapis.com/v1/public/yql?format=json&diagnostics=false&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys&callback=&q={0}", Uri.EscapeUriString(query));
  
          var webReq = WebRequest.Create(url);
          responseTasks[stock] = webReq.GetResponseAsync();
        }
        catch (Exception ex)
        {
          Log(ex);
        }
      }

      foreach (var tuple in responseTasks)
      {
        var stock = tuple.Key;
        var task = tuple.Value;
        try
        {
          using (var response = await task)
          {
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
              var text = reader.ReadToEnd().Trim();
              var obj = JObject.Parse(text);

              var results = (JArray)(obj["query"]["results"]["quote"]);

              var history = new List<Entry>();

              foreach (var result in results)
              {
                Entry entry = result.ToObject<Entry>();
                history.Add(entry);
              }

              history.Sort((e1, e2) => DateTime.Compare(e1.Date, e2.Date));
              stockManagers[stock].RecentHistory = history;
            }
          }
        }
        catch (Exception ex)
        {
          Log(ex);
        }
      }
    }

    async Task Iterate()
    {
      Log("Iterating");
      var responseTasks = new Dictionary<string, Task<WebResponse>>();
      foreach (var stock in stockManagers.Keys)
      {
        try
        {
          var webReq = WebRequest.Create(string.Format(@"http://finance.google.com/finance/info?client=ig&q=nasdaq:{0}", stock));
          responseTasks[stock] = webReq.GetResponseAsync();
        }
        catch (Exception ex)
        {
          Log(ex);
        }
      }

      foreach (var tuple in responseTasks)
      {
        var stock = tuple.Key;
        var task = tuple.Value;
        var mgr = stockManagers[stock];
        try
        {
          using (var response = await task)
          {
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
              var text = reader.ReadToEnd().Replace("//", "").Trim();
              var obj = JArray.Parse(text);
              StockQuote quote = obj[0].ToObject<StockQuote>();

              Entry entry = quote.ToEntry();

              mgr.Decide(entry,

                // Buy
                (nShares, investment) => 
                  Log(string.Format("Buy {0} x {1} at {2}/share for {3}", nShares, mgr.Symbol, entry.Close, investment), type: EventLogEntryType.Warning),

                // Sell
                (gainLoss) => 
                  Log(string.Format("Sell {0} x {1} at {2}/share for {3} (Gain/loss: {4})", 
                    mgr.ShareCount, mgr.Symbol, entry.Close, entry.Close * mgr.ShareCount, gainLoss), type: EventLogEntryType.Warning),
                    
                // Wait
                () => Log(string.Format("{0}: Wait", mgr.Symbol)),

                // Log
                Log);
              //var message = string.Format("[{0}] {1} Price: {2} Last Close Price: {3}", quote.LastTradeDateTime, quote.Symbol, quote.LastTradePrice, quote.PreviousClosePrice);
              //Log(message);
            }
          }
        }
        catch (Exception ex)
        {
          Log(ex);
        }
      }
    }

    #region Logging

    void Log(string title, string message = null, EventLogEntryType type = EventLogEntryType.Information)
    {
      eventLog.WriteEntry(message ?? title, type);
      emailLogger.Log(title, message, type);
    }

    void Log(Exception ex)
    {
      Log(ex.GetType().ToString(),
          string.Format("An error occured: {0}\n{1}", ex.Message, ex.StackTrace), EventLogEntryType.Error);
    }

    #endregion

    #region Test Stubs 

    public void StartStub(params string[] args)
    {
      OnStart(args);
    }

    public void StopStub()
    {
      OnStop();
    }

    #endregion
  }

  public enum ServiceState
  {
    SERVICE_STOPPED = 0x00000001,
    SERVICE_START_PENDING = 0x00000002,
    SERVICE_STOP_PENDING = 0x00000003,
    SERVICE_RUNNING = 0x00000004,
    SERVICE_CONTINUE_PENDING = 0x00000005,
    SERVICE_PAUSE_PENDING = 0x00000006,
    SERVICE_PAUSED = 0x00000007,
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct ServiceStatus
  {
    public long dwServiceType;
    public ServiceState dwCurrentState;
    public long dwControlsAccepted;
    public long dwWin32ExitCode;
    public long dwServiceSpecificExitCode;
    public long dwCheckPoint;
    public long dwWaitHint;
  };
}
