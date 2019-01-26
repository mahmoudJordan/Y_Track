using System.Windows;
using System.Windows.Controls;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Y_Track.UserControls.MenuItemsUserControls
{
    /// <summary>
    /// Interaction logic for SettingsUserControl.xaml
    /// </summary>
    public partial class SettingsUserControl : UserControl
    {


        public SettingsUserControl()
        {
            InitializeComponent();
            DataContext = Properties.Settings.Default;
        }



        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // default output directory
            if (!Directory.Exists(this.VideoDirectoryPath.Text))
            {
                MessageBox.Show("Error", "New Videos Store Location Is Incorrect", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Properties.Settings.Default.OutputDirectory = this.VideoDirectoryPath.Text;

            // database dorectory
            if (!Directory.Exists(this.DatabaseDirectory.Text))
            {
                MessageBox.Show("Videos Database Location Is Incorrect", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Properties.Settings.Default.DatabaseDirectory = this.DatabaseDirectory.Text;

            // run on startup 
            Helpers.WindowsHelper.ToggleStartAtStartup(this.RunOnStartUp.IsChecked.Value);


            // NotificationMessageTimeOut
            bool nmtParsed = int.TryParse(NotificationMessageTimeOut.Text, out var nmt);
            if (!nmtParsed)
            {
                MessageBox.Show("Notification Message TimeOut must be a valid number", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (nmt < 300 || nmt > 20000)
            {
                MessageBox.Show("Notification Message TimeOut must be a within the range of 30 - 20000", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Properties.Settings.Default.NotificationMessageTimeout = nmt;


            // NotificationDialogTimeOut
            bool ndtParsed = int.TryParse(NotificationDialogTimeout.Text, out var ndt);
            if (!ndtParsed)
            {
                MessageBox.Show("Notification Dialog TimeOut must be a valid number", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (ndt < 300 || ndt > 20000)
            {
                MessageBox.Show("Notification Message Dialog must be a within the range of 30 - 20000", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Properties.Settings.Default.NotificationDialogTimeout = ndt;

            // DefaultProxyPort
            bool dppParsed = int.TryParse(DefaultProxyPort.Text, out var dpp);
            if (!dppParsed)
            {
                MessageBox.Show("Default Proxy must be a valid number", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (dpp != 0)
            {
                if (dpp < 11000 || dpp > 65000)
                {
                    MessageBox.Show("Default Proxt must be a within the range of 11000 - 65000", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
              
            Properties.Settings.Default.DefaultProxyPort = dpp;


            //ReencodeMediaFallback
            Properties.Settings.Default.FallBackToEncodeMedia = this.ReencodeMediaFallback.IsChecked.Value;


            // draggable notification panel
            Properties.Settings.Default.NotificationDraggable = this.DraggableNotification.IsChecked.Value;


            // NumberOfStartUpVideos
            bool nosvParsed = int.TryParse(NumberOfStartupVideosTextBox.Text, out var nosv);
            if (!nosvParsed)
            {
                MessageBox.Show("Error", "Number Of Startup Videos be a valid number", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Properties.Settings.Default.NumberOfVideosShowAtStartup = nosv;



            Properties.Settings.Default.Save();

            NotificationProvider.Instance.ShowMessageNotification("Saved Successfully", "New Configureation Are Save Successfully", NotificationMessageType.Reminder);

        }

        private void OutputDirectoryBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.OutputDirectory = _showUserDialog();
            Properties.Settings.Default.Save();

        }

        private void DatabaseDirectoryBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DatabaseDirectory = _showUserDialog();
            Properties.Settings.Default.Save();
        }

        private string _showUserDialog()
        {
            // TODO:: check write permessions
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var selected = dialog.SelectedPath;
                    if (Directory.Exists(selected))
                    {
                        return dialog.SelectedPath; ;

                    }
                }
            }
            return null;
        }



        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
