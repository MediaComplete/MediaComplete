using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using MediaComplete.Lib.Metadata;
using MediaComplete.Lib.Sorting;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using Label = System.Windows.Controls.Label;
using Orientation = System.Windows.Controls.Orientation;

namespace MediaComplete
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings
    {
        private readonly List<ComboBox> _comboBoxes;
        private List<string> _sortOrderList;
        private readonly List<bool> _showComboBox; 

        /// <summary>
        /// Loads the list of bool values for whether to show
        /// the contents of the combobox
        /// </summary>
        private void LoadComboBoxes()
        {
            _showComboBox.Clear();
            _sortOrderList.ForEach(delegate
            {
                _showComboBox.Add(true);
            });
        }

        /// <summary>
        /// Loads the the sort order UI elements
        /// Populates possible comboboxes with correct values
        /// Setts up handlers for button clicks
        /// </summary>
        private void LoadSortListBox()
        {
            _comboBoxes.Clear();
            SortConfig.Children.Clear();

            //Will be used to keep track of which attribute to show
            var attributeIndex = -1;

            for (var i = 0; i < _showComboBox.Count; i++)
            {
                var showValue = _showComboBox[i];
                if (showValue)
                {
                    attributeIndex++;
                }
                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                var folder = new Polygon
                {
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(16 * (i + 1), 8, 0, 8)
                };
                folder.SetResourceReference(StyleProperty, "FolderIconStyle");
                var comboBox = new ComboBox
                {
                    Width = 100,
                    Height = 24,
                    //If showValue is true you get the attributes that include the one you are currently showing
                    //Else you get all the remaining attributes
                    ItemsSource = showValue ? SortHelper.GetAllValidAttributes(_sortOrderList, _sortOrderList[attributeIndex])
                                                : SortHelper.GetAllUnusedMetaAttributes(_sortOrderList),
                    //If you need to show a value grab the correct value from the list of attributes
                    //Else show an empty combobox
                    SelectedValue = showValue ? (object)_sortOrderList[attributeIndex] : -1,
                    Tag = i
                };

                var minus = new Button
                {
                    Width = 20,
                    Height = 20,
                    Tag = i
                };
                minus.SetResourceReference(StyleProperty, "Minus");
                var plus = new Button
                {
                    Width = 20,
                    Height = 20,
                    Tag = i
                };
                plus.SetResourceReference(StyleProperty, "Plus");

                minus.Click += MinusClicked;
                plus.Click += PlusClicked;
                comboBox.SelectionChanged += SelectChanged;


                _comboBoxes.Add(comboBox);
                stackPanel.Children.Add(folder);
                stackPanel.Children.Add(comboBox);
                stackPanel.Children.Add(minus);

                //If all of the MetaAttribute options are used don't add a plus
                if (Enum.GetNames(typeof(MetaAttribute)).Length != _showComboBox.Count)
                {
                    stackPanel.Children.Add(plus);
                }

                SortConfig.Children.Add(stackPanel);
            }
            if (_sortOrderList.Count != 0 || _comboBoxes.Count != 0) return;

            //The No Sort option to be displayed
            var stackPanelForNoSort = new StackPanel { Orientation = Orientation.Horizontal };
            var label = new Label { Content = "No Sort" };
            var plusForNoSort = new Button
            {
                Width = 20,
                Height = 20,
                Tag = -1
            };
            plusForNoSort.Click += PlusClicked;
            plusForNoSort.SetResourceReference(StyleProperty, "Plus");

            stackPanelForNoSort.Children.Add(label);
            stackPanelForNoSort.Children.Add(plusForNoSort);
            SortConfig.Children.Add(stackPanelForNoSort);
        }

        /// <summary>
        /// The event that happens when the plus button is clicked
        /// Reloads the UI with new combobox
        /// </summary>
        /// <param name="sender">The sender of the action(the folder selection button)</param>
        /// <param name="e">Type of event</param>
        private void PlusClicked(object sender, RoutedEventArgs e)
        {
            var plus = sender as Button;
            if (plus == null) return;
            var tag = (int)plus.Tag;

            //Inserts  a new combobox that will be empty
            _showComboBox.Insert(tag + 1, false);

            LoadSortListBox();
        }

        /// <summary>
        /// The event that happens when the minus button is clicked
        /// Reloads the UI after removing the correct combobox
        /// </summary>
        /// <param name="sender">The sender of the action(the folder selection button)</param>
        /// <param name="e">Type of event</param>
        private void MinusClicked(object sender, RoutedEventArgs e)
        {
            var minus = sender as Button;
            if (minus == null) return;
            var tag = (int)minus.Tag;

            //If the combobox has a value it will be removed for the sort order
            if (_showComboBox[tag])
            {
                _sortOrderList.Remove((string)_comboBoxes[tag].SelectedValue);
            }

            _showComboBox.RemoveAt(tag);
            LoadSortListBox();
        }

        /// <summary>
        /// The event that happens when a item is selected in a combobox
        /// Changes the values that are in each dropdown
        /// </summary>
        /// <param name="sender">The sender of the action(the folder selection button)</param>
        /// <param name="e">Type of event</param>
        private void SelectChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;

            var comboBox = sender as ComboBox;
            if (comboBox == null) return;

            var tag = (int)comboBox.Tag;
            
            //Changes the combobox so it will now be recognized as having a value
            _showComboBox[tag] = true;

            var addItem = (string)e.AddedItems[0];
            //Removes the past element from the sort order
            if (e.RemovedItems.Count > 0)
            {
                var removedItem = (string)e.RemovedItems[0];
                _sortOrderList.Remove(removedItem);
            }

            var insertInt = _sortOrderList.Count <= tag ? _sortOrderList.Count : tag;
            _sortOrderList.Insert(insertInt, addItem);

            //Repopulates the combobox options
            foreach (var box in _comboBoxes)
            {
                box.ItemsSource = box.SelectedValue == null ? SortHelper.GetAllUnusedMetaAttributes(_sortOrderList) : SortHelper.GetAllValidAttributes(_sortOrderList, (string)box.SelectedValue);
            }
        }
    }
}
