using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Massacre.Snv.Core.Windows
{
    public partial class ExceptionWindow : Window
    {
        public ExceptionWindow(object exceptionObject)
        {
            InitializeComponent();
            text.Text = exceptionObject?.ToString() ?? "<undetermined exception>";

            var icon = SystemIcons.Error;
            var source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            iconImage.Source = source;
        }

        private void OnExitButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
