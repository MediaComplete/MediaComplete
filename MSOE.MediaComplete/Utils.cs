using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MSOE.MediaComplete
{
    static class Utils
    {
        public static string FilePath(this TreeViewItem leaf)
        {
            string parentPath = "";
            if (leaf.Parent is TreeViewItem)
            {
                parentPath = (leaf.Parent as TreeViewItem).FilePath();
            }
            return parentPath + leaf.Header.ToString();
        }
    }
}
