using System;
using System.Collections.Generic;
using System.Linq;

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
      State.HashFunction = es => string.Join(sep, from e in es select Math.Max(Math.Min((int)Math.Floor(e.ChangePercent * 100.0), 2), -2));

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
      var freqs = (from c in counts select new KeyValuePair<string, double>(c.Key, c.Value / (double)parentgramCounts[c.Key.Substring(0, c.Key.LastIndexOf(sep))])).ToArray();
      Array.Sort(freqs, (p1, p2) => p2.Value.CompareTo(p1.Value));

      foreach (var f in freqs)
      {
        //Console.WriteLine("{0}\t{1}", f.Key, (double)f.Value / (double)states.Length);
        Console.WriteLine("{0},{1},{2}", f.Key, f.Value, parentgramCounts[f.Key.Substring(0, f.Key.LastIndexOf(sep))]);
      }
    }

    static void GenerateTrainingStatsWithDelay(string input, int delay)
    {
      Entry[] entries = Entry.FromCsvFile(input);

      State.Sep = ",";
      string sep = State.Sep;
      State.Size = 4;
      State.HashFunction = es => string.Join(sep, from e in es select Math.Max(Math.Min((int)Math.Floor(e.ChangePercent * 100.0), 2), -2));

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
      var freqs = (from c in counts select new KeyValuePair<string, double>(c.Key, c.Value / (double)parentgramCounts[c.Key.Substring(0, c.Key.LastIndexOf(sep))])).ToArray();
      Array.Sort(freqs, (p1, p2) => p2.Value.CompareTo(p1.Value));

      foreach (var f in freqs)
      {
        //Console.WriteLine("{0}\t{1}", f.Key, (double)f.Value / (double)states.Length);
        Console.WriteLine("{0},{1},{2}", f.Key, f.Value, parentgramCounts[f.Key.Substring(0, f.Key.LastIndexOf(sep))]);
      }
    }

    static void SimulateStrategy(string testInput, Func<State, bool> predicate, double investment, double transactionFee) 
    {
      Entry[] entries = Entry.FromCsvFile(testInput);

      State.Sep = ",";
      string sep = State.Sep;
      State.Size = 4;
      State.HashFunction = es => string.Join(sep, from e in es select Math.Max(Math.Min((int)Math.Floor(e.ChangePercent * 100.0), 2), -2));

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
      State.HashFunction = es => string.Join(sep, from e in es select Math.Max(Math.Min((int)Math.Floor(e.ChangePercent * 100.0), 2), -2));

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
      State.HashFunction = es => string.Join(sep, from e in es select Math.Max(Math.Min((int)Math.Floor(e.ChangePercent * 100.0), 2), -2));

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

    static void Simulation3(BuyAdvisor advisor, Entry[] testEntries)
    {
      // Test the trainer
      var analysis = false;
      var maxInvestment = 5000.0;
      var investAtATime = 5000.0; // How much money do we invest at a time
      var totalInvested = 0.0; // How much money have we invested so far
      var bought = 0.0; // How much stock do we have
      var extra = 0.0;
      var delay = 3;
      var emergencySell = 60;
      var sellCountdown = 0;
      var emergencySellCountdown = 0;
      double error = 0.0;
      for (var i = 0; i < testEntries.Length - advisor.NgramSize - 1; ++i)
      {
        var es = new Entry[advisor.NgramSize];
        for (var j = 0; j < advisor.NgramSize; ++j)
        {
          es[j] = testEntries[i + j];

          // Add new data
          // adviser.Add(es[j]);
        }
        var ngram = new Ngram(es);

        var advice = advisor.Predict(ngram);
        var next = testEntries[i + advisor.NgramSize];
        var current = testEntries[i + advisor.NgramSize - 1];
        var dif = advice.Prediction - next.ChangePercent;
        error += dif * dif * 10000;

        if (!analysis)
        {
          // Console.WriteLine("Prediction: {0}, Truth: {1}, Dif: {2}, Confidence: {3}", advice.PredictionMean * 100, next.ChangePercent * 100, dif * 100, advice.Confidence * 100);
          Console.WriteLine("{0},{1},{2},", current.Date.ToShortDateString(), advice.Prediction * 100.0, next.ChangePercent * 100.0);
        }

        --sellCountdown;
        --emergencySellCountdown;
        if (analysis) { Console.Write("{0},{1},", current.Date.ToShortDateString(), current.Close); }

        if (advice.Prediction > 0.0 && advice.Confidence >= 30.0 / (advisor.Count - advisor.NgramSize) && totalInvested < maxInvestment)
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

      error /= testEntries.Length - advisor.NgramSize - 1;

      //Console.WriteLine("Error: {0}", error);
      Console.WriteLine("Total gain/loss: {0}", extra);
    }

    static void Simulation4(BuyAdvisor[] advisors, Entry[] testEntries, int ngramSize)
    {
      // Test the trainer
      var analysis = false;
      var maxInvestment = 5000.0;
      var investAtATime = 5000.0; // How much money do we invest at a time
      var totalInvested = 0.0; // How much money have we invested so far
      var bought = 0.0; // How much stock do we have
      var extra = 0.0;
      var delay = 3;
      var emergencySell = 60;
      var sellCountdown = 0;
      var emergencySellCountdown = 0;
      //double error = 0.0;
      for (var i = 0; i < testEntries.Length - ngramSize - 1; ++i)
      {
        var es = new Entry[ngramSize];
        for (var j = 0; j < ngramSize; ++j)
        {
          es[j] = testEntries[i + j];

          // Add new data
          // adviser.Add(es[j]);
        }
        var ngram = new Ngram(es);

        var advices = from a in advisors select a.Predict(ngram);
        var advice = new Advice
        {
          Prediction = (from a in advices select a.Prediction).Average(),
          Confidence = (from a in advices select a.Confidence).Average()
          //advisers.Predict(ngram);
        };
        var next = testEntries[i + ngramSize];
        var current = testEntries[i + ngramSize - 1];
        //var dif = advice.Prediction - next.ChangePercent;
        //error += dif * dif * 10000;

        if (!analysis)
        {
          // Console.WriteLine("Prediction: {0}, Truth: {1}, Dif: {2}, Confidence: {3}", advice.PredictionMean * 100, next.ChangePercent * 100, dif * 100, advice.Confidence * 100);
          Console.WriteLine("{0},{1},{2},", current.Date.ToShortDateString(), advice.Prediction * 100.0, next.ChangePercent * 100.0);
        }

        --sellCountdown;
        --emergencySellCountdown;
        if (analysis) { Console.Write("{0},{1},", current.Date.ToShortDateString(), current.Close); }

        if (advice.Prediction > 0.0 && advice.Confidence >= 0.03 && totalInvested < maxInvestment)
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

      //error /= testEntries.Length - advisers.NgramSize - 1;

      //Console.WriteLine("Error: {0}", error);
      Console.WriteLine("Total gain/loss: {0}", extra);
    }

    static void GenerateTestResults(BuyAdvisor[] advisors, Entry[] testEntries, int ngramSize)
    {
      for (var i = 0; i < testEntries.Length - ngramSize - 1; ++i)
      {
        var es = new Entry[ngramSize];
        for (var j = 0; j < ngramSize; ++j)
        {
          es[j] = testEntries[i + j];
        }
        var ngram = new Ngram(es);

        var advices = from a in advisors select a.Predict(ngram);
        var sumConfidence = (from a in advices select a.Confidence).Sum();

        var advice = new Advice
        {
          Prediction = (from a in advices select a.Prediction * a.Confidence / sumConfidence).Sum(),
          Confidence = sumConfidence / advisors.Length
        };
        var next = testEntries[i + ngramSize];
        var current = testEntries[i + ngramSize - 1];

        Console.WriteLine("{0},{1},{2},{3},", current.Date.ToShortDateString(), advice.Prediction * 100.0, next.ChangePercent * 100.0, advice.Confidence);
      }
    }

    static void PrintStats(BuyAdvisor[] advisors)
    {
      IDictionary<string, IDictionary<string, int>> ngramCounts = new SortedDictionary<string, IDictionary<string, int>>();
      IDictionary<string, int> parentCounts = new SortedDictionary<string, int>();

      foreach (var adviser in advisors)
      {
        foreach (var pair in adviser.PredictionNgrams)
        {
          IDictionary<string, int> counts;
          if (!ngramCounts.TryGetValue(pair.Key, out counts))
          {
            counts = new SortedDictionary<string, int>();
            ngramCounts[pair.Key] = counts;
            parentCounts[pair.Key] = 0;
          }
          foreach (var pair2 in pair.Value)
          {
            int count;
            if (!counts.TryGetValue(pair2.Key, out count))
            {
              count = 0;
              counts[pair2.Key] = count;
            }
            counts[pair2.Key] = count + pair2.Value.Count;
            parentCounts[pair.Key] += pair2.Value.Count;
          }
        }
      }

      foreach (var pair in ngramCounts)
      {
        var t = (from p in pair.Value select double.Parse(p.Key.Substring(p.Key.LastIndexOf(',') + 1)) * p.Value / parentCounts[pair.Key]).Sum();
        foreach (var pair2 in pair.Value)
        {
          Console.WriteLine("{0},{1},{2},{3}", pair2.Key, (double)pair2.Value / parentCounts[pair.Key], parentCounts[pair.Key], t);
          //Console.WriteLine("{0},", pair2.Key);
        }
        //Console.WriteLine(pair.Key);
      }
      Console.WriteLine((from p in parentCounts select p.Value).Sum());
    }

    static void Simulate5(CompositeAdvisor advisor, IList<Entry> testEntries, int ngramSize)
    {
      double threshold = 0.60;
      double totalPositives = 0.0;
      double correctPositives = 0.0;
      double totalNegatives = 0.0;
      double correctNegatives = 0.0;
      for (var i = 0; i < testEntries.Count - ngramSize - 1; ++i)
      {
        var es = new Entry[ngramSize];
        for (var j = 0; j < ngramSize; ++j)
        {
          es[j] = testEntries[i + j];
        }
        var ngram = new Ngram(es);

        var advice = advisor.Predict(ngram);
        
        var next = testEntries[i + ngramSize];
        var current = testEntries[i + ngramSize - 1];

        Console.WriteLine("{0},{1},{2},{3},", next.Date.ToShortDateString(), advice.PositiveChangeChance * 100.0, next.ChangePercent * 100.0, advice.Confidence);

        if (advice.PositiveChangeChance >= threshold)
        {
          totalPositives += 1.0;
          if (next.ChangePercent > 0)
          {
            correctPositives += 1.0;
          }
        }

        if (advice.PositiveChangeChance <= 1.0 - threshold)
        {
          totalNegatives += 1.0;
          if (next.ChangePercent < 0)
          {
            correctNegatives += 1.0;
          }
        }
      }
      Console.WriteLine("Percentage correct positives: {0:0.00}% ({1}/{2})", (totalPositives > 0.0) ? correctPositives / totalPositives : 0.0, correctPositives, totalPositives);
      Console.WriteLine("Percentage correct negatives: {0:0.00}% ({1}/{2})", (totalNegatives > 0.0) ? correctNegatives / totalNegatives : 0.0, correctNegatives, totalNegatives);
      Console.WriteLine("Percentage correct: {0:0.00}% ({1}/{2})", (totalNegatives + totalPositives > 0.0) ? (correctNegatives + correctPositives) / (totalNegatives + totalPositives) : 0.0, correctNegatives + correctPositives, totalNegatives + totalPositives);
    }

    static void Main(string[] args)
    {
      //var input = @"c:\Documents\work\stock-prediction\train.csv";
      //var input = @"c:\Documents\work\stock-prediction\aapl_train.csv";
      //var input = @"c:\Documents\work\stock-prediction\amzn_train_until2015.csv";
      //var input = @"c:\Documents\work\stock-prediction\msft_train.csv";
      //var input = @"c:\Documents\work\stock-prediction\msft_train_more.csv";
      //var input = @"c:\Documents\work\stock-prediction\tsla_train_until2015.csv";
      //var input = @"c:\Documents\work\stock-prediction\amzn_train.csv";

      const int ngramSize = 3;
      //var inputs = new string[]
      //{
      //   @"c:\Documents\work\stock-prediction\aapl_train.csv",
      //   @"c:\Documents\work\stock-prediction\amzn_train_until2015.csv",
      //   @"c:\Documents\work\stock-prediction\msft_train_more.csv",
      //   @"c:\Documents\work\stock-prediction\tsla_train_until2015.csv"
      //};
      //var inputs = new Tuple<string, string>[]
      //{
      //   new Tuple<string, string>("aapl", @"c:\Documents\work\stock-prediction\train\aapl-2010-2016.csv"),
      //   new Tuple<string, string>("amzn", @"c:\Documents\work\stock-prediction\train\amzn-2010-2016.csv"),
      //   new Tuple<string, string>("baba", @"c:\Documents\work\stock-prediction\train\baba-2010-2016.csv"),
      //   new Tuple<string, string>("fb", @"c:\Documents\work\stock-prediction\train\fb-2010-2016.csv"),
      //   new Tuple<string, string>("goog", @"c:\Documents\work\stock-prediction\train\goog-2010-2016.csv"),
      //   new Tuple<string, string>("msft", @"c:\Documents\work\stock-prediction\train\msft-2010-2016.csv"),
      //   new Tuple<string, string>("nflx", @"c:\Documents\work\stock-prediction\train\nflx-2010-2016.csv"),
      //   new Tuple<string, string>("tsla", @"c:\Documents\work\stock-prediction\train\tsla-2010-2016.csv"),
      //   new Tuple<string, string>("yhoo", @"c:\Documents\work\stock-prediction\train\yhoo-2010-2016.csv")
      //};
      var inputs = new[]
      {
         new Tuple<string, string>("aapl", @"c:\Documents\work\stock-prediction\train_old\aapl-2010-2014.csv"),
         new Tuple<string, string>("amzn", @"c:\Documents\work\stock-prediction\train_old\amzn-2010-2014.csv"),
         new Tuple<string, string>("baba", @"c:\Documents\work\stock-prediction\train_old\baba-2010-2014.csv"),
         new Tuple<string, string>("fb", @"c:\Documents\work\stock-prediction\train_old\fb-2010-2014.csv"),
         new Tuple<string, string>("goog", @"c:\Documents\work\stock-prediction\train_old\goog-2010-2014.csv"),
         new Tuple<string, string>("msft", @"c:\Documents\work\stock-prediction\train_old\msft-2010-2014.csv"),
         new Tuple<string, string>("nflx", @"c:\Documents\work\stock-prediction\train_old\nflx-2010-2014.csv"),
         new Tuple<string, string>("tsla", @"c:\Documents\work\stock-prediction\train_old\tsla-2010-2014.csv"),
         new Tuple<string, string>("yhoo", @"c:\Documents\work\stock-prediction\train_old\yhoo-2010-2014.csv")
      };
      var advisers = (from input in inputs select new BuyAdvisor(input.Item1, Entry.FromCsvFile(input.Item2), ngramSize)).ToArray();
      //var adviser = new BuyAdviser(inputs[0].Item1, Entry.FromCsvFile(inputs[0].Item2), ngramSize);
      //var advisers = (from input in inputs select new BuyAdviser(input.Item1, Entry.FromCsvFile(input.Item2), ngramSize)).Take(2).ToArray();
      var cAdviser = new CompositeAdvisor(advisers, ngramSize);

      var testEntries = Entry.FromCsvFile(@"c:\Documents\work\stock-prediction\test\amzn-2016-2016.csv");

      Simulate5(cAdviser, testEntries, ngramSize);

      // PrintStats(advisers);

      //var trainingEntries = Entry.FromCsvFile(input);

      //var adviser = new BuyAdviser(trainingEntries);

      //var testEntries = Entry.FromCsvFile(@"c:\Documents\work\stock-prediction\aapl_after_split2016.csv");
      //var testEntries = Entry.FromCsvFile(@"c:\Documents\work\stock-prediction\amzn2014.csv");
      //var testEntries = Entry.FromCsvFile(@"c:\Documents\work\stock-prediction\msft2014.csv");
      //var testEntries = Entry.FromCsvFile(@"c:\Documents\work\stock-prediction\tsla2016.csv");
      //var testEntries = Entry.FromCsvFile(@"c:\Documents\work\stock-prediction\amzn2016.csv");

      //GenerateTestResults(advisers, testEntries, ngramSize);

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
