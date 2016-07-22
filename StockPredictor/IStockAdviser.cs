using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPredictor
{
  public interface IStockAdviser
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
