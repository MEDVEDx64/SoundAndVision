using System.Windows;
using System;
using System.Windows.Controls;
using System.Threading;
using Massacre.Snv.DisplayServer.ViewModels;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace Massacre.Snv.DisplayServer
{
    public partial class MainWindow : Window
    {
        Server srv = null;
        bool running = true;
        static MainWindow wnd = null;
        double lastScrollOffset = 0;

        public MainWindow()
        {
            wnd = this;

            InitializeComponent();
            srv = new Server();
            DataContext = srv;
            Closed += OnWindowClosed;

            ConfigureScaleSlider();
            Activated += ((sender, e) =>
            {
                WindowStyle = WindowStyle.None;
            });

            // Date/time textblock updating thread
            new Thread(() =>
            {
                while(running)
                {
                    try
                    {
                        Dispatcher?.Invoke(() =>
                        {
                            dateTimeTextBlock.Text = DateTime.Now.ToString();
                        });
                    }

                    catch { }
                    Thread.Sleep(250);
                }
            }).Start();
        }

        private void ConfigureScaleSlider()
        {
            scaleSlider.Minimum = 128;
            scaleSlider.Maximum = 512;
            scaleSlider.ValueChanged += OnScaleSliderValueChanged;
            scaleSlider.Value = Display.DefaultWidth;
        }

        private void OnScaleSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var clients = srv.Clients.ToList();
            foreach(var c in clients)
            {
                var displays = c.Displays.ToList();
                foreach(var d in displays)
                {
                    if(!d.IsFullScreen)
                    {
                        d.OnActualSizeChanged(e.NewValue, e.NewValue /* sic! */, true);
                    }
                }
            }
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            foreach (var cl in srv.Clients)
            {
                cl.Shutdown();
            }

            running = false;
        }

        private void OnEventLogChanged(object sender, TextChangedEventArgs e)
        {
            if (eventLogTextBox.VerticalOffset + eventLogTextBox.ViewportHeight > eventLogTextBox.ExtentHeight - 48)
            {
                eventLogTextBox.ScrollToEnd();
            }

            else
            {
                eventLogTextBox.ScrollToVerticalOffset(lastScrollOffset);
            }
        }

        private void OnEventLogScrollChanged(object sender, RoutedEventArgs e)
        {
            lastScrollOffset = eventLogTextBox.VerticalOffset;
        }

        private void OnHeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        // Custom button set code

        private void OnMinimizeButtonMouseEnter(object sender, MouseEventArgs e)
        {
            minimizeButton.Opacity = 1;
        }

        private void OnMinimizeButtonMouseLeave(object sender, MouseEventArgs e)
        {
            minimizeButton.Opacity = 0;
        }

        private void OnMinimizeButtonMouseUp(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnMaximizeButtonMouseEnter(object sender, MouseEventArgs e)
        {
            maximizeButton.Opacity = 1;
        }

        private void OnMaximizeButtonMouseLeave(object sender, MouseEventArgs e)
        {
            maximizeButton.Opacity = 0;
        }

        private void OnMaximizeButtonMouseUp(object sender, MouseButtonEventArgs e)
        {
            if(WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void OnCloseButtonMouseEnter(object sender, MouseEventArgs e)
        {
            closeButton.Opacity = 1;
        }

        private void OnCloseButtonMouseLeave(object sender, MouseEventArgs e)
        {
            closeButton.Opacity = 0;
        }

        private void OnCloseButtonMouseUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void OnButtonMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowStyle = WindowStyle.None;
            }
        }

        // Window header double-click behavior

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (VisualTreeHelper.HitTest(doubleClickArea, e.GetPosition(this)) != null)
            {
                OnMaximizeButtonMouseUp(sender, e);
            }
        }
    }
}
