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

    ILogger logger;
    
    const int NgramSize = 3;

    IDictionary<string, StockManager> stockManagers;
    CompositeAdviser compositeAdviser;

    Timer timer;
    DateTime lastIterationTime = DateTime.Now.Subtract(TimeSpan.FromDays(1.0)); // yesterday

    string[] symbols;
    string trainingDataDir;

    public StockWatch(params string[] args)
    {
      InitializeComponent();

      trainingDataDir = args.Length >= 1 ? args[0] : @"c:\Documents\work\stock-prediction\train";
      symbols = args.Skip(1).ToArray();

      eventLog = new EventLog();
      if (!EventLog.SourceExists("StockWatchSource"))
      {
        EventLog.CreateEventSource("StockWatchSource", "StockWatchLog");
      }
      eventLog.Source = "StockWatchSource";
      eventLog.Log = "StockWatchLog";

      ILogger eventLogger = new EventLogger(eventLog);

      var emailLogger = new EmailLogger(eventLogger,
        // new MailAddress("florin.chelaru@gmail.com", "Florin Chelaru"),
        new MailAddress("florin@twinfog.com", "StockWatch"),
        new MailAddress[] 
        {
          new MailAddress("florin.chelaru@gmail.com", "Florin Chelaru")
        });

      logger = new CompositeLogger(new ILogger[] { eventLogger, emailLogger, new ConsoleLogger() });
      //logger = new CompositeLogger(new ILogger[] { new ConsoleLogger() });
    }

    #region Event Handlers

    protected override async void OnStart(string[] args)
    {
      ServiceStatus serviceStatus = new ServiceStatus();

      // Update the service state to Start Pending.
      logger.Info("Service starting");
      serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
      serviceStatus.dwWaitHint = 100000;
      SetServiceStatus(ServiceHandle, ref serviceStatus);

      if (args != null && args.Length > 0)
      {
        trainingDataDir = args.Length >= 1 ? args[0] : @"c:\Documents\work\stock-prediction\train";
        symbols = args.Skip(1).ToArray();
      }

      await Initialize();
      //Initialize(symbols);

      // Update the service state to Running.
      serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
      SetServiceStatus(this.ServiceHandle, ref serviceStatus);
      logger.Info("Service Running");

      // Call the iterating function for the first time
      await Iterate();

      // Set up a timer to trigger periodically
      timer = new Timer();
      timer.Interval = 1000 * 60 * 60 * 3; // ms * s * m * h
      timer.Elapsed += async (sender, e) => await Iterate();
      timer.Start();
    }

    protected override void OnStop()
    {
      logger.Info("Stopping");
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

    async Task Initialize()
    {
      if (symbols == null || symbols.Length == 0)
      {
        logger.Error("No inputs for service");
        Stop();
      }

      var watchers = new Action<string, Ngram>[]
      {
        (symbol, ngram) =>
        {
          var advice = compositeAdviser.Predict(ngram);
          var title = string.Format("[{0}] Positive Chance: {1:0.00}% Prediction: {2:0.000}% (Confidence: {3:0.000}%)", 
            symbol, advice.PositiveChangeChance * 100.0, advice.Prediction * 100.0, advice.Confidence*100.0);

          var message = new StringBuilder();
          message.Append(string.Format("Statistics for {0} and today's ngram, {1} ({2} | {3}) ({4}-{5}):\n\nOverall chance of positive change: {6:0.00}%\n\nOverall average change: {7:0.000}%, {8}/{9}\n\n",
            symbol,
            ngram.Hash,
            string.Join(",", from e in ngram.Entries select string.Format("{0:0.00}", e.AdjClose)),
            string.Join(",", from e in ngram.Entries select string.Format("{0:0.000}%", e.ChangePercent * 100.0)),
            ngram.Entries[0].Date.ToString("MM/dd"),
            ngram.Entries[ngram.Entries.Length - 1].Date.ToString("MM/dd"),
            advice.PositiveChangeChance * 100.0,
            advice.Prediction * 100.0, 
            advice.Confidence * (compositeAdviser.Count - compositeAdviser.NgramSize), 
            compositeAdviser.Count - compositeAdviser.NgramSize));
          message.Append(string.Format("Ngram {0} found {1} times in a {2} corpus.\n\n", ngram.Hash, compositeAdviser.NgramCount(ngram.Hash), compositeAdviser.Count - compositeAdviser.NgramSize));

          message.Append(string.Format("Possible combinations:\n\n"));

          var relevantPairs = new List<KeyValuePair<string, int>>();
          foreach (var p in compositeAdviser.NgramCounts)
          {
            if (p.Key.StartsWith(ngram.Hash))
            {
              relevantPairs.Add(p);
            }
          }
          relevantPairs.Sort((p1, p2) => p2.Value - p1.Value);
          foreach (var p in relevantPairs)
          {
            message.Append(string.Format("{0}: {1}/{2} ({3:0.000}%)\n", p.Key, p.Value, compositeAdviser.NgramCount(ngram.Hash), (double)p.Value / compositeAdviser.NgramCount(ngram.Hash) * 100.0));
          }

          message.Append("\n\nWhat other advisers think:\n\n");
          foreach (BuyAdviser a in compositeAdviser.Advisers)
          {
            var adv = a.Predict(ngram);
            message.Append(string.Format("A[{0}]: {1:0.000}%, {2}/{3}; Pos-chance: {4:0.00}%\n", a.Symbol, adv.Prediction * 100.0, adv.Confidence * (a.Count - a.NgramSize), a.Count - a.NgramSize, adv.PositiveChangeChance * 100.0));

            IDictionary<string, IList<Ngram>> options;
            if (a.PredictionNgrams.TryGetValue(ngram.Hash, out options))
            {
              relevantPairs = new List<KeyValuePair<string, int>>();
              foreach (var p in options)
              {
                relevantPairs.Add(new KeyValuePair<string, int>(p.Key, p.Value.Count));
              }
              relevantPairs.Sort((p1, p2) => p2.Value - p1.Value);
              foreach (var p in relevantPairs)
              {
                message.Append(string.Format("A[{0}]: {1}: {2} ({3:0.000}%)\n", a.Symbol, p.Key, p.Value, (double)p.Value / a.NgramCount(ngram.Hash) * 100.0));
              }
            }
            message.Append("\n\n");
          }

          logger.Info(message.ToString(), title);
        }
      };

      try
      {
        var stockHistories = new StockScraper(logger).GetCachedStocksHistories(symbols, new DirectoryInfo(trainingDataDir));
        //var stockHistories = await new StockScraper(logger).GetStocksHistories(args, new DateTime(2010, 1, 1), DateTime.Now);
        //var stockHistories = await new StockScraper(logger).GetStocksHistories(args, new DateTime(2016, 1, 1), DateTime.Now);

        stockManagers = new Dictionary<string, StockManager>();
        var advisers = new List<IStockAdviser>();
        foreach (var symbol in stockHistories.Keys)
        {
          // Get recent history
          IList<Entry> recentHistory = null;
          var error = false;
          var yesterday = DateTime.Today.Subtract(TimeSpan.FromDays(1));
          var startDay = yesterday.Subtract(TimeSpan.FromDays(NgramSize + 8));
          do
          {
            error = false;
            try { recentHistory = await new StockScraper(logger).GetStockSmallHistory(symbol, startDay, yesterday);}
            catch (Exception ex)
            {
              logger.Warn(string.Format("Unable to get recent history for {0}. Trying again. Details: {1}", symbol, ex.Message));
              error = true;
              await Task.Delay(30000);
            }
          } while (error);

          advisers.Add(new BuyAdviser(symbol, stockHistories[symbol], NgramSize));
          var mgr = new StockManager
          {
            NgramSize = NgramSize,
            Symbol = symbol,
            RecentHistory = recentHistory,//stockHistories[symbol].Skip(Math.Max(0, stockHistories[symbol].Count - NgramSize)).ToList(),
            Watchers = watchers
          };
          stockManagers[symbol] = mgr;
        }

        compositeAdviser = new CompositeAdviser(advisers, NgramSize);
      }
      catch (Exception ex)
      {
        Error(ex);
      }
    }

    async Task Iterate()
    {
      if (!IsWithinMarketHours) { return; }

      logger.Info("Iterating");

      var quotes = await new StockScraper(logger).GetRealTimeQuotes(stockManagers.Keys);

      foreach (var p in quotes)
      {
        stockManagers[p.Key].Add(p.Value);
      }
    }

    bool IsFirstIterationOfDay
    {
      get
      {
        var now = DateTime.Now;
        var ret = now.Subtract(lastIterationTime) >= TimeSpan.FromDays(1.0) || now.Day != lastIterationTime.Day;
        lastIterationTime = now;
        return ret;
      }
    }

    bool IsWithinMarketHours
    {
      get
      {
        var timeUtc = DateTime.UtcNow;
        TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        DateTime now = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
        return ((now.Hour == 9 && now.Minute > 30) || now.Hour >= 10) && (now.Hour <= 16);
      }
    }

    #region Logging

    void Error(Exception ex)
    {
      logger.Error(
        string.Format("An error occured: {0}\n{1}", ex.Message, ex.StackTrace),
        ex.GetType().ToString());
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
