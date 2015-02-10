using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using MSOE.MediaComplete.Lib;
using MSOE.MediaComplete.Lib.Sorting;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using Label = System.Windows.Controls.Label;
using Orientation = System.Windows.Controls.Orientation;

namespace MSOE.MediaComplete
{
    /// <summary>

    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings
    {
        private readonly List<ComboBox> _comboBoxes;
        private List<string> _sortOrderList;
        private readonly List<bool> _showComboBox; 

        private readonly Dictionary<LayoutType, string> _layoutsDict = new Dictionary<LayoutType, string>
        {
            {LayoutType.Dark, "layout\\Dark.xaml"},
            {LayoutType.Pink, "layout\\Pink.xaml"}
        };
        private LayoutType _changedType;
        private bool _layoutHasChanged;
        public Settings()
        {

            InitializeComponent();
            TxtboxSelectedFolder.Text = SettingWrapper.GetHomeDir();
            LblInboxFolder.Content = SettingWrapper.GetInboxDir();
            ComboBoxPollingTime.SelectedValue = SettingWrapper.GetPollingTime().ToString(CultureInfo.InvariantCulture);
            CheckboxPolling.IsChecked = SettingWrapper.GetIsPolling();
            CheckboxShowImportDialog.IsChecked = SettingWrapper.GetShowInputDialog();
            CheckBoxSorting.IsChecked = SettingWrapper.GetIsSorting();
            PollingCheckBoxChanged(CheckboxPolling, null);
            if (SettingWrapper.GetLayout().Equals(_layoutsDict[LayoutType.Pink]))
            {
                PinkCheck.IsChecked = true;
            }
            else if (SettingWrapper.GetLayout().Equals(_layoutsDict[LayoutType.Dark]))
            {
                DarkCheck.IsChecked = true;
            }
            _comboBoxes = new List<ComboBox>();
            _sortOrderList= SortHelper.ConvertToString(SettingWrapper.GetSortOrder());
            _showComboBox = new List<bool>();
            LoadDictionary();
            LoadSortListBox();
        }

        private void LoadDictionary()
        {
            _showComboBox.Clear();
            for (var i = 0; i < _sortOrderList.Count; i++)
            {
                _showComboBox.Add(true);
            }
        }

        private void LoadSortListBox()
        {
            var withValue = -1;

            for (var i = 0; i < _showComboBox.Count; i++)
            {
                var showValue = _showComboBox[i];
                if (showValue)
                {
                    withValue++;
                }
                var stackPanel = new StackPanel {Orientation = Orientation.Horizontal};
                var folder = new Image
                {
                    Source = (ImageSource)Resources["Settings-Folder-Icon"],
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(16 * (i + 1), 8, 0, 8)
                };
                var comboBox = new ComboBox
                {
                    Width = 100,
                    Height = 24,
                    ItemsSource = showValue ? SortHelper.GetAllValidAttributes(_sortOrderList, _sortOrderList[withValue]) 
                                                : SortHelper.GetAllUnusedMetaAttributes(_sortOrderList),
                    SelectedValue = showValue ? (object) _sortOrderList[withValue] : -1,
                    Tag = i
                };
                var minus = new Button
                {
                    Content = new Image
                    {
                        Source = (ImageSource) Resources["Settings-MinusConfig-Icon"]
                    },
                    Tag = i,
                    Margin = new Thickness(0, 0, 2, 0)
                };
                minus.SetResourceReference(StyleProperty, "Plus/Minus"); 
                var plus = new Button
                {
                    Content = new Image
                    {
                        Source = (ImageSource)Resources["Settings-AddConfig-Icon"]
                    },
                    Tag = i,
                };
                plus.SetResourceReference(StyleProperty, "Plus/Minus"); 

                minus.Click += MinusClicked;
                plus.Click += PlusClicked;
                comboBox.SelectionChanged += SelectChanged;

                _comboBoxes.Add(comboBox);
                stackPanel.Children.Add(folder);
                stackPanel.Children.Add(comboBox);

                stackPanel.Children.Add(minus);
                stackPanel.Children.Add(plus);
                SortConfig.Children.Add(stackPanel);
            }
            if (_sortOrderList.Count == 0 && _comboBoxes.Count == 0)
            {
                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                var label = new Label { Content = "No Sort" };
                var plus = new Button
                {
                    Content = new Image
                    {
                        Source = (ImageSource)Resources["Settings-AddConfig-Icon"]
                    },
                    Tag = -1,
                };
                plus.Click += PlusClicked;
                plus.SetResourceReference(StyleProperty, "Plus/Minus"); 

                stackPanel.Children.Add(label);
                stackPanel.Children.Add(plus);
                SortConfig.Children.Add(stackPanel);

            }

        }

        private void PlusClicked(object sender, RoutedEventArgs e)
        {
            var plus = sender as Button;
            if (plus == null) return;
            var tag = (int)plus.Tag;

            _showComboBox.Insert(tag + 1 , false);

            _comboBoxes.Clear();

            SortConfig.Children.Clear();
            LoadSortListBox();

            _comboBoxes[tag + 1].ItemsSource = SortHelper.GetAllUnusedMetaAttributes(_sortOrderList);
            _comboBoxes[tag + 1].SelectedValue = -1;
        }

        private void MinusClicked(object sender, RoutedEventArgs e)
        {
            var minus = sender as Button;
            if (minus == null) return;
            var tag = (int)minus.Tag;

            if (_showComboBox[tag])
            {
                _sortOrderList.Remove((string)_comboBoxes[tag].SelectedValue);
            }
            _showComboBox.RemoveAt(tag);
            _comboBoxes.Clear();

            SortConfig.Children.Clear();
            LoadSortListBox();
        }

        private void SelectChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;

            var comboBox = sender as ComboBox;
            if (comboBox == null) return;
            var tag = (int)comboBox.Tag;
            _showComboBox[tag] = true;

            var addItem = (string)e.AddedItems[0];
            if (e.RemovedItems.Count > 0)
            {
                var removedItem = (string)e.RemovedItems[0];
                _sortOrderList.Remove(removedItem);
            }
            var insertInt = _sortOrderList.Count <= tag ? _sortOrderList.Count : tag;
            _sortOrderList.Insert(insertInt , addItem);

            foreach (var box in _comboBoxes)
            {
                box.ItemsSource = box.SelectedValue == null ? SortHelper.GetAllUnusedMetaAttributes(_sortOrderList) : SortHelper.GetAllValidAttributes(_sortOrderList, (string)box.SelectedValue);
            }
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
                    LblInboxFolder.Content = folderBrowserDialog1.SelectedPath;
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

                LblInboxFolder.IsEnabled = true;
                ComboBoxPollingTime.IsEnabled = true;
                BtnInboxFolder.IsEnabled = true;
                LblPollTime.IsEnabled = true;
                LblMin.IsEnabled = true;
                LblSelectInboxLocation.IsEnabled = true;
            }
            else
            {
                CheckboxShowImportDialog.IsEnabled = false;
                LblInboxFolder.IsEnabled = false;
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
            if (!homeDir.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.CurrentCulture)))
            {
                homeDir += Path.DirectorySeparatorChar;
            }
            var inboxDir = (string) LblInboxFolder.Content;
            if (!inboxDir.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.CurrentCulture)))
            {
                inboxDir += Path.DirectorySeparatorChar;
            }

            if (_layoutHasChanged)
            {
                var dictUri = new Uri(_layoutsDict[_changedType], UriKind.Relative);
                var resourceDict = System.Windows.Application.LoadComponent(dictUri) as ResourceDictionary;
                System.Windows.Application.Current.Resources.MergedDictionaries.Clear();
                System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                SettingWrapper.SetLayout(_layoutsDict[_changedType]);

                _layoutHasChanged = false;
            }SettingWrapper.SetHomeDir(homeDir);

            SettingWrapper.SetInboxDir(inboxDir);
            SettingWrapper.SetPollingTime(ComboBoxPollingTime.SelectedValue);
            SettingWrapper.SetIsPolling(CheckboxPolling.IsChecked.GetValueOrDefault(false));
            SettingWrapper.SetShowInputDialog(CheckboxShowImportDialog.IsChecked.GetValueOrDefault(false));
            SettingWrapper.SetIsSorting(CheckBoxSorting.IsChecked.GetValueOrDefault(false));

            SettingWrapper.SetSortOrder(SortHelper.ConvertToMetaAttribute(_sortOrderList));
            SettingWrapper.Save();

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
            LoadDictionary();
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
            SettingWrapper.SetLayout(_layoutsDict[_changedType]);

            SettingWrapper.Save();
        }
    }
}
