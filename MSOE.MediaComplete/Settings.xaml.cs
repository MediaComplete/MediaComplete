using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using MSOE.MediaComplete.Lib;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private SettingPublisher settingPublisher;
        public Settings()
        {
            InitializeComponent();
            var homedir = (string)Properties.Settings.Default["HomeDir"];
            txtboxSelectedFolder.Text = homedir;
            settingPublisher = new SettingPublisher();
            settingPublisher.RaiseSettingEvent += HandleSettingChangeEvent;

        }

        private void HandleSettingChangeEvent(object sender, SettingChanged e)
        {
            Importer.Instance._homeDir = e.HomeDir;
            Console.WriteLine("The home Dir has been changed to: " + e.HomeDir);
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtboxSelectedFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // add settings here as they are added to the UI
            var homeDir = txtboxSelectedFolder.Text;
            if (!homeDir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            {
                homeDir += System.IO.Path.DirectorySeparatorChar;
            }
            Properties.Settings.Default["HomeDir"] = homeDir;
            Properties.Settings.Default.Save();
            settingPublisher.ChangeSetting(homeDir);

            Directory.CreateDirectory(homeDir);

        }
    }
}
