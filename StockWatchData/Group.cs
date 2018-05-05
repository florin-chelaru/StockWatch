using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace StockWatchData
{
  public class Group
  {
    [SuppressMessage("Microsoft.Usage",
      "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Group()
    {
      Symbols = new HashSet<Symbol>();
    }

    [StringLength(50)]
    public string Id { get; set; }

    [SuppressMessage("Microsoft.Usage",
      "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public virtual ICollection<Symbol> Symbols { get; set; }
  }
}