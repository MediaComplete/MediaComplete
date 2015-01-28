using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using MSOE.MediaComplete.Lib;
using System;
using MSOE.MediaComplete.Lib.Import;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Interaction logic for InboxImportDialog.xaml
    /// </summary>
    public partial class InboxImportDialog
    {
        private static IEnumerable<FileInfo> _files;
        private static InboxImportDialog _instance;

        /// <summary>
        /// initializes the component
        /// </summary>
        private InboxImportDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// gets the instance of the dialog
        /// </summary>
        /// <param name="owner">owner to set on the new dialog so it can be modal</param>
        /// <returns></returns>
        private static InboxImportDialog Instance(Window owner)
        {
            if (_instance == null || !_instance.IsLoaded)// IsLoaded == false iff dialog not ready to be shown or already closed (so we need to init one)
            {
                _instance = new InboxImportDialog { Owner = owner };
            }
            return _instance;
        }

        /// <summary>
        /// sets the text properly based on the number of files and shows the dialog if it is not already shown
        /// </summary>
        /// <param name="newOwner"></param>
        /// <param name="files"></param>
        public static void Prompt(Window newOwner, IEnumerable<FileInfo> files)
        {
            _files = files;
            var inst = Instance(newOwner);
            inst.MessageTextBlock.Text = "Found " + _files.Count() + " file(s).\nWould you like to import them now?";
            if (!inst.IsVisible)
            {
                inst.ShowDialog();
            }
        }

        /// <summary>
        /// sets the user's choice to be shown the pop up or not, then fires the event to move the files into the library
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void okButton_Click(object sender, RoutedEventArgs e)
        {
            SettingWrapper.SetShowInputDialog(!StopShowingCheckBox.IsChecked.GetValueOrDefault(false));

            //Do the move
            var results = await new Importer(SettingWrapper.GetMusicDir()).ImportFiles(_files.Select(f => new FileInfo(f.FullName)).ToList(), false);
            if (results.FailCount > 0)
            {
                try
                {
                    MessageBox.Show(this,
                        String.Format(Resources["Dialog-Import-ItemsFailed-Message"].ToString(), results.FailCount),
                        Resources["Dialog-Common-Warning-Title"].ToString(),
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                catch (NullReferenceException)
                {
                    StatusBarHandler.Instance.ChangeStatusBarMessage("FailedImport-Error", StatusBarHandler.StatusIcon.Error);
                }
            }

            Polling.Instance.Reset();
            DialogResult = true;
        }

        /// <summary>
        /// sets the application to stop polling if the user elected to not be shown the pop up again
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            SettingWrapper.SetIsPolling(!StopShowingCheckBox.IsChecked.GetValueOrDefault((false)));
            Polling.Instance.Reset();
            DialogResult = false;
            SettingWrapper.Save();
        }
    }
}
