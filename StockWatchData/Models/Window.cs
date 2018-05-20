using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace StockWatchData.Models
{
  public class Window
  {
    public string Symbol { get; set; }
    public string DayOne { get; set; }
    public short PastSize { get; set; }
    public short FutureSize { get; set; }
    public string Content { get; set; }
    public Symbol SymbolNavigation { get; set; }

    private WindowContent unpackedContent;

    [NotMapped]
    public WindowContent UnpackedContent
    {
      get => unpackedContent ??
             (unpackedContent =
               JsonConvert.DeserializeObject<WindowContent>(Content)
             );
      set
      {
        unpackedContent = value;
        Content = JsonConvert.SerializeObject(value, Formatting.None);
      }
    }

    public class WindowContent
    {
      public string[] PastDays { get; set; }
      public decimal[] PastValues { get; set; }
      public string[] FutureDays { get; set; }
      public decimal[] FutureValues { get; set; }
    }
  }
}