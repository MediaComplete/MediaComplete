using System;
using System.Collections.Generic;
using System.Linq;

namespace MSOE.MediaComplete.Lib.Sorting
{
    public class SortHelper
    {
        private static bool _shouldSort;

        public static void SetSorting(List<MetaAttribute> oldSort, List<MetaAttribute> newSort)
        {
            _shouldSort = !oldSort.SequenceEqual(newSort) && SettingWrapper.GetIsSorting();
        }

        public static bool GetSorting()
        {
            return _shouldSort;
        }
        public static List<MetaAttribute> GetDefault()
        {
            return new List<MetaAttribute>
            {
                MetaAttribute.Artist, 
                MetaAttribute.Album
            };
        } 
        public static List<MetaAttribute> GetAllUnusedMetaAttributes(List<MetaAttribute> valueList)
        {
            
            var metaList = Enum.GetValues(typeof(MetaAttribute)).Cast<MetaAttribute>().ToList().Except(valueList).ToList();
            return metaList;
        }

        public static List<MetaAttribute> GetAllValidAttributes(List<MetaAttribute> valueList, MetaAttribute metaAttribute)
        {

            var metaList = Enum.GetValues(typeof(MetaAttribute)).Cast<MetaAttribute>().ToList().Except(valueList).ToList();
            metaList.Add(metaAttribute);
            return metaList;
        }
    }
}
