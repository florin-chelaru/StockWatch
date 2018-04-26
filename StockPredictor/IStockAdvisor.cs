using System.Collections.Generic;

namespace StockPredictor
{
  public interface IStockAdvisor
  {
    Advice Predict(Ngram ngram);

    IDictionary<string, int> NgramCounts { get; }
    IDictionary<string, int> ParentNgramCounts { get; }

    int NgramSize { get; }

    int NgramCount(string hash);

    int NgramCount(Ngram ngram);

    int Count { get; }
  }
}
