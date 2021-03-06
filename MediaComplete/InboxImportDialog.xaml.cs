﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MediaComplete.Lib;
using System;
using MediaComplete.Lib.Background;
using MediaComplete.Lib.Import;
using MediaComplete.Lib.Library.DataSource;

namespace MediaComplete
{
    /// <summary>
    /// Interaction logic for InboxImportDialog.xaml
    /// </summary>
    public partial class InboxImportDialog
    {
        private static IEnumerable<SongPath> _files;
        private readonly IQueue _queue;
        private static InboxImportDialog _instance;
        private readonly IPolling _polling;

        /// <summary>
        /// initializes the component
        /// </summary>
        private InboxImportDialog()
        {
            InitializeComponent();
            _queue = Dependency.Resolve<IQueue>();
            _polling = Dependency.Resolve<IPolling>();
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
        public static void Prompt(Window newOwner, IEnumerable<SongPath> files)
        {
            _files = files;
            var inst = Instance(newOwner);
            inst.MessageTextBlock.Text = String.Format(newOwner.Resources["Dialog-Polling-Prompt"].ToString(), _files.Count());
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
        private void okButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            SettingWrapper.ShowInputDialog =!StopShowingCheckBox.IsChecked.GetValueOrDefault(false);

            //Do the move
            _queue.Add(new Importer(Dependency.Resolve<IFileSystem>(), _files, false));

            _polling.Reset();
            DialogResult = true;
        }

        /// <summary>
        /// sets the application to stop polling if the user elected to not be shown the pop up again
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            SettingWrapper.IsPolling = !StopShowingCheckBox.IsChecked.GetValueOrDefault((false));
            _polling.Reset();
            DialogResult = false;
            SettingWrapper.Save();
        }
    }
}
