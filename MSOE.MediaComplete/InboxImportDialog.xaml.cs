using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using MSOE.MediaComplete.Lib;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Interaction logic for InboxImportDialog.xaml
    /// </summary>
    public partial class InboxImportDialog
    {
        private static IEnumerable<FileInfo> _files;
        private static InboxImportDialog _instance;


        private InboxImportDialog()
        {
            InitializeComponent();
        }

        private static InboxImportDialog Instance(Window owner)
        {
            if (_instance == null || !_instance.IsLoaded)// IsLoaded == false iff dialog not ready to be shown or already closed (so we need to init one)
            {
                _instance = new InboxImportDialog {Owner = owner};
            }
            return _instance;
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
            SettingWrapper.SetShowInputDialog(!StopShowingCheckBox.IsChecked.GetValueOrDefault(false));
            await Importer.Instance.ImportFiles(_files.Select(f => f.FullName).ToArray(), false);
            DialogResult = true;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            SettingWrapper.SetIsPolling(!StopShowingCheckBox.IsChecked.GetValueOrDefault((false)));
            DialogResult = false;
        }
    }
}
