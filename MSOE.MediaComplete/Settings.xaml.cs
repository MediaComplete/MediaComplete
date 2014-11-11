using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
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
        
        public Settings()
        {
            InitializeComponent();

            txtboxSelectedFolder.Text = SettingWrapper.GetHomeDir();
            txtboxInboxFolder.Text = SettingWrapper.GetInboxDir();
            comboBox.SelectedValue = SettingWrapper.GetPollingTime().ToString();
            checkboxPolling.IsChecked = SettingWrapper.GetIsPolling();
            CheckBoxChanged(checkboxPolling, null);
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
                comboBox.IsEnabled = true;
                btnInboxFolder.IsEnabled = true;
                lblPollTime.IsEnabled = true;
                lblMin.IsEnabled = true;
                lblSelectInboxLocation.IsEnabled = true;
            }
            else
            {
                txtboxInboxFolder.IsEnabled = false;
                comboBox.IsEnabled = false;
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

            SettingWrapper.SetHomeDir(homeDir);
            SettingWrapper.SetInboxDir(txtboxInboxFolder.Text);
            SettingWrapper.SetPollingTime(comboBox.SelectedValue);
            SettingWrapper.SetIsPolling(checkboxPolling.IsChecked.GetValueOrDefault(false));
            SettingWrapper.Save();
			
            Polling.Instance.PollingChanged(Convert.ToDouble(comboBox.SelectedValue), txtboxInboxFolder.Text);
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs args)
        {
            var dataList = new List<string> {"0.5", "1", "5", "10", "30", "60", "120", "240"};

            var box = sender as System.Windows.Controls.ComboBox;
            if (box != null) box.ItemsSource = dataList;
        }
    }
}
