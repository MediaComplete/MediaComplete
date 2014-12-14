using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
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
using MSOE.MediaComplete.Lib;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Interaction logic for InboxImportDialog.xaml
    /// </summary>
    public partial class InboxImportDialog : Window
    {
        private readonly IEnumerable<FileInfo> _files;

        public InboxImportDialog(IEnumerable<FileInfo> files)
        {
            InitializeComponent();
            _files = files;
            MessageTextBlock.Text = "Found " + _files.Count() + " file(s).\nWould you like to import them now?";
        }

        private async void okButton_Click(object sender, RoutedEventArgs e)
        {
            //apply to settings if they choose to not show again
            SettingWrapper.SetShowInputDialog(!StopShowingCheckBox.IsChecked.GetValueOrDefault(false));
            //Do the move
            var results = await new Importer().ImportFiles(_files.Select(f => f.FullName).ToArray(), false);
            if (results.FailCount > 0)
            {
                MessageBox.Show(this,
                    String.Format(Resources["Dialog-Import-ItemsFailed-Message"].ToString(), results.FailCount),
                    Resources["Dialog-Common-Warning-Title"].ToString(),
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            DialogResult = true;
        }


        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
