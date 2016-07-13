using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPredictor
{
  class Advice
  {
    public double PredictionMean { get; set; }
    public double PredictionMedian { get; set; }
    public double Confidence { get; set; }
  }
}
