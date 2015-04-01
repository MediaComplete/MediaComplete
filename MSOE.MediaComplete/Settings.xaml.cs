using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using MSOE.MediaComplete.Lib;
using MSOE.MediaComplete.Lib.Sorting;
using Button = System.Windows.Controls.Button;
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
        public Settings()
        {

            InitializeComponent();
            TxtboxSelectedFolder.Text = SettingWrapper.HomeDir;
            TxtInboxFolder.Text = SettingWrapper.InboxDir;
            ComboBoxPollingTime.SelectedValue = SettingWrapper.PollingTime.ToString(CultureInfo.InvariantCulture);
            CheckboxPolling.IsChecked = SettingWrapper.IsPolling;
            CheckboxShowImportDialog.IsChecked = SettingWrapper.ShowInputDialog;
            CheckBoxSorting.IsChecked = SettingWrapper.IsSorting;
            MoveOrCopy.IsChecked = SettingWrapper.ShouldRemoveOnImport;
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
            var folderBrowserDialog1 = new FolderBrowserDialog();
            var button = sender as Button;
            if (folderBrowserDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            if (button == null) return;
            switch (button.Name)
            {
                case "BtnSelectFolder":
                    TxtboxSelectedFolder.Text = folderBrowserDialog1.SelectedPath;
                    break;
                case "BtnInboxFolder":
                    TxtInboxFolder.Text = folderBrowserDialog1.SelectedPath;
                    break;
            }
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
            // add settings here as they are added to the UI
            var homeDir = TxtboxSelectedFolder.Text;
            if (homeDir != null && !homeDir.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.CurrentCulture), StringComparison.Ordinal))
            {
                homeDir += Path.DirectorySeparatorChar;
            }
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
            
            SettingWrapper.HomeDir =homeDir;
            SettingWrapper.InboxDir =inboxDir;
            SettingWrapper.PollingTime = Convert.ToDouble(ComboBoxPollingTime.SelectedValue.ToString());
            SettingWrapper.IsPolling = CheckboxPolling.IsChecked.GetValueOrDefault(false);
            SettingWrapper.ShowInputDialog = CheckboxShowImportDialog.IsChecked.GetValueOrDefault(false);

            var newSortOrder = SortHelper.ConvertToMetaAttribute(_sortOrderList);
            var newIsSorted = CheckBoxSorting.IsChecked.GetValueOrDefault(false);
            SortHelper.SetSorting(SettingWrapper.SortOrder, newSortOrder, SettingWrapper.IsSorting, newIsSorted);

            SettingWrapper.SortOrder = newSortOrder;
            SettingWrapper.IsSorting = newIsSorted;

            SettingWrapper.ShouldRemoveOnImport = MoveOrCopy.IsChecked.GetValueOrDefault(false);

            SettingWrapper.Save();

            if (!Directory.Exists(SettingWrapper.MusicDir))
                Directory.CreateDirectory(SettingWrapper.MusicDir);
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
    }
}
