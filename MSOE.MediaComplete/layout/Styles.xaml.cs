using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace MSOE.MediaComplete.layout
{
    /// <summary>
    /// Contains high-level event handlers
    /// </summary>
    public partial class Styles
    {
        /// <summary>
        /// When clicking on a listview, and not on a list item, the selection should clear.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var listView = (ListView) sender;

            var isEmptySpace = listView.Items.Cast<object>().All(item => !((ListViewItem) item).IsMouseOver);

            if (isEmptySpace)
            {
                listView.UnselectAll();
            }
        }
    }
}
