using System.Collections.Generic;
using System.Linq;

namespace StockWatchData.Models
{
  public class Symbol
  {
    public Symbol()
    {
      DailyQuotes = new HashSet<DailyQuote>();
      SymbolGroupMemberships = new HashSet<SymbolGroupMembership>();
    }

    public string Id { get; set; }
    public string Market { get; set; }

    public ICollection<DailyQuote> DailyQuotes { get; set; }
    public ICollection<SymbolGroupMembership> SymbolGroupMemberships { get; set; }

    public IEnumerable<Group> Groups =>
      from membership in SymbolGroupMemberships select membership.GroupNavigation;

    public override bool Equals(object obj)
    {
      return obj is Symbol symbol &&
             Id == symbol.Id &&
             Market == symbol.Market;
    }

    public override int GetHashCode()
    {
      var hashCode = 554044561;
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Market);
      return hashCode;
    }
  }
}