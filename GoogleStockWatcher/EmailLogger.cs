using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GoogleStockWatcher
{
  class EmailLogger
  {
    public IEnumerable<MailAddress> Subscribers { get; set; }

    public MailAddress From { get; set; }
    
    SmtpClient client;
    EventLog eventLog;

    Task lastSend;

    public EmailLogger(EventLog eventLog, MailAddress from = null, IEnumerable<MailAddress> subscribers = null)
    {
      this.eventLog = eventLog;

      // Comcast blocks ports 25 and 565, so we have to use 3535 unsecured:
      client = new SmtpClient(@"smtpout.secureserver.net", 3535);
      client.Credentials = new NetworkCredential("contact@twinfog.com", "Filadelfia7");
      // client.EnableSsl = true;

      // This works, provided you input the correct password:
      //client = new SmtpClient("smtp.gmail.com", 587)
      //{
      //  Credentials = new NetworkCredential("florin.chelaru@gmail.com", ""),
      //  EnableSsl = true
      //};

      From = from;
      Subscribers = subscribers;
    }

    //public async void Log(string title, string message, EventLogEntryType type)
    public async void Log(string title, string message, EventLogEntryType type)
    {
      if (From == null || Subscribers == null || Subscribers.FirstOrDefault() == null) { return; }

      var email = new MailMessage
      {
        From = From,
        Subject = string.Format("[{0}] {1}", type.ToString(), title),
        SubjectEncoding = Encoding.UTF8,
        Body = message,
        BodyEncoding = Encoding.UTF8,
        IsBodyHtml = false
      };
      foreach (var address in Subscribers)
      {
        email.To.Add(address);
      }


      try
      {
        if (lastSend != null)
        {
          await lastSend;
        }

        lastSend = client.SendMailAsync(email);
        await lastSend;
        eventLog.WriteEntry(string.Format("Sent email: [{0}]", title), EventLogEntryType.Information);
      }
      catch (Exception ex)
      {
        eventLog.WriteEntry(string.Format("An error occured sending an email: {0}\n{1}", ex.Message, ex.StackTrace), EventLogEntryType.Error);
      }
    }
  }
}
