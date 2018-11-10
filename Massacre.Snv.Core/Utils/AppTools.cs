using Massacre.Snv.Core.Backend;
using Massacre.Snv.Core.Configuration;
using Massacre.Snv.Core.Video;
using Massacre.Snv.Core.Windows;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Massacre.Snv.Core.Utils
{
    public static class AppTools
    {
        static bool icon = false;
        static SnvBackend.ClientIconCallback callback = null;

        public static void Initialize(bool clientIcon = false)
        {
            if(SnvBackend.Init() != 0)
            {
                throw new Exception("Core initialization failed");
            }

            if(clientIcon)
            {
                callback += ClientIconInvokedCallback;
                GC.KeepAlive(callback);

                SnvBackend.SetupClientIcon(callback);
                icon = true;
            }

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        }

        static void ClientIconInvokedCallback()
        {
            Environment.Exit(0);
        }

        private static void OnProcessExit(object sender = null, EventArgs e = null)
        {
            if(icon)
            {
                SnvBackend.RemoveClientIcon();
            }

            ScreenRecorder.IsApplicationRunning = false;
            SnvBackend.Quit();
        }

        public static void TryInvoke(Action a)
        {
            if (Application.Current == null)
            {
                try
                {
                    a();
                }

                catch { }
            }

            else
            {
                Application.Current.Dispatcher.Invoke(a);
            }
        }

        public static void Crash(object exceptionObject)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                new ExceptionWindow(exceptionObject).ShowDialog();
            });

            Environment.Exit(1);
        }

        public static void TaskCrash(Task task)
        {
            Crash(task.Exception);
        }

        public static void SetUpExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Crash(e.ExceptionObject);
        }
    }
}
