using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPredictor
{
  public class Entry
  {
    public string Symbol { get; set; }
    public DateTime Date { get; set; }
    public double Open { get; set; }
    public double Close { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    
    [JsonProperty("Adj_Close")]
    public double AdjClose { get; set; }
    public int Volume { get; set; }

    [JsonIgnore]
    public double Change { get; set; }

    [JsonIgnore]
    public double ChangePercent { get; set; }

    public Entry Copy()
    {
      return new Entry
      {
        Symbol = Symbol,
        Date = Date,
        Open = Open,
        Close = Close,
        High = High,
        Low = Low,
        AdjClose = AdjClose,
        Volume = Volume,
        Change = Change,
        ChangePercent = ChangePercent
      };
    }

    public static Entry ParseCsv(string csv)
    {
      var tokens = csv.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

      var e = new Entry
      {
        Symbol = tokens[0],
        Date = DateTime.Parse(tokens[1]),
        Open = double.Parse(tokens[2]),
        Close = double.Parse(tokens[3]),
        High = double.Parse(tokens[4]),
        Low = double.Parse(tokens[5]),
        AdjClose = double.Parse(tokens[6]),
        Volume = int.Parse(tokens[7])
      };

      if (tokens.Length > 8)
      {
        e.Change = double.Parse(tokens[8]);
        e.ChangePercent = double.Parse(tokens[9]);
      }

      return e;
    }

    public string ToCsv()
    {
      return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},", Symbol, Date.ToShortDateString(), Open, Close, High, Low, AdjClose, Volume, Change, ChangePercent);
    }

    public static Entry[] FromCsvFile(string path)
    {
      using (var reader = new StreamReader(path))
      {
        var entries = (from line in reader.ReadToEnd().Trim().Split('\n') select Entry.ParseCsv(line)).ToArray();
        entries[0].Change = entries[0].Close - entries[0].Open;
        entries[0].ChangePercent = entries[0].Change / entries[0].Open;

        for (var i = 1; i < entries.Length; ++i)
        {
          entries[i].Change = entries[i].Close - entries[i - 1].Close;
          entries[i].ChangePercent = entries[i].Change / entries[i - 1].Close;
        }

        return entries;
      }
    }
  }
}
