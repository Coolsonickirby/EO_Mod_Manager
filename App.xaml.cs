using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EO_Mod_Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex = null;
        protected override void OnStartup(StartupEventArgs e)
        {
            Utils.RegisterProtocol(Utils.MM_PROTOCOL, "Etrian Odyssey HD Mod Manager");
            if (e.Args.Length == 1)
                this.StartupUri = new Uri("/EO_Mod_Manager;component/GBModPrompt.xaml", UriKind.Relative);
            else
            {
                const string appName = "EtrianOdysseyModManager";
                bool createdNew;

                _mutex = new Mutex(true, appName, out createdNew);

                if (!createdNew)
                {
                    //app is already running! Exiting the application
                    Application.Current.Shutdown();
                }
            }
            base.OnStartup(e);
        }
    }
}
