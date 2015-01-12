using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MSOE.MediaComplete.Lib;
using MSOE.MediaComplete.Lib.Sorting;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using Label = System.Windows.Controls.Label;

namespace MSOE.MediaComplete
{
    /// <summary>

    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings
    {
        private SortSettings _sortSettings;
        private bool _hasBeenSelected;
        private Button _plusButton;
        private Button _minusButton;
        private List<Label> _lables;
        private ComboBox _comboBox;
        private List<String> _sortOrderList; 

        public Settings()
        {
            InitializeComponent();
            TxtboxSelectedFolder.Text = SettingWrapper.GetHomeDir();
            TxtboxInboxFolder.Text = SettingWrapper.GetInboxDir();
            ComboBoxPollingTime.SelectedValue = SettingWrapper.GetPollingTime().ToString(CultureInfo.InvariantCulture);
            CheckboxPolling.IsChecked = SettingWrapper.GetIsPolling();
            CheckboxShowImportDialog.IsChecked = SettingWrapper.GetShowInputDialog();
            CheckBoxSorting.IsChecked = SettingWrapper.GetIsSorting();
            PollingCheckBoxChanged(CheckboxPolling, null);

            _hasBeenSelected = false;
            _lables = new List<Label>();
            _sortOrderList = SettingWrapper.GetSortOrder();
            _sortSettings = new SortSettings();
            LoadSortListBox();
        }

        private void LoadSortListBox()
        {
            _comboBox = new ComboBox();
            var grid = new Grid();

            var columnDefinition1 = new ColumnDefinition();
            var columnDefinition2 = new ColumnDefinition();
            var columnDefinition3 = new ColumnDefinition();
            var columnDefinition4 = new ColumnDefinition();

            columnDefinition1.Width = new GridLength((_sortOrderList.Count + 1) * 8);
            columnDefinition2.Width = new GridLength(100);
            columnDefinition3.Width = new GridLength(25);
            columnDefinition4.Width = new GridLength(25);

            grid.ColumnDefinitions.Add(columnDefinition1);
            grid.ColumnDefinitions.Add(columnDefinition2);
            grid.ColumnDefinitions.Add(columnDefinition3);
            grid.ColumnDefinitions.Add(columnDefinition4);

            if (_sortOrderList.Count != 0)
            {
                for (var i = 0; i < _sortOrderList.Count; i++)
                {
                    var label = new Label
                    {
                        Content = _sortOrderList[i],
                        Padding = new Thickness(8 * (i + 1), 8, 8, 8),
                        Name = _sortOrderList[i]
                        
                    };
                    _lables.Add(label);
                    SortConfig.Children.Add(label);
                }
                _comboBox.ItemsSource = SortHelper.GetAllMetaAttributes(_sortOrderList);
                SettingWrapper.GetSortOrder();
                _comboBox.SelectionChanged += SelectChanged;


                _plusButton = new Button
                {
                    Content = "Add",
                    Visibility = Visibility.Hidden
                };
                _plusButton.Click += PlusClicked;

                _minusButton = new Button
                {
                    Content = "Minus",
                    Visibility = Visibility.Hidden
                };
                _minusButton.Click += MinusClicked;

                Grid.SetColumn(_comboBox, 1);
                Grid.SetColumn(_plusButton, 2);
                Grid.SetColumn(_minusButton, 3);

                grid.Children.Add(_comboBox);
                grid.Children.Add(_plusButton);
                grid.Children.Add(_minusButton);


                SortConfig.Children.Add(grid);
            }
            else
            {
                
            }
        }

        private void MinusClicked(object sender, RoutedEventArgs e)
        {
            SortConfig.Children.Remove(_lables[_lables.Count - 1]);
            _sortOrderList.RemoveAt(_sortOrderList.Count - 1);
            _comboBox.ItemsSource = SortHelper.GetAllMetaAttributes(_sortOrderList);
            _lables.RemoveAt(_lables.Count - 1);
            _comboBox.SelectedIndex = -1;

        }

        private void PlusClicked(object sender, RoutedEventArgs e)
        {
            SortConfig.Children.Clear();
            _lables.Clear();
            _sortOrderList.Add(_comboBox.SelectedValue.ToString());
            LoadSortListBox();
        }

        private void SelectChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_hasBeenSelected)
            {
                _plusButton.Visibility = Visibility.Visible;
                _minusButton.Visibility = Visibility.Visible;
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
            var button = sender as System.Windows.Controls.Button;
            if (folderBrowserDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            if (button == null) return;
            switch (button.Name)
            {
                case "BtnSelectFolder":
                    TxtboxSelectedFolder.Text = folderBrowserDialog1.SelectedPath;
                    break;
                case "BtnInboxFolder":
                    TxtboxInboxFolder.Text = folderBrowserDialog1.SelectedPath;
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

                TxtboxInboxFolder.IsEnabled = true;
                ComboBoxPollingTime.IsEnabled = true;
                BtnInboxFolder.IsEnabled = true;
                LblPollTime.IsEnabled = true;
                LblMin.IsEnabled = true;
                LblSelectInboxLocation.IsEnabled = true;
            }
            else
            {
                CheckboxShowImportDialog.IsEnabled = false;
                TxtboxInboxFolder.IsEnabled = false;
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
            var inboxDir = TxtboxInboxFolder.Text;
            if (!inboxDir.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.CurrentCulture)))
            {
                inboxDir += Path.DirectorySeparatorChar;
            }

            SettingWrapper.SetHomeDir(homeDir);

            SettingWrapper.SetInboxDir(inboxDir);
            SettingWrapper.SetPollingTime(ComboBoxPollingTime.SelectedValue);
            SettingWrapper.SetIsPolling(CheckboxPolling.IsChecked.GetValueOrDefault(false));
            SettingWrapper.SetShowInputDialog(CheckboxShowImportDialog.IsChecked.GetValueOrDefault(false));
            SettingWrapper.SetIsSorting(CheckBoxSorting.IsChecked.GetValueOrDefault(false));
            SettingWrapper.Save();

            Close();
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs args)
        {
            var dataList = new List<string> { "0.5", "1", "5", "10", "30", "60", "120", "240" };

            var box = sender as ComboBox;
            if (box != null) box.ItemsSource = dataList;
        }
    }
}
