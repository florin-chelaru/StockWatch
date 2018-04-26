using System.Diagnostics;

namespace StockWatch
{
  class EventLogger : ILogger
  {
    EventLog log;

    public EventLogger(EventLog log)
    {
      this.log = log;
    }

    public void Error(string message, string title = null)
    {
      string all = (title ?? "") + (title == null ? "" : " - ") + message;
      log.WriteEntry(all, EventLogEntryType.Error);
    }

    public void Info(string message, string title = null)
    {
      string all = (title ?? "") + (title == null ? "" : " - ") + message;
      log.WriteEntry(all, EventLogEntryType.Information);
    }

    public void Warn(string message, string title = null)
    {
      string all = (title ?? "") + (title == null ? "" : " - ") + message;
      log.WriteEntry(all, EventLogEntryType.Warning);
    }
  }
}
