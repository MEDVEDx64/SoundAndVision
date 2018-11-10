using Massacre.Snv.Client.ViewModels;
using System;
using System.Windows;
using System.Windows.Forms;

namespace Massacre.Snv.Client.Windows
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(Action closeCallback)
        {
            InitializeComponent();
            Closed += delegate { closeCallback(); };
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            var settings = DataContext as EditableSettings;
            if(settings != null)
            {
                settings.Save();
            }

            Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpenRecDirectoryButtonClick(object sender, RoutedEventArgs e)
        {
            var settings = DataContext as EditableSettings;
            if (settings == null)
            {
                return;
            }

            using (var dialog = new FolderBrowserDialog())
            {
                var result = dialog.ShowDialog();
                if(result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    settings.RecPrefix = dialog.SelectedPath +
                        (dialog.SelectedPath[dialog.SelectedPath.Length - 1] != '\\' ? "\\" : "");
                    settings.OnExternalChange();
                }
            }
        }
    }
}
