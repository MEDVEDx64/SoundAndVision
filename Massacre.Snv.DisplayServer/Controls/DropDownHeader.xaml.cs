using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Massacre.Snv.DisplayServer.Controls
{
    public partial class DropDownHeader : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(DropDownHeader),
            new FrameworkPropertyMetadata(
                "", OnTextChanged
                )
            );

        static readonly double opacityDelta = 0.4;

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        
        public DropDownHeader()
        {
            InitializeComponent();
        }

        static void OnTextChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var obj = o as DropDownHeader;
            if(obj != null)
            {
                obj.textBlock.Text = (string)e.NewValue;
            }
        }

        private void OnHandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if(contentRow.Height.IsAuto)
            {
                contentRow.Height = new GridLength(0);
                handle.Opacity -= opacityDelta;
            }

            else
            {
                contentRow.Height = GridLength.Auto;
                handle.Opacity += opacityDelta;
            }
        }
    }
}
