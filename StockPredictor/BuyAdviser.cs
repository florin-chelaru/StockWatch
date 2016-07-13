using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPredictor
{
  class BuyAdviser
  {
    const string Sep = ",";

    List<Entry> history = new List<Entry>();

    IDictionary<string, IList<Ngram>> ngrams = new Dictionary<string, IList<Ngram>>();

    // parentHash -> hash -> ngrams
    IDictionary<string, IDictionary<string, IList<Ngram>>> predictionNgrams = new Dictionary<string, IDictionary<string, IList<Ngram>>>();

    Ngram last;

    Advice lastAdvice;

    public int NgramSize { get; private set; }

    public int Count { get { return history.Count; } }

    public BuyAdviser(IEnumerable<Entry> history = null, int ngramSize = 2)
    {
      this.NgramSize = ngramSize;
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
        if (lastAdvice != null) { return lastAdvice; }

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
        if (!ngrams.TryGetValue(ngram.Hash, out hashNgrams))
        {
          hashNgrams = new List<Ngram>();
          ngrams[ngram.Hash] = hashNgrams;
        }
        hashNgrams.Add(ngram);
        last = ngram;
      }

      if (history.Count >= NgramSize + 1)
      {
        Entry[] entries = new Entry[NgramSize+1];
        history.CopyTo(history.Count - NgramSize - 1, entries, 0, NgramSize + 1);
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
      }
    }

    public Advice Predict(Ngram ngram)
    {
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

      // Compute average of all options
      double sum = 0.0;
      int count = 0;
      // Also compute the median
      var changes = new List<double>();
      foreach (var ngrams in options.Values)
      {
        foreach (var g in ngrams)
        {
          sum += g.Entries[g.Entries.Length - 1].ChangePercent;
          ++count;

          changes.Add(g.Entries[g.Entries.Length - 1].ChangePercent);
        }
      }
      if (count == 0.0)
      {
        return new Advice();
      }

      changes.Sort();
      var i1 = changes.Count / 2;
      var i2 = (changes.Count - 1) / 2;

      return new Advice
      {
        PredictionMedian = (changes[i1] + changes[i2]) / 2.0,
        PredictionMean = sum / count,
        Confidence = count / (double)(history.Count - NgramSize)
      };
    }
  }
}
