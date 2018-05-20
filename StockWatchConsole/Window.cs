using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace StockWatchConsole
{
  public class Window
  {
    private static readonly decimal[] Limits =
    {
      -0.50m,
      -0.20m,
      -0.10m,
      -0.05m,
      0.00m,
      0.05m,
      0.10m,
      0.20m,
      0.50m
    };

    public static readonly Dictionary<int, string> BucketLabels = new Dictionary<int, string>
    {
      {0, "x < -50%"},
      {1, "-50% <= x < -20%"},
      {2, "-20% <= x < -10%"},
      {3, "-10% <= x < -5%"},
      {4, "-5% <= x < 0%"},
      {5, "0% <= x < 5%"},
      {6, "5% <= x < 10%"},
      {7, "10% <= x < 20%"},
      {8, "20% <= x < 50%"},
      {9, "50% <= x"},
    };

    public string Symbol { get; set; }
    public int PastSize { get; set; }
    public string[] PastDays { get; set; }
    public decimal[] PastValues { get; set; }
    public string DayOne { get; set; }
    public int FutureSize { get; set; }
    public string[] FutureDays { get; set; }
    public decimal[] FutureValues { get; set; }

    public int MaxBucket => ComputeBucket(FutureValues.Max());
    public int MinBucket => ComputeBucket(FutureValues.Min());
    public int MedianBucket => ComputeBucket(FutureValues.Median());

    public string MaxBucketLabel => BucketLabels[MaxBucket];
    public string MinBucketLabel => BucketLabels[MinBucket];
    public string MedianBucketLabel => BucketLabels[MedianBucket];

    private int ComputeBucket(decimal value)
    {
      for (int i = 0; i < Limits.Length; ++i)
      {
        if (value < Limits[i])
        {
          return i;
        }
      }

      return Limits.Length;
    }
  }
}