using System.Collections.Generic;
using System.Linq;

namespace StockWatchData.Models
{
  public class Group
  {
    public Group()
    {
      SymbolGroupMemberships = new HashSet<SymbolGroupMembership>();
    }

    public string Id { get; set; }

    public ICollection<SymbolGroupMembership> SymbolGroupMemberships { get; set; }

    public IEnumerable<Symbol> Symbols =>
      from membership in SymbolGroupMemberships select membership.SymbolNavigation;
  }
}