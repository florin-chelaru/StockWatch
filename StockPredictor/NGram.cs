using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPredictor
{
  public class Ngram
  {
    public const string Sep = ",";

    public Entry[] Entries { get; private set; }

    string hash;
    string parentHash;

    public string Hash
    {
      get
      {
        //return hash ?? (hash = string.Join(Sep, from e in Entries select Math.Max(Math.Min((int)Math.Floor(e.ChangePercent * 100.0), 2), -2)));
        return hash ?? (hash = string.Join(Sep, from e in Entries select Math.Max(Math.Min(e.ChangePercent < 0 ? (int)Math.Floor(e.ChangePercent * 100.0) : (int)Math.Ceiling(e.ChangePercent * 100.0), 3), -3)));
      }
    }

    public string ParentHash
    {
      get
      {
        //return parentHash ?? (parentHash = string.Join(Sep, from e in Entries.Take(Entries.Length - 1) select Math.Max(Math.Min((int)Math.Floor(e.ChangePercent * 100.0), 2), -2)));
        return parentHash ?? (parentHash = string.Join(Sep, from e in Entries.Take(Entries.Length - 1) select Math.Max(Math.Min(e.ChangePercent < 0 ? (int)Math.Floor(e.ChangePercent * 100.0) : (int)Math.Ceiling(e.ChangePercent * 100.0), 3), -3)));
      }
    }

    public Ngram(Entry[] entries)
    {
      Entries = entries;
    }
  }
}
