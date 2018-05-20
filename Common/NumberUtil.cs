using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
  public static class NumberUtil
  {
    public static decimal ComputeChangeRatio(this decimal current, decimal previous, int roundTo = 5)
    {
      if (previous == 0m)
      {
        return 1m;
      }
      return decimal.Round(current / previous - 1m, roundTo);
    }

    public static decimal Median(this decimal[] values)
    {
      var copy = values.ToArray();
      Array.Sort(copy);
      var mid = copy.Length / 2;
      if (copy.Length % 2 == 0)
      {
        return (copy[mid - 1] + copy[mid]) / 2m;
      }

      return copy[mid];
    }
  }
}
