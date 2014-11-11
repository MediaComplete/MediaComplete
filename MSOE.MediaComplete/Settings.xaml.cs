﻿using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using WinForms = System.Windows.Forms;
using MSOE.MediaComplete.Lib;
using System.Windows.Controls;

namespace MSOE.MediaComplete
{
    /// <summary>
    ///     Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings
    {
        private readonly SettingPublisher _settingPublisher = new SettingPublisher();
        public Settings()
        {
            InitializeComponent();

            var homedir = (string)Properties.Settings.Default["HomeDir"];
            TxtboxSelectedFolder.Text = homedir;
            txtboxInboxFolder.Text = (string)Properties.Settings.Default["InboxDir"];
            comboBox.SelectedValue = Properties.Settings.Default["PollingTime"];
            checkboxPolling.IsChecked = ((bool)Properties.Settings.Default["isPolling"]);
            CheckBoxChanged(checkboxPolling, null);

            _settingPublisher.RaiseSettingEvent += HandleSettingChangeEvent;

        }

        private void HandleSettingChangeEvent(object sender, SettingChanged e)
        {
            Importer.Instance.HomeDir = e.HomeDir;
        }

        /// <summary>
        /// The handler for the folder selection buttons of the setting screen.
        /// Handles where the path from the dialog will appear.
        /// </summary>
        /// <param name="sender">The sender of the action(the folder selection button)</param>
        /// <param name="e">Type of event</param>
        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            var folderBrowserDialog1 = new WinForms.FolderBrowserDialog();
            var button = sender as Button;
            if (folderBrowserDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            if (button != null)
                switch (button.Name)
                {
                    case "btnSelectFolder":
                        TxtboxSelectedFolder.Text = folderBrowserDialog1.SelectedPath;
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

            var button = sender as CheckBox;
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
            var homeDir = TxtboxSelectedFolder.Text;
            if (!homeDir.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.CurrentCulture)))
            {
                homeDir += Path.DirectorySeparatorChar;
            }
            Properties.Settings.Default["HomeDir"] = homeDir;
            Properties.Settings.Default["InboxDir"] = txtboxInboxFolder.Text;
            Properties.Settings.Default["PollingTime"] = comboBox.SelectedValue;
            Properties.Settings.Default["isPolling"] = checkboxPolling.IsChecked;

            Properties.Settings.Default.Save();
			
            _settingPublisher.ChangeSetting(homeDir);

            Polling.Instance.PollingChanged(Convert.ToDouble(comboBox.SelectedValue), txtboxInboxFolder.Text);
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs args)
        {
            var dataList = new List<string> {".5", "1", "5", "10", "30", "60", "120", "240"};

            var box = sender as ComboBox;
            if (box != null) box.ItemsSource = dataList;
        }
    }
}
