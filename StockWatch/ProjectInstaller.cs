using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace StockWatch
{
  [RunInstaller(true)]
  public partial class ProjectInstaller : System.Configuration.Install.Installer
  {
    public ProjectInstaller()
    {
      InitializeComponent();
    }

    protected override void OnBeforeInstall(IDictionary savedState)
    {
      string parameter = @"c:\Documents\work\stock-prediction\train" + " aapl amzn baba fb goog msft nflx tsla yhoo";
      //Context.Parameters["assemblypath"] = "\"" + Context.Parameters["assemblypath"] + "\" \"" + parameter + "\"";
      Context.Parameters["assemblypath"] = "\"" + Context.Parameters["assemblypath"] + "\" " + parameter;

      base.OnBeforeInstall(savedState);
    }
  }
}
