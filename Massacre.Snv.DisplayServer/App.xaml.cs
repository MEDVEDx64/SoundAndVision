using Massacre.Snv.Core.Utils;
using System.Windows;

namespace Massacre.Snv.DisplayServer
{
    public partial class App : Application
    {
        public App() : base()
        {
            Startup += OnStartup;
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            AppTools.SetUpExceptionHandling();
            AppTools.Initialize();
        }
    }
}
