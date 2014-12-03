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
        private static IEnumerable<FileInfo> _files;
        private static InboxImportDialog _instance;


        private InboxImportDialog()
        {
            InitializeComponent();
        }

        private static InboxImportDialog Instance(Window owner)
        {
            return _instance ?? (_instance = new InboxImportDialog {Owner = owner});
        }
        
        public static void Prompt(Window newOwner, IEnumerable<FileInfo> files)
        {
            _files = files;
            var inst = Instance(newOwner);
            inst.MessageTextBlock.Text = "Found " + _files.Count() + " file(s).\nWould you like to import them now?";
            if(!inst.IsVisible)
            {
                inst.ShowDialog();
            }
        }

        private async void okButton_Click(object sender, RoutedEventArgs e)
        {
            //apply to settings if they choose to not show again
            SettingWrapper.SetShowInputDialog(!StopShowingCheckBox.IsChecked.GetValueOrDefault(false));
            //Do the move
            await Importer.Instance.ImportFiles(_files.Select(f => f.FullName).ToArray(), false);

            DialogResult = true;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
