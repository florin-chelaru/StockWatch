using System;

namespace StockPredictor
{
  class State
  {
    public Entry[] Entries { get; }

    public string Hash { get; }

    public string ParentHash => Hash.Substring(0,
      Hash.LastIndexOf(Sep, StringComparison.Ordinal));

    public State(Entry[] entries, string hash)
    {
      Entries = entries;
      Hash = hash;
    }

    public static string Sep { get; set; } = " | ";

    public static int Size { get; set; } = 1;


    public static Func<Entry[], string> HashFunction { get; set; }

    public static State Create(Entry[] rawEntries, int index)
    {
      if (HashFunction == null)
      {
        throw new Exception("HashFunction not defined");
      }

      var entries = new Entry[Size];
      for (int i = 0; i < Size && i + index < rawEntries.Length; ++i)
      {
        entries[i] = rawEntries[i + index];
      }

      return new State(entries, HashFunction(entries));
    }

    public static State CreateWithDelay(Entry[] rawEntries, int index,
      int delay)
    {
      if (HashFunction == null)
      {
        throw new Exception("HashFunction not defined");
      }

      var entries = new Entry[Size];
      for (int i = 0; i < Size - 1 && i + index < rawEntries.Length; ++i)
      {
        entries[i] = rawEntries[i + index];
      }

      var last = rawEntries[index + Size - 1 + delay].Copy();
      last.Change = last.Close - entries[Size - 2].Close;
      last.ChangePercent = last.Change / entries[Size - 2].Close;
      entries[Size - 1] = last;

      return new State(entries, HashFunction(entries));
    }
  }
}