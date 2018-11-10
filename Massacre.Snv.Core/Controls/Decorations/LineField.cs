using System.Windows;
using System.Windows.Media;

namespace Massacre.Snv.Core.Controls.Decorations
{
    public class LineField : FrameworkElement
    {
        public static readonly DependencyProperty DistanceProperty = DependencyProperty.Register(
            nameof(Distance),
            typeof(int),
            typeof(LineField),
            new FrameworkPropertyMetadata(
                4, FrameworkPropertyMetadataOptions.AffectsRender
                )
            );

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            nameof(Color),
            typeof(Color),
            typeof(LineField),
            new FrameworkPropertyMetadata(
                new Color() { R = 0, G = 0, B = 0, A = 255 },
                FrameworkPropertyMetadataOptions.AffectsRender
                )
            );

        public static readonly DependencyProperty BorderProperty = DependencyProperty.Register(
            nameof(Border),
            typeof(bool),
            typeof(LineField),
            new FrameworkPropertyMetadata(
                false, FrameworkPropertyMetadataOptions.AffectsRender
                )
            );

        public int Distance
        {
            get { return (int)GetValue(DistanceProperty); }
            set { SetValue(DistanceProperty, value); }
        }

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public bool Border
        {
            get { return (bool)GetValue(BorderProperty); }
            set { SetValue(BorderProperty, value); }
        }

        protected override void OnRender(DrawingContext ctx)
        {
            base.OnRender(ctx);
            ctx.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, ActualWidth, ActualHeight));

            var col = Color;
            var pen = new Pen(new SolidColorBrush(col), 1);

            ctx.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));
            for (int i = (int)-ActualHeight; i <= ActualWidth; i += Distance)
            {
                ctx.DrawLine(pen, new Point(i + ActualHeight, 0), new Point(i, ActualHeight));
            }

            if (Border)
            {
                ctx.DrawRectangle(null, pen, new Rect(new Point(0, 0), new Point(ActualWidth, ActualHeight)));
            }
        }
    }
}
