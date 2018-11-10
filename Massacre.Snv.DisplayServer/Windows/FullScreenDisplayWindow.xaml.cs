using Massacre.Snv.Core.Backend;
using Massacre.Snv.Core.Utils;
using Massacre.Snv.DisplayServer.ViewModels;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace Massacre.Snv.DisplayServer.Windows
{
    public partial class FullScreenDisplayWindow : Window
    {
        double scrW, scrH, pW = 0, pH = 0;
        Display display = null;

        public Display Display
        {
            get { return display; }
        }

        public FullScreenDisplayWindow(Display display)
        {
            InitializeComponent();
            DataContext = display;
            this.display = display;

            var res = SnvBackend.ScrGetResolution(0);
            if (res <= 0)
            {
                return;
            }

            scrW = res >> 16;
            scrH = res & 0xffff;

            const double opacity = 0.64;
            restoreIcon.Opacity = opacity;
            closeIcon.Opacity = opacity;
            restoreIcon.MouseEnter += delegate { restoreIcon.Opacity = 1.0; };
            restoreIcon.MouseLeave += delegate { restoreIcon.Opacity = opacity; };
            closeIcon.MouseEnter += delegate { closeIcon.Opacity = 1.0; };
            closeIcon.MouseLeave += delegate { closeIcon.Opacity = opacity; };
            closeIcon.MouseUp += delegate { Close(); };
            panel.Opacity = 0;
            panel.IsEnabled = false;

            bool running = true;
            Title = display.WindowHeader;
            display.Origin.Disconnected += OnClientDisconnected;
            display.IsFullScreen = true;

            Loaded += delegate
            {
                HwndSource.FromHwnd(new WindowInteropHelper(this).Handle).AddHook(new HwndSourceHook(WndProc));
            };

            Closed += delegate
            {
                running = false;
                if(display != null)
                {
                    display.IsFullScreen = false;
                }
            };

            restoreIcon.MouseUp += delegate
            {
                Width = pW;
                Height = pH;
                panel.Opacity = 0;
                panel.IsEnabled = false;
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanResize;
            };

            // Size polling thread
            new Thread(() =>
            {
                uint pw = 0, ph = 0;
                bool changed = false;

                while (running)
                {
                    Thread.Sleep(500);
                    AppTools.TryInvoke(() =>
                    {
                        try
                        {
                            if ((uint)image.ActualWidth != pw || (uint)image.ActualHeight != ph)
                            {
                                pw = (uint)image.ActualWidth;
                                ph = (uint)image.ActualHeight;
                                changed = true;
                            }

                            if (display != null)
                            {
                                if (changed)
                                {
                                    display.OnActualSizeChanged(pw, ph);
                                    changed = false;
                                }

                                if(!display.Origin.IsRunning)
                                {
                                    running = false;
                                }
                            }

                            else
                            {
                                running = false;
                            }
                        }

                        catch
                        {
                            running = false;
                        }
                    });
                }

                display.RestoreDenominator();
            }).Start();
        }

        ~FullScreenDisplayWindow()
        {
            if (display != null && display.Origin != null)
            {
                display.Origin.Disconnected -= OnClientDisconnected;
            }
        }

        private void OnClientDisconnected(object sender, EventArgs e)
        {
            Close();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if(msg == WinDefs.WM_SYSCOMMAND)
            {
                if(wParam.ToInt32() == WinDefs.SC_MAXIMIZE)
                {
                    WindowStyle = WindowStyle.None;
                    ResizeMode = ResizeMode.NoResize;
                    pW = Width;
                    pH = Height;
                    Width = scrW;
                    Height = scrH;
                    panel.Opacity = 1;
                    panel.IsEnabled = true;
                }
            }

            return IntPtr.Zero;
        }
    }
}
