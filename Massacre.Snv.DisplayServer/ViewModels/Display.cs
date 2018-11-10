using DevExpress.Mvvm;
using Massacre.Snv.Core.Network.Packets;
using Massacre.Snv.Core.Stage2;
using Massacre.Snv.Core.Utils;
using Massacre.Snv.DisplayServer.Windows;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace Massacre.Snv.DisplayServer.ViewModels
{
    public class Display : S2Receiver, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public Client Origin { get; }
        public ImageSource Image { get; private set; }
        public uint SuggestedDenominator { get; private set; }
        public bool IsFullScreen { get; set; }

        public static readonly double DefaultWidth = 300;

        ushort sourceWidth = 0, sourceHeight = 0,
            lastWidth = 0, lastHeight = 0, controlWidth = (ushort)DefaultWidth;
        bool invalHeader = true;

        public string Header
        {
            get
            {
                var sb = new StringBuilder();
                var probe = "";
                var ff = new FontFamily("Arial");
                var ending = MakeHeader(null);
                foreach (var ch in Origin.Name)
                {
                    sb.Append(ch);
                    probe = sb.ToString() + "... " + ending;

                    var text = new FormattedText(
                        probe,
                        CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(ff, FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                        12,
                        Brushes.Black
                        );

                    if(text.Width > lastWidth - 32)
                    {
                        return probe;
                    }
                }

                return Origin.Name + ending;
            }
        }

        public string WindowHeader
        {
            get { return MakeHeader(Origin.Name); }
        }

        public string PrintableId
        {
            get { return (Origin.Id ?? Guid.Empty).ToString().Substring(0, 23); }
        }

        public Brush BadgeColor
        {
            get
            {
                var id = Origin.Id.ToString();
                return new SolidColorBrush(new Color()
                {
                    A = 0x80,
                    R = byte.Parse("" + id[0] + id[1], NumberStyles.HexNumber),
                    G = byte.Parse("" + id[2] + id[3], NumberStyles.HexNumber),
                    B = byte.Parse("" + id[4] + id[5], NumberStyles.HexNumber)
                });
            }
        }

        public DelegateCommand SendNotificationCommand { get; }

        public Display(ushort port, Client origin, ushort width, ushort height) : base(port)
        {
            Origin = origin;
            SuggestedDenominator = ScreenTransmitter.InitialDenominator;

            sourceWidth = width;
            sourceHeight = height;

            FrameAccepted += OnFrameAccepted;
            Origin.NameChanged += delegate
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Header)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowHeader)));
            };

            Origin.IdChanged += delegate { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BadgeColor))); };
            SendNotificationCommand = new DelegateCommand(delegate { new NotificationSenderDialog(Origin).ShowDialog(); });

            new Thread(() =>
            {
                Thread.Sleep(100);
                RestoreDenominator();
            }).Start();

            const string headerProp = nameof(Header);
            new Thread(() =>
            {
                while (Origin.IsRunning)
                {
                    try
                    {
                        Thread.Sleep(200);
                        if (invalHeader)
                        {
                            invalHeader = false;
                            AppTools.TryInvoke(() =>
                            {
                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(headerProp));
                            });
                        }
                    }

                    catch { }
                }
            }).Start();
        }

        private void OnFrameAccepted(object sender, FrameAcceptedEventArgs e)
        {
            if (e.IsImageChanged)
            {
                Image = e.Image;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
            }
        }

        public void OnActualSizeChanged(double w, double h, bool control = false)
        {
            var nw = (w < 8 ? 8 : w);
            var nh = (h < 8 ? 8 : h);

            if (nw != sourceWidth || nh != sourceHeight)
            {
                lastWidth = (ushort)nw;
                lastHeight = (ushort)nh;
            }

            if(control)
            {
                controlWidth = (ushort)nw;
            }

            var num = sourceWidth;
            var den = nw;

            if(nh < nw)
            {
                num = sourceHeight;
                den = nh;
            }

            if(den > num)
            {
                SuggestedDenominator = 1;
            }

            else
            {
                SuggestedDenominator = (uint)(num / den);
            }

            var values = Origin.GetDenominators();
            if (values.Count() > 0)
            {
                var sb = new StringBuilder();
                foreach(var d in values)
                {
                    sb.Append(d + "/");
                }

                sb.Remove(sb.Length - 1, 1);
                var vp = new ValuePacket();
                vp.Data["den"] = sb.ToString();
                Origin.SendPacket(vp).Flush();
            }

            invalHeader = true;
        }

        public void RestoreDenominator()
        {
            OnActualSizeChanged(controlWidth, controlWidth /* sic! */); // preview size
        }

        string MakeHeader(string name)
        {
            return (name ?? "") + (Origin.Displays.Count > 1 ? " (" + (Origin.Displays.IndexOf(this) + 1) + ")" : "");
        }
    }
}
