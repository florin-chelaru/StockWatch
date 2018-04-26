using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;

namespace StockWatch
{
  [RunInstaller(true)]
  public partial class ProjectInstaller : Installer
  {
    public ProjectInstaller()
    {
      InitializeComponent();
    }

    protected override void OnBeforeInstall(IDictionary savedState)
    {
      string parameter = @"c:\Documents\work\stock-prediction\train" + " aapl amzn baba fb goog msft nflx tsla yhoo znga ebay intc gpro";
      //Context.Parameters["assemblypath"] = "\"" + Context.Parameters["assemblypath"] + "\" \"" + parameter + "\"";
      Context.Parameters["assemblypath"] = "\"" + Context.Parameters["assemblypath"] + "\" " + parameter;

      base.OnBeforeInstall(savedState);
    }
  }
}
