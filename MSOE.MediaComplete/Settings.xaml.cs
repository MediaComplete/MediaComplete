using System;
using System.Collections.Generic;
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
            txtboxSelectedFolder.Text = (string)Properties.Settings.Default["HomeDir"];
            txtboxInboxFolder.Text = (string)Properties.Settings.Default["InboxDir"];
            txtboxPollTime.Text = (string)Properties.Settings.Default["PollingTime"];
            checkboxPolling.IsChecked = ((bool)Properties.Settings.Default["isPolling"]);
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
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
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
        }

        /// <summary>
        /// The handler of the checkbox change event for the setting screen.
        /// Will enable or disable the polling related fields.
        /// </summary>
        /// <param name="sender">The sender of the action(the checkbox for polling)</param>
        /// <param name="e">Type of event</param>
        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {

            System.Windows.Controls.CheckBox button = sender as System.Windows.Controls.CheckBox;
            if (button.IsChecked == true)
            {
                txtboxInboxFolder.IsEnabled = true;
                txtboxPollTime.IsEnabled = true;
                btnInboxFolder.IsEnabled = true;
            }
            else
            {
                txtboxInboxFolder.IsEnabled = false;
                txtboxPollTime.IsEnabled = false;
                btnInboxFolder.IsEnabled = false;
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
            Properties.Settings.Default["HomeDir"] = txtboxSelectedFolder.Text;
            Properties.Settings.Default["InboxDir"] = txtboxInboxFolder.Text;
            Properties.Settings.Default["PollingTime"] = txtboxPollTime.Text;
            Properties.Settings.Default["isPolling"] = checkboxPolling.IsChecked;
            Properties.Settings.Default.Save();
        }
    }
}
