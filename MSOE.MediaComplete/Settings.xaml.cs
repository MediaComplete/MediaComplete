using System;
using System.Windows;
using System.Windows.Forms;

namespace MSOE.MediaComplete
{
    /// <summary>
    ///     Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            txtboxSelectedFolder.Text = (string) Properties.Settings.Default["HomeDir"];
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
            Properties.Settings.Default["HomeDir"] = txtboxSelectedFolder.Text;
            Properties.Settings.Default.Save();
        }
    }
}