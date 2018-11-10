using Massacre.Snv.DisplayServer.ViewModels;
using Massacre.Snv.DisplayServer.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Massacre.Snv.DisplayServer.Controls
{
    public partial class DisplayControl : UserControl
    {
        static List<FullScreenDisplayWindow> windows = new List<FullScreenDisplayWindow>();

        public DisplayControl()
        {
            InitializeComponent();
        }

        private void OnFullScreenIconMouseUp(object sender, MouseButtonEventArgs e)
        {
            var display = DataContext as Display;
            if (display != null)
            {
                Window found = null;
                var windowsCopy = windows.ToList();
                foreach(var w in windowsCopy)
                {
                    if(w.Display == display)
                    {
                        found = w;
                        break;
                    }
                }

                if(found == null)
                {
                    var dw = new FullScreenDisplayWindow(display);
                    windows.Add(dw);
                    dw.Closed += delegate
                    {
                        windows.Remove(dw);
                    };

                    dw.Show();
                }

                else
                {
                    found.Focus();
                }
            }
        }
    }
}
