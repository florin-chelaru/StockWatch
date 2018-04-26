using System.Collections.Generic;
using System.Linq;

namespace StockPredictor
{
  public class BuyAdvisor : IStockAdvisor
  {
    readonly List<Entry> history = new List<Entry>();

    readonly IDictionary<string, IList<Ngram>> parentNgrams =
      new Dictionary<string, IList<Ngram>>();

    // parentHash -> hash -> ngrams
    readonly IDictionary<string, IDictionary<string, IList<Ngram>>>
      predictionNgrams =
        new Dictionary<string, IDictionary<string, IList<Ngram>>>();

    Ngram last;
    Advice lastAdvice;
    readonly AggregateMethod aggregate;

    public string Symbol { get; private set; }

    public int NgramSize { get; private set; }

    public int Count => history.Count;

    public IDictionary<string, IDictionary<string, IList<Ngram>>>
      PredictionNgrams => predictionNgrams;

    public IDictionary<string, int> NgramCounts { get; private set; } =
      new Dictionary<string, int>();

    public IDictionary<string, int> ParentNgramCounts { get; private set; } =
      new Dictionary<string, int>();

    // Random rand = new Random();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="history"></param>
    /// <param name="ngramSize"></param>
    /// <param name="aggregate">The default will be average</param>
    public BuyAdvisor(string symbol, IEnumerable<Entry> history = null,
      int ngramSize = 2, AggregateMethod aggregate = null)
    {
      Symbol = symbol;
      this.aggregate = aggregate ?? AggregateMethod.Average;
      NgramSize = ngramSize;
      if (history != null)
      {
        foreach (var e in history)
        {
          Add(e);
        }
      }
    }

    public Advice Advice
    {
      get
      {
        if (lastAdvice != null)
        {
          return lastAdvice;
        }

        lastAdvice = Predict(last);

        return lastAdvice;
      }
    }

    public void Add(Entry entry)
    {
      lastAdvice = null;

      var e = entry.Copy();
      if (history.Count == 0)
      {
        e.Change = e.Close - e.Open;
        e.ChangePercent = e.Change / e.Open;
      }
      else
      {
        e.Change = e.Close - history[history.Count - 1].Close;
        e.ChangePercent = e.Change / history[history.Count - 1].Close;
      }

      history.Add(e);

      if (history.Count >= NgramSize)
      {
        Entry[] entries = new Entry[NgramSize];
        history.CopyTo(history.Count - NgramSize, entries, 0, NgramSize);
        var ngram = new Ngram(entries);

        IList<Ngram> hashNgrams;
        if (!parentNgrams.TryGetValue(ngram.Hash, out hashNgrams))
        {
          hashNgrams = new List<Ngram>();
          parentNgrams[ngram.Hash] = hashNgrams;
        }

        hashNgrams.Add(ngram);
        last = ngram;
      }

      if (history.Count >= NgramSize + 1)
      {
        Entry[] entries = new Entry[NgramSize + 1];
        history.CopyTo(history.Count - NgramSize - 1, entries, 0,
          NgramSize + 1);
        var ngram = new Ngram(entries);

        IDictionary<string, IList<Ngram>> childNgrams;
        IList<Ngram> hashNgrams;
        if (!predictionNgrams.TryGetValue(ngram.ParentHash, out childNgrams))
        {
          childNgrams = new Dictionary<string, IList<Ngram>>();
          predictionNgrams[ngram.ParentHash] = childNgrams;
        }

        if (!childNgrams.TryGetValue(ngram.Hash, out hashNgrams))
        {
          hashNgrams = new List<Ngram>();
          childNgrams[ngram.Hash] = hashNgrams;
        }

        hashNgrams.Add(ngram);

        int parentNgramCount;
        if (!ParentNgramCounts.TryGetValue(ngram.ParentHash,
          out parentNgramCount))
        {
          parentNgramCount = 0;
        }

        ParentNgramCounts[ngram.ParentHash] = parentNgramCount + 1;

        int ngramCount;
        if (!NgramCounts.TryGetValue(ngram.Hash, out ngramCount))
        {
          ngramCount = 0;
        }

        NgramCounts[ngram.Hash] = ngramCount + 1;
      }
    }

    public Advice Predict(Ngram ngram)
    {
      // TODO: Remove. Just testing to see whether random does the same...
      //return new Advice
      //{
      //  PredictionMean = rand.NextDouble() * 2.0 - 1.0,
      //  Confidence = 0.5
      //};

      if (ngram == null)
      {
        return new Advice();
      }

      IDictionary<string, IList<Ngram>> options;
      if (!predictionNgrams.TryGetValue(ngram.Hash, out options))
      {
        // This ngram has never been encountered before. 
        return new Advice();
      }

      // Make a list of all options
      var changes = new List<double>();
      foreach (var ngrams in options.Values)
      {
        foreach (var g in ngrams)
        {
          changes.Add(g.Entries[g.Entries.Length - 1].ChangePercent);
        }
      }

      return new Advice
      {
        Prediction = aggregate.Aggregate(changes),
        Confidence = changes.Count / (double) (history.Count - NgramSize),
        PositiveChangeChance =
          changes.Where(c => c >= 0).Count() / (double) changes.Count
      };
    }

    public int NgramCount(Ngram ngram)
    {
      IDictionary<string, int> counts = ngram.Entries.Length == NgramSize + 1
        ? NgramCounts
        : ParentNgramCounts;
      int ret;
      return counts.TryGetValue(ngram.Hash, out ret) ? ret : 0;
    }

    public int NgramCount(string hash)
    {
      int ret;
      return NgramCounts.TryGetValue(hash, out ret)
        ? ret
        : (ParentNgramCounts.TryGetValue(hash, out ret) ? ret : 0);
    }
  }
}