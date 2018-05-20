using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace StockWatchData.Models
{
  public class Symbol
  {
    private static readonly Regex TagPattern = new Regex("^{([^}]+)}$");

    public Symbol()
    {
      DailyQuotes = new HashSet<DailyQuote>();
      SymbolGroupMemberships = new HashSet<SymbolGroupMembership>();
      Windows = new HashSet<Window>();
    }

    public override string ToString()
    {
      return $"{nameof(Id)}: {Id}, {nameof(Tags)}: {Tags}";
    }

    public string Id { get; set; }
    public string Market { get; set; }
    public string Tags { get; set; }

    public ICollection<DailyQuote> DailyQuotes { get; set; }
    public ICollection<SymbolGroupMembership> SymbolGroupMemberships { get; set; }
    public ICollection<Window> Windows { get; set; }

    public IEnumerable<Group> Groups =>
      from membership in SymbolGroupMemberships select membership.GroupNavigation;


    public ImmutableHashSet<string> TagSet => Tags == null
      ? ImmutableHashSet<string>.Empty
      : Tags.Split(',').Select(t => TagPattern.Match(t).Groups[1].Value).ToImmutableHashSet();

    public void AddTag(string tag)
    {
      if (!TagSet.Contains(tag))
      {
        Tags = Tags == null ? $"{{{tag}}}" : $"{Tags},{{{tag}}}";
      }
    }

    public override bool Equals(object obj)
    {
      return obj is Symbol symbol &&
             Id == symbol.Id &&
             Market == symbol.Market &&
             Tags == symbol.Tags;
    }

    public override int GetHashCode()
    {
      var hashCode = 554044561;
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Market);
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Tags);
      return hashCode;
    }
  }
}