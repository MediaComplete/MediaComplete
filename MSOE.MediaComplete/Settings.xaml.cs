using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using MSOE.MediaComplete.Lib;
using MSOE.MediaComplete.Lib.Logging;
using MSOE.MediaComplete.Lib.Files;
using MSOE.MediaComplete.Lib.Sorting;
using ComboBox = System.Windows.Controls.ComboBox;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings
    {
        private readonly Dictionary<LayoutType, string> _layoutsDict = new Dictionary<LayoutType, string>
        {
            {LayoutType.Dark, "..\\layout\\Dark.xaml"},
            {LayoutType.Pink, "..\\layout\\Pink.xaml"}
        };
        private LayoutType _changedType;
        private bool _layoutHasChanged;
        private readonly List<string> _allDirs;
        private readonly IFileManager _fileManager;
        public Settings()
        {
            InitializeComponent();
            TxtboxSelectedFolder.Text = SettingWrapper.HomeDir.FullPath;
            TxtInboxFolder.Text = SettingWrapper.InboxDir;
            ComboBoxPollingTime.SelectedValue = SettingWrapper.PollingTime.ToString(CultureInfo.InvariantCulture);
            CheckboxPolling.IsChecked = SettingWrapper.IsPolling;
            CheckboxShowImportDialog.IsChecked = SettingWrapper.ShowInputDialog;
            CheckBoxSorting.IsChecked = SettingWrapper.IsSorting;
            // TODO MC-308 Commented out until we figure out how to get an installed app logging
            //CheckBoxInfoLogging.IsChecked = SettingWrapper.LogLevel!=0;
            Logger.SetLogLevel(SettingWrapper.LogLevel);
            MoveOrCopy.IsChecked = SettingWrapper.ShouldRemoveOnImport;
            _allDirs = SettingWrapper.AllDirectories;
            _fileManager = FileManager.Instance;
            PollingCheckBoxChanged(CheckboxPolling, null);
            if (SettingWrapper.Layout.Equals(_layoutsDict[LayoutType.Pink]))
            {
                PinkCheck.IsChecked = true;
            }
            else if (SettingWrapper.Layout.Equals(_layoutsDict[LayoutType.Dark]))
            {
                DarkCheck.IsChecked = true;
            }
            _comboBoxes = new List<ComboBox>();
            _sortOrderList= SortHelper.ConvertToString(SettingWrapper.SortOrder);
            _showComboBox = new List<bool>();
            LoadComboBoxes();
            LoadSortListBox();
        }


        /// <summary>
        /// The handler for the folder selection buttons of the setting screen.
        /// Handles where the path from the dialog will appear.
        /// </summary>
        /// <param name="sender">The sender of the action(the folder selection button)</param>
        /// <param name="e">Type of event</param>
        private void BtnSelectFolder_Click(object sender, EventArgs e)
        {
            var folderBrowserDialog1 = new FolderBrowserDialog
            {
                SelectedPath = SettingWrapper.InboxDir
            };
            if (folderBrowserDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            TxtInboxFolder.Text = folderBrowserDialog1.SelectedPath;
        }

        private void BtnSelectHomeFolder_Click(object sender, RoutedEventArgs e)
        {
            var homeDirChooser = new FolderBrowserDialog
            {
                Description = Properties.Resources.Description,
                SelectedPath = SettingWrapper.HomeDir.FullPath
            };

            if (homeDirChooser.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            var homeDir = homeDirChooser.SelectedPath;
            if (homeDir != null && !homeDir.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.CurrentCulture), StringComparison.Ordinal))
            {
                homeDir += Path.DirectorySeparatorChar;
            }
            SettingWrapper.HomeDir = new DirectoryPath(homeDir);

            if (!_allDirs.Contains(SettingWrapper.HomeDir.FullPath))
            {
                var tempPath = new DirectoryPath(SettingWrapper.HomeDir.FullPath + "temp"+ new Random().Next());
                _fileManager.MoveDirectory(SettingWrapper.HomeDir, tempPath);
                _fileManager.MoveDirectory(tempPath, SettingWrapper.MusicDir);
                _allDirs.Add(SettingWrapper.HomeDir.FullPath);
            }

            TxtboxSelectedFolder.Text = SettingWrapper.HomeDir.FullPath;
        }

        /// <summary>
        /// The handler of the checkbox change event for the setting screen.
        /// Will enable or disable the polling related fields.
        /// </summary>
        /// <param name="sender">The sender of the action(the checkbox for polling)</param>
        /// <param name="e">Type of event</param>
        private void PollingCheckBoxChanged(object sender, RoutedEventArgs e)
        {

            var button = sender as System.Windows.Controls.CheckBox;
            if (button != null && button.IsChecked == true)
            {
                CheckboxShowImportDialog.IsEnabled = true;
                TxtInboxFolder.IsEnabled = true;
                ComboBoxPollingTime.IsEnabled = true;
                BtnInboxFolder.IsEnabled = true;
                LblPollTime.IsEnabled = true;
                LblMin.IsEnabled = true;
                LblSelectInboxLocation.IsEnabled = true;
            }
            else
            {
                CheckboxShowImportDialog.IsEnabled = false;
                TxtInboxFolder.IsEnabled = false;
                ComboBoxPollingTime.IsEnabled = false;
                BtnInboxFolder.IsEnabled = false;
                LblPollTime.IsEnabled = false;
                LblMin.IsEnabled = false;
                LblSelectInboxLocation.IsEnabled = false;
            }
        }

        /// <summary>
        /// The handler for a save button click.
        /// Will save the values to the properties file.
        /// </summary>
        /// <param name="sender">The sender of the action</param>
        /// <param name="e">Type of event</param>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var inboxDir = TxtInboxFolder.Text;
            if (!inboxDir.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.CurrentCulture), StringComparison.Ordinal))
            {
                inboxDir += Path.DirectorySeparatorChar;
            }

            if (_layoutHasChanged)
            {
                var dictUri = new Uri(_layoutsDict[_changedType], UriKind.Relative);
                var resourceDict = System.Windows.Application.LoadComponent(dictUri) as ResourceDictionary;
                System.Windows.Application.Current.Resources.MergedDictionaries.Clear();
                System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                SettingWrapper.Layout = _layoutsDict[_changedType];

                _layoutHasChanged = false;
            }
            
            SettingWrapper.InboxDir =inboxDir;
            SettingWrapper.PollingTime = Convert.ToDouble(ComboBoxPollingTime.SelectedValue.ToString());
            SettingWrapper.IsPolling = CheckboxPolling.IsChecked.GetValueOrDefault(false);
            SettingWrapper.ShowInputDialog = CheckboxShowImportDialog.IsChecked.GetValueOrDefault(false);

            var newSortOrder = SortHelper.ConvertToMetaAttribute(_sortOrderList);
            var newIsSorted = CheckBoxSorting.IsChecked.GetValueOrDefault(false);
            SortHelper.SetSorting(SettingWrapper.SortOrder, newSortOrder, SettingWrapper.IsSorting, newIsSorted);

            SettingWrapper.SortOrder = newSortOrder;
            SettingWrapper.IsSorting = newIsSorted;
            SettingWrapper.AllDirectories = _allDirs;
            SettingWrapper.ShouldRemoveOnImport = MoveOrCopy.IsChecked.GetValueOrDefault(false);

            SettingWrapper.Save();

            if (!_fileManager.DirectoryExists(SettingWrapper.MusicDir))
                _fileManager.CreateDirectory(SettingWrapper.MusicDir);
            
            Close();
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs args)
        {
            var dataList = new List<string> { "0.5", "1", "5", "10", "30", "60", "120", "240" };

            var box = sender as ComboBox;
            if (box != null) box.ItemsSource = dataList;
        }

        private void ResetDefault(object sender, RoutedEventArgs e)
        {
            _sortOrderList = SortHelper.GetDefaultStringValues();
            _comboBoxes.Clear();
            LoadComboBoxes();
            SortConfig.Children.Clear();
            LoadSortListBox();
        }

        private void Skins_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox = (sender as System.Windows.Controls.RadioButton);
            if (checkbox == null) return;
            
            if (checkbox.Equals(PinkCheck))
            {
                _changedType = LayoutType.Pink;
                _layoutHasChanged = true;
            }
            else if (checkbox.Equals(DarkCheck))
            {
                _changedType = LayoutType.Dark;
                _layoutHasChanged = true;
            }
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Apply_OnClick(object sender, RoutedEventArgs e)
        {
            var dictUri = new Uri(_layoutsDict[_changedType], UriKind.Relative);
            var resourceDict = System.Windows.Application.LoadComponent(dictUri) as ResourceDictionary;
            System.Windows.Application.Current.Resources.MergedDictionaries.Clear();
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            SettingWrapper.Layout =_layoutsDict[_changedType];

            SettingWrapper.Save();
        }


        private void CheckBoxInfoLoggingChanged(object sender, RoutedEventArgs e)
        {
            var cb = sender as System.Windows.Controls.CheckBox;
            if (cb != null && cb.IsChecked == true)
            {
                SettingWrapper.LogLevel = (int)Logger.LoggingLevel.Info;
            }
            else
            {
                SettingWrapper.LogLevel = (int)Logger.LoggingLevel.Error;
            }
        }

        private void OpenLogFolder(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", Logger.LogDir);
        }
    }
}
