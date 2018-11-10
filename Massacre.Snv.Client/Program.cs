using Massacre.Snv.Core.Configuration;
using Massacre.Snv.Core.Network;
using Massacre.Snv.Core.Video;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace Massacre.Snv.Client
{
    class Program
    {
        static void AppThread()
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        static void Main(string[] args)
        {
            if(IsProcessRunning())
            {
                MessageBox.Show("Found another instance of the client software running, will exit now.", "SnvClient",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var chest = Chest.Get();
            string address = chest.Data.EnsureValue("ServerAddress", "127.0.0.1");
            ushort port = Convert.ToUInt16(chest.Data.EnsureValue("ServerPort", EndPointBase.Port.ToString()));

            var appThread = new Thread(() =>
            {
                AppThread();
            });

            appThread.SetApartmentState(ApartmentState.STA);
            appThread.Start();

            // Recorder thread
            new ScreenRecorder();

            while(true)
            {
                try
                {
                    var client = new Client(new TcpClient(address, port).Client);
                    client.Faulted += OnClientFaulted;
                    client.Start();
                    client.Loop();
                }

                catch { }

                Thread.Sleep(10000);
            }
        }

        private static void OnClientFaulted(object sender, EndPointExceptionEventArgs e)
        {
            throw e.ExceptionObject;
        }

        private static bool IsProcessRunning()
        {
            if(Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                return true;
            }

            return false;
        }
    }
}
