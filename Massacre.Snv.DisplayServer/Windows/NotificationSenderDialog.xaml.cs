using Massacre.Snv.Core.Network.Packets;
using System.Windows;

namespace Massacre.Snv.DisplayServer.Windows
{
    public partial class NotificationSenderDialog : Window
    {
        Client client = null;
        const int maxMsgLen = 255;

        public NotificationSenderDialog(Client cli)
        {
            InitializeComponent();
            client = cli;
            Title += " – " + cli.Name;
        }

        private void OnSendButtonClick(object sender, RoutedEventArgs e)
        {
            if(textBox.Text.Length == 0)
            {
                MessageBox.Show("Can't send an empty message", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if(textBox.Text.Length > maxMsgLen)
            {
                MessageBox.Show("The message is too large (" + maxMsgLen + ')',
                    "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            client.SendPacket(new NotificationPacket()
            {
                Text = textBox.Text,
                SoundAlert = soundCheckBox.IsChecked == true
            }).Flush();

            Close();
        }
    }
}
