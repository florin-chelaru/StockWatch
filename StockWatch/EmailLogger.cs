using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace StockWatch
{
  class EmailLogger : ILogger
  {
    public IEnumerable<MailAddress> Subscribers { get; set; }

    public MailAddress From { get; set; }
    
    SmtpClient client;
    ILogger backupLogger;

    public EmailLogger(ILogger backupLogger, MailAddress from = null, IEnumerable<MailAddress> subscribers = null)
    {
      this.backupLogger = backupLogger;

      // Comcast blocks ports 25 and 565, so we have to use 3535 unsecured:
      // client = new SmtpClient(@"smtpout.secureserver.net", 3535);
      // client.Credentials = new NetworkCredential("contact@twinfog.com", "Filadelfia7");
      // client.EnableSsl = true;

      // This works, provided you input the correct password:
      client = new SmtpClient("smtp.gmail.com", 587)
      {
        Credentials = new NetworkCredential("florin.chelaru@gmail.com", "mobutusss12conguruzazabanga$"),
        EnableSsl = true
      };

      From = from;
      Subscribers = subscribers;
    }

    public void Log(string message, string title = null, string type = "Info")
    {
      if (From == null || Subscribers == null || Subscribers.FirstOrDefault() == null) { return; }

      MailMessage email;
      try
      {
        email = new MailMessage
        {
          From = From,
          Subject = string.Format("[{0}] {1}", type, title ?? message),
          SubjectEncoding = Encoding.UTF8,
          Body = message,
          BodyEncoding = Encoding.UTF8,
          IsBodyHtml = false
        };
        foreach (var address in Subscribers)
        {
          email.To.Add(address);
        }
      }
      catch (Exception ex)
      {
        backupLogger.Error(string.Format("An error occured sending an email: {0}\n{1}", ex.Message, ex.StackTrace));
        return;
      }

      try
      {
        lock (client)
        {
          client.Send(email);
        }
        backupLogger.Info(string.Format("Sent email: [{0}]", title ?? message));
      }
      catch (Exception ex)
      {
        backupLogger.Info(string.Format("An error occured sending an email: {0}\n{1}", ex.Message, ex.StackTrace));
      }
    }

    public void Info(string message, string title = null)
    {
      Log(message, title, "Info");
    }

    public void Warn(string message, string title = null)
    {
      Log(message, title, "Warn");
    }

    public void Error(string message, string title = null)
    {
      Log(message, title, "Error");
    }
  }
}
