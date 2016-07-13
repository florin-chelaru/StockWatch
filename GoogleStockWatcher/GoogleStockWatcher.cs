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

namespace GoogleStockWatcher
{
  public partial class GoogleStockWatcher : ServiceBase
  {
    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

    int eventId;
    EmailLogger emailLogger;

    Timer timer;

    public GoogleStockWatcher()
    {
      InitializeComponent();

      eventLog = new EventLog();
      if (!EventLog.SourceExists("GoogleStockWatcherSource"))
      {
        EventLog.CreateEventSource("GoogleStockWatcherSource", "GoogleStockWatcherLog");
      }
      eventLog.Source = "GoogleStockWatcherSource";
      eventLog.Log = "GoogleStockWatcherLog";

      emailLogger = new EmailLogger(eventLog,
        // new MailAddress("florin.chelaru@gmail.com", "Florin Chelaru"),
        new MailAddress("florin@twinfog.com", "StockWatch"),
        new MailAddress[] 
        {
          new MailAddress("florin.chelaru@gmail.com", "Florin Chelaru")
        });
      emailLogger.Log("Email sending works!", "Email sending works!", EventLogEntryType.Information);
    }

    #region Event Handlers

    protected override void OnStart(string[] args)
    {
      // Update the service state to Start Pending.
      ServiceStatus serviceStatus = new ServiceStatus();
      serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
      serviceStatus.dwWaitHint = 100000;
      SetServiceStatus(this.ServiceHandle, ref serviceStatus);

      eventLog.WriteEntry("In OnStart");

      // Set up a timer to trigger every minute.
      timer = new Timer();
      timer.Interval = 5000; // 60 seconds
      timer.Elapsed += new ElapsedEventHandler(OnTimer);
      timer.Start();

      // Update the service state to Running.
      serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
      SetServiceStatus(this.ServiceHandle, ref serviceStatus);
    }

    protected override void OnStop()
    {
      eventLog.WriteEntry("In onStop.");
      if (timer.Enabled)
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

    void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
    {
      // TODO: Insert monitoring activities here.
      //eventLog.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
      try
      {
        var request = WebRequest.Create(string.Format(@"http://finance.google.com/finance/info?client=ig&q=nasdaq:{0}", "amzn"));
        using (var reader = new StreamReader(request.GetResponse().GetResponseStream()))
        {
          var text = reader.ReadToEnd().Replace("//", "").Trim();
          var obj = JArray.Parse(text);
          StockQuote quote = obj[0].ToObject<StockQuote>();

          var message = string.Format("[{0}] {1} Price: {2} Last Close Price: {3}", quote.LastTradeDateTime, quote.Symbol, quote.LastTradePrice, quote.PreviousClosePrice);
          eventLog.WriteEntry(message, EventLogEntryType.Information);
          emailLogger.Log(message, message, EventLogEntryType.Information);
        }
      }
      catch (Exception ex)
      {
        eventLog.WriteEntry(string.Format("An error occured: {0}\n{1}", ex.Message, ex.StackTrace), EventLogEntryType.Error);
        emailLogger.Log(ex.Message, string.Format("An error occured: {0}\n{1}", ex.Message, ex.StackTrace), EventLogEntryType.Error);
      }
    }

    #region Test Stubs 

    public void Start()
    {
      OnStart(new string[0]);
    }

    public void Stop()
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
