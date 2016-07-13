using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleStockWatcher
{
  class StockQuote
  {
    [JsonProperty("t")]
    public string Symbol { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonProperty("e")]
    public string Index { get; set; }

    [JsonProperty("l")]
    public double LastTradePrice { get; set; }

    [JsonProperty("l_cur")]
    public string LastTradeWithCurrency { get; set; }

    [JsonProperty("l_fix")]
    public double LastTradeFixed { get; set; }

    [JsonProperty("s")]
    public int LastTradeSize { get; set; }

    [JsonProperty("ltt")]
    public string LastTradeTime { get; set; }

    [JsonProperty("lt")]
    public string LastTradeDateTimeLong { get; set; }

    [JsonProperty("lt_dts")]
    public string LastTradeDateTime { get; set; }

    [JsonProperty("c")]
    public double Change { get; set; }

    [JsonProperty("c_fix")]
    public double ChangeFixed { get; set; }
    
    [JsonProperty("cp")]
    public double ChangePercent { get; set; }

    [JsonProperty("cp_fix")]
    public double ChangePercentFixed { get; set; }
    
    [JsonProperty("ccol")]
    public string Ccol { get; set; }

    [JsonProperty("pcls_fix")]
    public double PreviousClosePrice { get; set; }
  }
}
