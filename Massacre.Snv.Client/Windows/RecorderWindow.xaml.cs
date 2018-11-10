using Massacre.Snv.Core.Backend;
using Massacre.Snv.Core.Utils;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Massacre.Snv.Client.Windows
{
    public partial class RecorderWindow : Window
    {
        public RecorderWindow()
        {
            InitializeComponent();
            MouseDown += OnMouseDown;
            Closing += OnClosing;
            Visibility = Visibility.Hidden;

            var desktop = SystemParameters.WorkArea;
            Left = desktop.Right - Width;
            Top = desktop.Bottom - Height;

            // Back-end query thread
            new Thread(() =>
            {
                int recState = 0;
                int token = 0;
                bool isSettingsWindowOpen = false;

                var watch = new Stopwatch();

                while (true)
                {
                    Thread.Sleep(50);

                    var recStateNew = SnvBackend.RecIsRunning();
                    if (recStateNew != recState)
                    {
                        recState = recStateNew;
                        AppTools.TryInvoke(() =>
                        {
                            Visibility = recState == 0 ? Visibility.Hidden : Visibility.Visible;
                        });

                        if(recState == 0)
                        {
                            watch.Stop();
                        }
                    }

                    var tokenNew = SnvBackend.RecGetSessionId();
                    if(tokenNew != token)
                    {
                        token = tokenNew;
                        watch.Restart();
                    }

                    AppTools.TryInvoke(() =>
                    {
                        var ts = watch.Elapsed;
                        timerTextBlock.Text = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                    });

                    if(SnvBackend.RecGetSettingsWindowFlag() == 1)
                    {
                        SnvBackend.RecSetSettingsWindowFlag(0);
                        if (!isSettingsWindowOpen)
                        {
                            AppTools.TryInvoke(() =>
                            {
                                new SettingsWindow(() => { isSettingsWindowOpen = false; }).Show();
                                isSettingsWindowOpen = true;
                            });
                        }
                    }
                }
            }).Start();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
