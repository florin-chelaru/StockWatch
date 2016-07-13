using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPredictor
{
  class Program
  {
    static void GenerateTrainingStats(string input)
    {
      Entry[] entries = Entry.FromCsvFile(input);

      State.Sep = ",";
      string sep = State.Sep;
      State.Size = 3;
      State.HashFunction = (es) => string.Join(sep, from e in es select Math.Max(Math.Min((int)Math.Floor(e.ChangePercent * 100.0), 2), -2));

      State[] states = new State[entries.Length - State.Size + 1];
      var counts = new Dictionary<string, int>();
      var parentgramCounts = new Dictionary<string, int>();
      for (var i = 0; i < states.Length; ++i)
      {
        states[i] = State.Create(entries, i);

        int stateCount;
        var hash = states[i].Hash;
        if (!counts.TryGetValue(hash, out stateCount))
        {
          counts[hash] = 0;
        }
        counts[hash]++;

        var parentgram = states[i].ParentHash;
        int parentgramCount;
        if (!parentgramCounts.TryGetValue(parentgram, out parentgramCount))
        {
          parentgramCounts[parentgram] = 0;
        }
        parentgramCounts[parentgram]++;
      }

      // var freqs = counts.ToArray();
      var freqs = (from c in counts select new KeyValuePair<string, double>(c.Key, (double)c.Value / (double)parentgramCounts[c.Key.Substring(0, c.Key.LastIndexOf(sep))])).ToArray();
      Array.Sort(freqs, (p1, p2) => p2.Value.CompareTo(p1.Value));

      foreach (var f in freqs)
      {
        //Console.WriteLine("{0}\t{1}", f.Key, (double)f.Value / (double)states.Length);
        Console.WriteLine("{0},{1},{2}", f.Key, (double)f.Value, parentgramCounts[f.Key.Substring(0, f.Key.LastIndexOf(sep))]);
      }
    }

    static void GenerateTrainingStatsWithDelay(string input, int delay)
    {
      Entry[] entries = Entry.FromCsvFile(input);

      State.Sep = ",";
      string sep = State.Sep;
      State.Size = 4;
      State.HashFunction = (es) => string.Join(sep, from e in es select Math.Max(Math.Min((int)Math.Floor(e.ChangePercent * 100.0), 2), -2));

      State[] states = new State[entries.Length - State.Size + 1 - delay];
      var counts = new Dictionary<string, int>();
      var parentgramCounts = new Dictionary<string, int>();
      for (var i = 0; i < states.Length; ++i)
      {
        states[i] = State.CreateWithDelay(entries, i, delay);

        int stateCount;
        var hash = states[i].Hash;
        if (!counts.TryGetValue(hash, out stateCount))
        {
          counts[hash] = 0;
        }
        counts[hash]++;

        var parentgram = states[i].ParentHash;
        int parentgramCount;
        if (!parentgramCounts.TryGetValue(parentgram, out parentgramCount))
        {
          parentgramCounts[parentgram] = 0;
        }
        parentgramCounts[parentgram]++;
      }

      // var freqs = counts.ToArray();
      var freqs = (from c in counts select new KeyValuePair<string, double>(c.Key, (double)c.Value / (double)parentgramCounts[c.Key.Substring(0, c.Key.LastIndexOf(sep))])).ToArray();
      Array.Sort(freqs, (p1, p2) => p2.Value.CompareTo(p1.Value));

      foreach (var f in freqs)
      {
        //Console.WriteLine("{0}\t{1}", f.Key, (double)f.Value / (double)states.Length);
        Console.WriteLine("{0},{1},{2}", f.Key, (double)f.Value, parentgramCounts[f.Key.Substring(0, f.Key.LastIndexOf(sep))]);
      }
    }

    static void SimulateStrategy(string testInput, Func<State, bool> predicate, double investment, double transactionFee) 
    {
      Entry[] entries = Entry.FromCsvFile(testInput);

      State.Sep = ",";
      string sep = State.Sep;
      State.Size = 4;
      State.HashFunction = (es) => string.Join(sep, from e in es select Math.Max(Math.Min((int)Math.Floor(e.ChangePercent * 100.0), 2), -2));

      State[] states = new State[entries.Length - State.Size + 1];
      var counts = new Dictionary<string, int>();
      var parentgramCounts = new Dictionary<string, int>();

      var bought = 0.0;
      var extra = 0.0;
      for (var i = 0; i < states.Length; ++i)
      {
        states[i] = State.Create(entries, i);
        if (bought > 0 && !predicate(states[i]))
        {
          // Sell
          extra += entries[i + State.Size - 1].Close * bought - investment;
          bought = 0;
          extra -= transactionFee;
        }
        else if (bought <= 0 && predicate(states[i]))
        {
          // Buy
          bought = investment / entries[i + State.Size - 1].Close;
          extra -= transactionFee;
        }
      }

      Console.WriteLine(extra);
    }

    static void SimulateStrategy2(string testInput, Func<State, bool> predicate, double investment, double transactionFee)
    {
      Entry[] entries = Entry.FromCsvFile(testInput);

      State.Sep = ",";
      string sep = State.Sep;
      State.Size = 4;
      State.HashFunction = (es) => string.Join(sep, from e in es select Math.Max(Math.Min((int)Math.Floor(e.ChangePercent * 100.0), 2), -2));

      State[] states = new State[entries.Length - State.Size + 1];
      var counts = new Dictionary<string, int>();
      var parentgramCounts = new Dictionary<string, int>();

      var bought = 0.0;
      var extra = 0.0;
      for (var i = 0; i < states.Length; ++i)
      {
        states[i] = State.Create(entries, i);
        if (bought > 0 && !predicate(states[i]))
        {
          // Sell
          var e = entries[i + State.Size - 1];
          var sellGainLoss = (e.Close + e.Open)/2.0 * bought - investment;
          if (sellGainLoss >= 0)
          {
            Console.WriteLine("Selling on " + e.Date.ToShortDateString() + " at price " + (e.Close + e.Open) / 2.0 + ". Gain/loss: " + sellGainLoss);
            extra += sellGainLoss;
            bought = 0;
            extra -= transactionFee;
          }
        }
        else if (bought <= 0 && predicate(states[i]))
        {
          // Buy
          var e = entries[i + State.Size - 1];
          Console.WriteLine("Buying on " + e.Date.ToShortDateString() + " at price " + (e.Close + e.Open) / 2.0);
          bought = investment / ((e.Close + e.Open) / 2.0);
          extra -= transactionFee;
        }
      }

      Console.WriteLine(extra);
    }

    static double SimulateStrategyWithDelay(string testInput, Func<State, bool> predicate, double investment, double transactionFee, int delay)
    {
      Console.WriteLine(testInput);
      Entry[] entries = Entry.FromCsvFile(testInput);
      
      State.Sep = ",";
      string sep = State.Sep;
      State.Size = 4;
      State.HashFunction = (es) => string.Join(sep, from e in es select Math.Max(Math.Min((int)Math.Floor(e.ChangePercent * 100.0), 2), -2));

      State[] states = new State[entries.Length - State.Size + 1];
      var counts = new Dictionary<string, int>();
      var parentgramCounts = new Dictionary<string, int>();

      var bought = 0.0;
      var extra = 0.0;
      var sellCountdown = 0;
      for (var i = 0; i < states.Length; ++i)
      {
        --sellCountdown;
        states[i] = State.Create(entries, i);
        if (bought > 0 && !predicate(states[i]) && sellCountdown <= 0)
        {
          // Sell
          var e = entries[i + State.Size - 1];
          var sellGainLoss = (e.Close + e.Open) / 2.0 * bought - investment;
          //if (sellGainLoss >= 0)
          //{
            Console.WriteLine("Selling on " + e.Date.ToShortDateString() + " at price " + (e.Close + e.Open) / 2.0 + ". Gain/loss: " + sellGainLoss);
            extra += sellGainLoss;
            bought = 0;
            extra -= transactionFee;
          //}
        }
        else if (bought <= 0 && predicate(states[i]))
        {
          // Buy
          var e = entries[i + State.Size - 1];
          Console.WriteLine("Buying on " + e.Date.ToShortDateString() + " at price " + (e.Close + e.Open) / 2.0);
          bought = investment / ((e.Close + e.Open) / 2.0);
          extra -= transactionFee;
          sellCountdown = delay;
        }
      }

      if (bought > 0)
      {
        // Sell last acquired stock
        var e = entries[entries.Length - 1];
        var sellGainLoss = (e.Close + e.Open) / 2.0 * bought - investment;
        Console.WriteLine("Selling on " + e.Date.ToShortDateString() + " at price " + (e.Close + e.Open) / 2.0 + ". Gain/loss: " + sellGainLoss);
        extra += sellGainLoss;
        bought = 0;
        extra -= transactionFee;
      }

      Console.WriteLine(extra + "\n");
      return extra;
    }

    static void Simulation3(BuyAdviser adviser, Entry[] testEntries)
    {
      // Test the trainer
      var analysis = false;
      var maxInvestment = 5000.0;
      var investAtATime = 1000.0; // How much money do we invest at a time
      var totalInvested = 0.0; // How much money have we invested so far
      var bought = 0.0; // How much stock do we have
      var extra = 0.0;
      var delay = 3;
      var emergencySell = 60;
      var sellCountdown = 0;
      var emergencySellCountdown = 0;
      double error = 0.0;
      for (var i = 0; i < testEntries.Length - adviser.NgramSize - 1; ++i)
      {
        var es = new Entry[adviser.NgramSize];
        for (var j = 0; j < adviser.NgramSize; ++j)
        {
          es[j] = testEntries[i + j];

          // Add new data
          // adviser.Add(es[j]);
        }
        var ngram = new Ngram(es);

        // TODO: Next, compare the result we get from the adviser with the next entry to see how close we are
        var advice = adviser.Predict(ngram);
        var next = testEntries[i + adviser.NgramSize];
        var current = testEntries[i + adviser.NgramSize - 1];
        var dif = advice.PredictionMedian - next.ChangePercent;
        error += dif * dif * 10000;
        //Console.WriteLine("{0},{1},{2}", advice.PredictionMedian * 100, next.ChangePercent * 100, dif * 100);

        --sellCountdown;
        --emergencySellCountdown;
        if (analysis) { Console.Write("{0},{1},", current.Date.ToShortDateString(), current.Close); }
        if (advice.PredictionMedian > 0 && advice.Confidence >= 30.0 / (adviser.Count - adviser.NgramSize) && totalInvested < maxInvestment)
        {
          // Buy!
          sellCountdown = delay;
          emergencySellCountdown = emergencySell;

          var nshares = Math.Ceiling(investAtATime / current.Close);
          var investment = nshares * current.Close;

          if (!analysis)
          {
            Console.WriteLine("[{3}] Buying {0} shares at {1} per share for a total of {2}", nshares, current.Close, investment, current.Date.ToShortDateString());
          }
          else
          {
            Console.Write("{0},,", current.Close);
          }

          // totalInvested += investAtATime;
          // bought += investAtATime / current.Close;
          totalInvested += investment;
          bought += nshares;
        }
        else if (bought > 0 && sellCountdown <= 0)
        {
          // We can sell, provided that the current price is above what we bought for. We know that the predicted price is worse. Otherwise, wait.
          if (current.Close > totalInvested / bought || emergencySellCountdown <= 0)
          {
            // Sell
            var gainLoss = current.Close * bought - totalInvested;
            
            if (!analysis)
            {
              Console.WriteLine("[{4}] Selling {0} shares at {1} per share for a total of {2} (Gain/loss: {3})", bought, current.Close, current.Close * bought, gainLoss, current.Date.ToShortDateString());
            }
            else
            {
              Console.Write(",{0}", current.Close);
            }
            extra += gainLoss;
            totalInvested = 0.0;
            bought = 0.0;
          }
          else
          {
            if (analysis)
            {
              Console.Write(",,");
            }
          }
        }
        if (analysis)
        {
          Console.WriteLine();
        }
      }

      if (bought > 0)
      {
        var current = testEntries[testEntries.Length - 1];
        var gainLoss = current.Close * bought - totalInvested;
        extra += gainLoss;

        if (!analysis)
        {
          Console.WriteLine("[{4}] Selling {0} shares at {1} per share for a total of {2} (Gain/loss: {3})", bought, current.Close, current.Close * bought, gainLoss, current.Date.ToShortDateString());
        }
      }

      error /= testEntries.Length - adviser.NgramSize - 1;

      //Console.WriteLine("Error: {0}", error);
      Console.WriteLine("Total gain/loss: {0}", extra);
    }

    static void Main(string[] args)
    {
      //var input = @"c:\Documents\work\stock-prediction\train.csv";
      //var input = @"c:\Documents\work\stock-prediction\aapl_train.csv";
      //var input = @"c:\Documents\work\stock-prediction\amzn_train_until2015.csv";
      //var input = @"c:\Documents\work\stock-prediction\msft_train_more.csv";
      //var input = @"c:\Documents\work\stock-prediction\tsla_train_until2015.csv";
      var input = @"c:\Documents\work\stock-prediction\amzn_train.csv";

      var trainingEntries = Entry.FromCsvFile(input);

      var adviser = new BuyAdviser(trainingEntries);

      //var testEntries = Entry.FromCsvFile(@"c:\Documents\work\stock-prediction\aapl_after_split2016.csv");
      var testEntries = Entry.FromCsvFile(@"c:\Documents\work\stock-prediction\amzn2014.csv");
      //var testEntries = Entry.FromCsvFile(@"c:\Documents\work\stock-prediction\msft2016.csv");
      //var testEntries = Entry.FromCsvFile(@"c:\Documents\work\stock-prediction\tsla2016.csv");

      Simulation3(adviser, testEntries);

      //GenerateTrainingStatsWithDelay(input, 3);

      //Func<State, bool> predicate = (state) =>
      //{
      //  return state.Hash.Contains(String.Format("{0}{1}{0}{1}{0}", State.Sep, "-2")) && !state.Hash.StartsWith("1");
      //};

      //Func<State, bool> predicate = (state) =>
      //{
      //  return state.Hash.Contains(String.Format("{0}{1}{0}{2}{0}", State.Sep, "0", "0")) && !state.Hash.StartsWith("-1");
      //};

      //var extra = SimulateStrategyWithDelay(@"c:\Documents\work\stock-prediction\test2015.csv", predicate, 1000, 0, 3);
      //extra += SimulateStrategyWithDelay(@"c:\Documents\work\stock-prediction\aapl2014.csv", predicate, 1000, 0, 3);
      //extra += SimulateStrategyWithDelay(@"c:\Documents\work\stock-prediction\aapl_after_split2016.csv", predicate, 1000, 0, 3);

      //Console.WriteLine("Total: " + extra);
    }
  }
}
