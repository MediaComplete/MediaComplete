using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MSOE.MediaComplete.Lib;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private readonly SettingPublisher _settingPublisher = new SettingPublisher();
        public Settings()
        {
            InitializeComponent();

            var homedir = (string)Properties.Settings.Default["HomeDir"];
            txtboxSelectedFolder.Text = homedir;
            txtboxInboxFolder.Text = (string)Properties.Settings.Default["InboxDir"];
            txtboxPollTime.Text = (string)Properties.Settings.Default["PollingTime"];
            checkboxPolling.IsChecked = ((bool)Properties.Settings.Default["isPolling"]);
            CheckBoxChanged(checkboxPolling, null);

            _settingPublisher.RaiseSettingEvent += HandleSettingChangeEvent;

        }

        private void HandleSettingChangeEvent(object sender, SettingChanged e)
        {
            Importer.Instance._homeDir = e.HomeDir;
        }

        /// <summary>
        /// The handler for the folder selection buttons of the setting screen.
        /// Handles where the path from the dialog will appear.
        /// </summary>
        /// <param name="sender">The sender of the action(the folder selection button)</param>
        /// <param name="e">Type of event</param>
        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            var folderBrowserDialog1 = new FolderBrowserDialog();
            var button = sender as System.Windows.Controls.Button;
            if (folderBrowserDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            if (button != null)
                switch (button.Name)
                {
                    case "btnSelectFolder":
                        txtboxSelectedFolder.Text = folderBrowserDialog1.SelectedPath;
                        break;
                    case "btnInboxFolder":
                        txtboxInboxFolder.Text = folderBrowserDialog1.SelectedPath;
                        break;
                }
        }

        /// <summary>
        /// The handler of the checkbox change event for the setting screen.
        /// Will enable or disable the polling related fields.
        /// </summary>
        /// <param name="sender">The sender of the action(the checkbox for polling)</param>
        /// <param name="e">Type of event</param>
        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {

            var button = sender as System.Windows.Controls.CheckBox;
            if (button != null && button.IsChecked == true)
            {
                txtboxInboxFolder.IsEnabled = true;
                txtboxPollTime.IsEnabled = true;
                btnInboxFolder.IsEnabled = true;
                lblPollTime.IsEnabled = true;
                lblMin.IsEnabled = true;
                lblSelectInboxLocation.IsEnabled = true;
            }
            else
            {
                txtboxInboxFolder.IsEnabled = false;
                txtboxPollTime.IsEnabled = false;
                btnInboxFolder.IsEnabled = false;
                lblPollTime.IsEnabled = false;
                lblMin.IsEnabled = false;
                lblSelectInboxLocation.IsEnabled = false;
            }
        }

        /// <summary>
        /// The handler for a save button click.
        /// Will save the values to the properties file.
        /// </summary>
        /// <param name="sender">The sender of the action</param>
        /// <param name="e">Type of event</param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // add settings here as they are added to the UI
            var homeDir = txtboxSelectedFolder.Text;
            if (!homeDir.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.CurrentCulture)))
            {
                homeDir += Path.DirectorySeparatorChar;
            }
            Properties.Settings.Default["HomeDir"] = homeDir;
            Properties.Settings.Default["InboxDir"] = txtboxInboxFolder.Text;
            Properties.Settings.Default["PollingTime"] = txtboxPollTime.Text;
            Properties.Settings.Default["isPolling"] = checkboxPolling.IsChecked;

            Properties.Settings.Default.Save();
            _settingPublisher.ChangeSetting(homeDir);

        }
    }
}
