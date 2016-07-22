using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPredictor
{
  public class AggregateMethod
  {
    private Func<IEnumerable<double>, double> aggregator;

    private AggregateMethod(Func<IEnumerable<double>, double> aggregator)
    {
      this.aggregator = aggregator;
    }

    public double Aggregate(IEnumerable<double> values) { return aggregator(values); }

    public static readonly AggregateMethod Average = new AggregateMethod((values) => values.DefaultIfEmpty(0.0).Average());
    public static readonly AggregateMethod Median = new AggregateMethod((values) =>
    {
      List<double> list = new List<double>(values.DefaultIfEmpty());
      list.Sort();
      var i1 = list.Count / 2;
      var i2 = (list.Count - 1) / 2;
      if (i1 == i2) { return list[i1]; }
      return (list[i1] + list[i2]) * 0.5;
    });
  }
}
