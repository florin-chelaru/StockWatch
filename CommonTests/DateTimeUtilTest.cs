using System;
using System.Linq;
using NUnit.Framework;
using static System.DayOfWeek;
using static Common.DateTimeUtil;

namespace CommonTests
{
  [TestFixture]
  public class DateTimeUtilTest
  {
    [Test]
    public void BusinessDaysBetween_Succeeds()
    {
      // Use a with a random date.
      var initial = new DateTime(2018, 4, 23);

      // Start from each day from Monday to Sunday
      for (int offset = 0; offset < 7; ++offset)
      {
        // Number of days in the interval
        for (int numDays = 1; numDays <= 7; ++numDays)
        {
          var firstDay = initial.AddDays(offset);
          var lastDay = firstDay.AddDays(numDays - 1);

          var actual = BusinessDaysBetween(firstDay, lastDay);

          // Count all business days.
          var expected = (from d in Enumerable.Range(0, numDays)
            let day = firstDay.AddDays(d).DayOfWeek
            where day != Saturday && day != Sunday
            select day).Count();

          Assert.That(actual, Is.EqualTo(expected));
        }
      }
    }
  }
}