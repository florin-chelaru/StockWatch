namespace StockWatchData.Models
{
    public class SymbolGroupMembership
    {
        public string Symbol { get; set; }
        public string Group { get; set; }

        public Group GroupNavigation { get; set; }
        public Symbol SymbolNavigation { get; set; }
    }
}
