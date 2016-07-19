using StockPredictor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWatch
{
  class StockManager
  {
    public int NgramSize { get; set; }

    public string Symbol { get; set; }

    public string TrainingFilePath { get; set; }

    public IList<Entry> RecentHistory { get; set; }

    public BuyAdviser Adviser { get; set; }

    public int SellDelay { get; set; }

    public int EmergencySellDelay { get; set; }

    public int SellCountdown { get; set; }

    public int EmergencySellCountdown { get; set; }

    public double TotalInvested { get; set; }

    public double MaxInvestment { get; set; }

    public double InvestAtATime { get; set; }

    public bool BoughtToday { get; set; }

    public double ShareCount { get; set; }

    public double Extra { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="buy">Action(nShares, investment)</param>
    /// <param name="sell">Action(gainLoss)</param>
    public void Decide(Entry entry, Action<double, double> buy, Action<double> sell, Action wait, Action<string, string, EventLogEntryType> log)
    {
      var tookAction = false;
      var history = RecentHistory;

      // First, figure out if this is the first entry in the day
      var lastEntry = history.Last();
      if (lastEntry.Date.Day != entry.Date.Day)
      {
        history.Add(entry);
        --SellCountdown;
        --EmergencySellCountdown;
        BoughtToday = false;
      }
      else
      {
        history[history.Count - 1] = entry; // replace last entry
      }

      var adviser = Adviser;
      var es = new Entry[NgramSize];
      for (var j = 0; j < NgramSize; ++j)
      {
        es[j] = history[history.Count - NgramSize + j];
      }
      var ngram = new Ngram(es);

      var advice = adviser.Predict(ngram);

      var msg = string.Format("{0} Prediction for tomorrow's change: {1:0.00}% (confidence: {2:0.00}% of training data)", entry.Symbol, advice.PredictionMean * 100.0, advice.Confidence * 100.0);
      log(msg, msg, EventLogEntryType.Information);

      if (!BoughtToday &&
        advice.PredictionMean > 0 &&
        advice.Confidence >= 30.0 / (adviser.Count - adviser.NgramSize) &&
        TotalInvested < MaxInvestment)
      {
        // Buy!
        BoughtToday = true;
        SellCountdown = SellDelay;
        EmergencySellCountdown = EmergencySellDelay;

        var nshares = Math.Ceiling(InvestAtATime / entry.Close);
        var investment = nshares * entry.Close;

        TotalInvested += investment;
        ShareCount += nshares;

        buy(nshares, investment);
        tookAction = true;
        //Log(string.Format("Buy {0} x {1} at {2}/share for {3}", nshares, mgr.Symbol, entry.Close, investment), type: EventLogEntryType.Warning);
      }
      else if (ShareCount > 0 && SellCountdown <= 0)
      {
        // We can sell, provided that the current price is above what we bought for. We know that the predicted price is worse. Otherwise, wait.
        if (entry.Close > TotalInvested / ShareCount || EmergencySellCountdown <= 0)
        {
          // Sell
          var gainLoss = entry.Close * ShareCount - TotalInvested;

          sell(gainLoss);
          tookAction = true;
          //Log(
          //  string.Format("Sell {0} x {1} at {2}/share for {3} (Gain/loss: {4})", mgr.ShareCount, mgr.Symbol, entry.Close, entry.Close * mgr.ShareCount, gainLoss),
          //  type: EventLogEntryType.Warning);

          Extra += gainLoss;
          TotalInvested = 0.0;
          ShareCount = 0.0;
        }
      }

      if (!tookAction) { wait(); }
    }
  }
}
