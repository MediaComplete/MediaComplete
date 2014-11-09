﻿using System;
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
            _settingPublisher.RaiseSettingEvent += HandleSettingChangeEvent;

        }

        private void HandleSettingChangeEvent(object sender, SettingChanged e)
        {
            Importer.Instance.HomeDir = e.HomeDir;
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            var folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtboxSelectedFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // add settings here as they are added to the UI
            var homeDir = txtboxSelectedFolder.Text;
            if (!homeDir.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.CurrentCulture)))
            {
                homeDir += Path.DirectorySeparatorChar;
            }
            Properties.Settings.Default["HomeDir"] = homeDir;
            Properties.Settings.Default.Save();
            _settingPublisher.ChangeSetting(homeDir);

            Directory.CreateDirectory(homeDir);

        }
    }
}
