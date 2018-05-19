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
      return decimal.Round(current / previous - 1m, roundTo);
    }
  }
}
