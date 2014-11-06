using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using MSOE.MediaComplete.Lib;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly String _homeDir;

        public MainWindow()
        {
            InitializeComponent();
            _homeDir = (string) Properties.Settings.Default["HomeDir"];
            if (_homeDir.EndsWith("\\"))
            {
                _homeDir += "library\\";
            }
            else
            {
                _homeDir += "\\library\\";
            }

            Directory.CreateDirectory(_homeDir);
            initTreeView();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            new Settings().Show();
        }

        private void AddFile_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = "MP3 Files (*.mp3)|" + Constants.MusicFilePattern,
                InitialDirectory = Path.GetPathRoot(Environment.SystemDirectory),
                Title = "Select Music File(s)",
                Multiselect = true
            };

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (String file in fileDialog.FileNames)
                {
                    try
                    {
                        System.IO.File.Copy(file.ToString(), _homeDir + System.IO.Path.GetFileName(file));
                        //Console.WriteLine(homeDir + System.IO.Path.GetFileName(file));
                    }
                    catch (Exception exception)
                    {
                        System.Console.WriteLine(exception);
                    }
                }
            }
        }

        private void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                String selectedDir = folderDialog.SelectedPath;
                String[] files = Directory.GetFiles(selectedDir, "*.mp3",
                    SearchOption.AllDirectories);
                foreach (String file in files)
                {
                    try
                    {
                        System.IO.File.Copy(file.ToString(),
                            _homeDir + System.IO.Path.GetFileName(file));

                        //Console.WriteLine(homeDir + System.IO.Path.GetFileName(file));
                    }
                    catch (Exception exception)
                    {
                        System.Console.WriteLine(exception);
                    }
                }
            }
        }

        public void refreshTreeView(object source, FileSystemEventArgs e)
        {
            LibraryTree.Items.Clear();

            var rootDirInfo = new DirectoryInfo(_homeDir);

            LibraryTree.Items.Add(CreateDirectoryItem(rootDirInfo));
        }

        public void refreshTreeView()
        {
            LibraryTree.Items.Clear();

            var rootDirInfo = new DirectoryInfo(_homeDir);

            LibraryTree.Items.Add(CreateDirectoryItem(rootDirInfo));
        }

        private void initTreeView()
        {
            refreshTreeView();

            var watcher = new FileSystemWatcher(_homeDir);
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnChanged);

            watcher.EnableRaisingEvents = true;
        }

        private static TreeViewItem CreateDirectoryItem(DirectoryInfo dirInfo)
        {
            var dirItem = new TreeViewItem {Header = dirInfo.Name};
            foreach (var dir in dirInfo.GetDirectories())
            {
                dirItem.Items.Add(CreateDirectoryItem(dir));
            }

            foreach (var file in dirInfo.GetFiles())
            {
                dirItem.Items.Add(new TreeViewItem {Header = file.Name});
            }

            return dirItem;
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                var win = App.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                win.refreshTreeView();
            }));
        }

        //private static bool CtrlPressed()
        //{
        //    return System.Windows.Input.Keyboard.IsKeyDown(Key.LeftCtrl) || System.Windows.Input.Keyboard.IsKeyDown(Key.RightCtrl);
        //}

        //private void LibraryTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        //{
        //    Console.WriteLine("Sender: " + sender);

        //    TreeViewItem selectedItem = (TreeViewItem) e.NewValue;
        //}
    }
}