using System;
using System.Windows.Media;

namespace Massacre.Snv.Core.Stage2
{
    public class FrameAcceptedEventArgs : EventArgs
    {
        public ImageSource Image { get; }
        public bool IsImageChanged { get; }

        public FrameAcceptedEventArgs(ImageSource image, bool changed)
        {
            Image = image;
            IsImageChanged = changed;
        }
    }
}
