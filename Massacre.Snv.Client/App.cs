using Massacre.Snv.Core.Utils;
using System;
using System.Windows;

namespace Massacre.Snv.Client
{
    public class App : Application
    {
        public void InitializeComponent()
        {
            StartupUri = new Uri("Windows/RecorderWindow.xaml", UriKind.Relative);
            Startup += OnStartup;
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            AppTools.SetUpExceptionHandling();
            AppTools.Initialize(true);
        }
    }
}
