using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace StockWatchData
{
  public class Symbol
  {
    [SuppressMessage("Microsoft.Usage",
      "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Symbol()
    {
      DailyQuotes = new HashSet<DailyQuote>();
      Groups = new HashSet<Group>();
    }

    [StringLength(50)]
    public string Id { get; set; }

    [StringLength(50)]
    public string Market { get; set; }

    [SuppressMessage("Microsoft.Usage",
      "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public virtual ICollection<DailyQuote> DailyQuotes { get; set; }

    [SuppressMessage("Microsoft.Usage",
      "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public virtual ICollection<Group> Groups { get; set; }
  }
}