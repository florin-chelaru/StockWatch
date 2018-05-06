using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using StockWatchData.Models;

namespace StockWatchConsole
{
  class Program
  {
    static void Main(string[] args)
    {
      //      var watcher = new StockWatch.StockWatch(@"c:\Documents\work\stock-prediction\train aapl amzn baba fb goog msft nflx tsla yhoo znga ebay intc gpro".Split(' '));
      //var watcher = new StockWatch.StockWatch(@"c:\Documents\work\stock-prediction\train aapl amzn".Split(' '));

      //      watcher.StartStub();

      // Do some testing

      //      Thread.Sleep(1000 * 60 * 60 * 4);

      //      watcher.StopStub();
      //      StockWatch.Data.Bla x;
      //      StockWatchData.Entities entities;
      //      entities.
//      StockWatchDb db;
      using (var db = new StockWatchDataContext())
      {
//        db.StockQuoteIntervals.Add(new StockQuoteInterval
//        {
//          Symbol = "testsymbol",
//          CollectionFunction = CollectionFunction.Test,
//          Open = 10.0m,
//          Close = 20.0m,
//          High = 25.0m,
//          Low = 5.0m,
//          Volume = 100L,
//          StartTime = DateTime.Parse("2018-04-28 9:00:00"),
//          EndTime = DateTime.Parse("2018-04-28 16:00:00")
//        });
//        db.SaveChanges();
//      
        var nasdaq = (from g in db.Groups where g.Id == "nasdaq" select g).FirstOrDefault() ??
                     db.Groups.Add(new Group {Id = "nasdaq"}).Entity;

        var msft =
          (from symbol in db.Symbols.Include(symbol => symbol.DailyQuotes)
            where symbol.Id == "msft"
            select symbol).FirstOrDefault() ?? db.Symbols.Add(new Symbol {Id = "msft"}).Entity;

        var membership =
          (from m in db.SymbolGroupMemberships
            where m.Symbol == msft.Id && m.Group == nasdaq.Id
            select m).FirstOrDefault() ??
          db.SymbolGroupMemberships
            .Add(new SymbolGroupMembership {Symbol = msft.Id, Group = nasdaq.Id}).Entity;

        var myQuote = (from quote in msft.DailyQuotes
                        where quote.Day == "2018-05-04"
                        select quote)
                      .FirstOrDefault() ??
                      db.DailyQuotes.Add(new DailyQuote
                      {
                        Symbol = msft.Id,
                        Day = "2018-05-04",
                        Open = 10.0m,
                        Close = 20.0m,
                        High = 25.55m,
                        Low = 5.23m,
                        Volume = 100L,
                        OpenChangePercent = 1.23m,
                        CloseChangePercent = 2.43m,
                        HighChangePercent = 4.21m,
                        LowChangePercent = -1.33m,
                        VolumeChangePercent = -8.75m,
                        CollectionFunction = "Test",
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow
                      }).Entity;
        db.SaveChanges();


        Console.WriteLine(myQuote);
        Console.WriteLine(myQuote.Date);
      }
    }
  }
}