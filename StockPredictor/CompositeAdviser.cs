using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPredictor
{
  public class CompositeAdviser : IStockAdviser
  {
    IEnumerable<IStockAdviser> advisers;

    public CompositeAdviser(IEnumerable<IStockAdviser> advisers, int ngramSize = 2)
    {
      this.advisers = advisers;

      Count = (from a in advisers select a.Count).Sum();
      NgramSize = ngramSize;

      foreach (var adviser in advisers)
      {
        foreach (var p in adviser.NgramCounts)
        {
          int count;
          if (!NgramCounts.TryGetValue(p.Key, out count))
          {
            count = 0;
          }
          NgramCounts[p.Key] = count + p.Value;
        }

        foreach (var p in adviser.ParentNgramCounts)
        {
          int count;
          if (!ParentNgramCounts.TryGetValue(p.Key, out count))
          {
            count = 0;
          }
          ParentNgramCounts[p.Key] = count + p.Value;
        }
      }
    }

    public IEnumerable<IStockAdviser> Advisers { get { return advisers; } }

    public int Count { get; private set; }

    public int NgramSize { get; private set; }

    public IDictionary<string, int> NgramCounts { get; private set; } = new Dictionary<string, int>();

    public IDictionary<string, int> ParentNgramCounts { get; private set; } = new Dictionary<string, int>();

    public int NgramCount(Ngram ngram)
    {
      IDictionary<string, int> counts = ngram.Entries.Length == NgramSize + 1 ? NgramCounts : ParentNgramCounts;
      int ret;
      return counts.TryGetValue(ngram.Hash, out ret) ? ret : 0;
    }

    public int NgramCount(string hash)
    {
      int ret;
      return NgramCounts.TryGetValue(hash, out ret) ? ret :
        (ParentNgramCounts.TryGetValue(hash, out ret) ? ret : 0);
    }

    public Advice Predict(Ngram ngram)
    {
      int ngramCount = NgramCount(ngram);

      if (ngramCount == 0) { return new Advice(); }

      var data = from adviser in advisers select new { Count = adviser.Count - adviser.NgramSize, Advice = adviser.Predict(ngram) };

      return new Advice
      {
        Prediction = (from o in data select o.Advice.Prediction * o.Advice.Confidence * o.Count).Sum() / ngramCount,
        Confidence = (from o in data select o.Advice.Confidence * o.Count).Sum() / (Count - NgramSize),
        PositiveChangeChance = (from o in data select o.Advice.PositiveChangeChance * o.Advice.Confidence * o.Count).Sum() / ngramCount
      };
    }
  }
}
