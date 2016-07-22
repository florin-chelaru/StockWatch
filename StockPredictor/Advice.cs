using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPredictor
{
  public class Advice
  {
    public double Prediction { get; set; }
    public double Confidence { get; set; }

    public double PositiveChangeChance { get; set; }
  }
}
