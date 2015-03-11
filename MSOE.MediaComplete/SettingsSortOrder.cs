using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MSOE.MediaComplete.Lib.Metadata;
using MSOE.MediaComplete.Lib.Sorting;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using Label = System.Windows.Controls.Label;
using Orientation = System.Windows.Controls.Orientation;

namespace MSOE.MediaComplete
{
    public partial class Settings
    {
        private void LoadComboBoxes()
        {
            _showComboBox.Clear();
            _sortOrderList.ForEach(delegate
            {
                _showComboBox.Add(true);
            });
        }

        private void LoadSortListBox()
        {
            _comboBoxes.Clear();
            SortConfig.Children.Clear();

            var comboboxIndex = -1;

            for (var i = 0; i < _showComboBox.Count; i++)
            {
                var showValue = _showComboBox[i];
                if (showValue)
                {
                    comboboxIndex++;
                }
                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
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
                    ItemsSource = showValue ? SortHelper.GetAllValidAttributes(_sortOrderList, _sortOrderList[comboboxIndex])
                                                : SortHelper.GetAllUnusedMetaAttributes(_sortOrderList),
                    SelectedValue = showValue ? (object)_sortOrderList[comboboxIndex] : -1,
                    Tag = i
                };
                var minus = new Button
                {
                    Content = new Image
                    {
                        Source = (ImageSource)Resources["Settings-MinusConfig-Icon"]
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
                    Tag = i
                };
                plus.SetResourceReference(StyleProperty, "Plus/Minus");

                minus.Click += MinusClicked;
                plus.Click += PlusClicked;
                comboBox.SelectionChanged += SelectChanged;


                _comboBoxes.Add(comboBox);
                stackPanel.Children.Add(folder);
                stackPanel.Children.Add(comboBox);
                stackPanel.Children.Add(minus);

                if (Enum.GetNames(typeof(MetaAttribute)).Length != _showComboBox.Count)
                {
                    stackPanel.Children.Add(plus);
                }

                SortConfig.Children.Add(stackPanel);
            }
            if (_sortOrderList.Count != 0 || _comboBoxes.Count != 0) return;
            var stackPanelForNoSort = new StackPanel { Orientation = Orientation.Horizontal };
            var label = new Label { Content = "No Sort" };
            var plusForNoSort = new Button
            {
                Content = new Image
                {
                    Source = (ImageSource)Resources["Settings-AddConfig-Icon"]
                },
                Tag = -1
            };
            plusForNoSort.Click += PlusClicked;
            plusForNoSort.SetResourceReference(StyleProperty, "Plus/Minus");

            stackPanelForNoSort.Children.Add(label);
            stackPanelForNoSort.Children.Add(plusForNoSort);
            SortConfig.Children.Add(stackPanelForNoSort);
        }

        private void PlusClicked(object sender, RoutedEventArgs e)
        {
            var plus = sender as Button;
            if (plus == null) return;
            var tag = (int)plus.Tag;

            _showComboBox.Insert(tag + 1, false);

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
            _sortOrderList.Insert(insertInt, addItem);

            foreach (var box in _comboBoxes)
            {
                box.ItemsSource = box.SelectedValue == null ? SortHelper.GetAllUnusedMetaAttributes(_sortOrderList) : SortHelper.GetAllValidAttributes(_sortOrderList, (string)box.SelectedValue);
            }
        }
    }
}
