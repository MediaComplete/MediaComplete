using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using MSOE.MediaComplete.Lib;
using Application = System.Windows.Application;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string Mp3FileFormat = "MP3 Files (*.mp3)|*.mp3";
        private const string FileDialogTitle = "Select Music File(s)";
        private readonly string _homeDir;
        private bool inputDialogShown = false;

        public MainWindow()
        {
            InitializeComponent();
            _homeDir = SettingWrapper.GetHomeDir();
            Importer.Instance.HomeDir = _homeDir;
			
            Directory.CreateDirectory(_homeDir);

            if (SettingWrapper.GetIsPolling())
            {
                Polling.Instance.TimeInMinutes = SettingWrapper.GetPollingTime();
                Polling.Instance.inboxDir = SettingWrapper.GetInboxDir();
                Polling.Instance.Start();
            }
            Polling.InboxFilesDetected += ImportFromInbox;
            InitTreeView();
        }

        private async void ImportFromInbox(IEnumerable<FileInfo> files)
        {
            if (SettingWrapper.GetShowInputDialog())
            {
                inputDialogShown = true;
                Dispatcher.BeginInvoke(new Action(() => InboxImportDialog.Prompt(this, files)));
            }
            else
            {
                await Importer.Instance.ImportFiles(files.Select(f => f.FullName).ToArray(), false);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            new Settings().Show();
        }

        private async void AddFile_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = Mp3FileFormat,
                InitialDirectory = "C:",
                Title = FileDialogTitle,
                Multiselect = true
            };
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                await Importer.Instance.ImportFiles(fileDialog.FileNames, true);
            }

        }

        private async void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var selectedDir = folderDialog.SelectedPath;
                await Importer.Instance.ImportDirectory(selectedDir, true);
                
            }
        }

        public void RefreshTreeView(object source, FileSystemEventArgs e)
        {
            LibraryTree.Items.Clear();

            var rootDirInfo = new DirectoryInfo(_homeDir);

            LibraryTree.Items.Add(CreateDirectoryItem(rootDirInfo));
        }

        public void RefreshTreeView()
        {
            LibraryTree.Items.Clear();

            var rootDirInfo = new DirectoryInfo(_homeDir);

            LibraryTree.Items.Add(CreateDirectoryItem(rootDirInfo));
        }

        private void InitTreeView()
        {
            RefreshTreeView();

            var watcher = new FileSystemWatcher(_homeDir)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnChanged;

            watcher.EnableRaisingEvents = true;
        }

        private static TreeViewItem CreateDirectoryItem(DirectoryInfo dirInfo)
        {
            var dirItem = new TreeViewItem { Header = dirInfo.Name };
            foreach (var dir in dirInfo.GetDirectories())
            {
                dirItem.Items.Add(CreateDirectoryItem(dir));
            }

            foreach (var file in dirInfo.GetFiles())
            {
                dirItem.Items.Add(new TreeViewItem { Header = file.Name });
            }

            return dirItem;
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var win = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (win != null) win.RefreshTreeView();
            }));
            
        }
        
    }
}
