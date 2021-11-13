using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using FileDB.Config;

namespace FileDB
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Utils.DemoModeEnabled = e.Args.Any(x => x == "--demo");

            if (Utils.DemoModeEnabled)
            {
                Utils.BrowserConfig = BrowserConfigFactory.CreateDemoConfig();
            }
            else
            {
                Utils.BrowserConfig = Utils.BrowserConfigIO.Read() ?? BrowserConfigFactory.CreateDefaultConfig();
            }
        }
    }
}
