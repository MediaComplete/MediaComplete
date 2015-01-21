using System;
using System.Collections.Generic;
using System.Linq;

namespace MSOE.MediaComplete.Lib.Sorting
{
    public class SortHelper
    {
        private static bool _shouldSort;

        private static readonly List<MetaAttribute> AlbumRule = new List<MetaAttribute>
        {
            MetaAttribute.Year,
            MetaAttribute.AlbumArt
        };
        private static readonly List<MetaAttribute> AlbumArtRule = new List<MetaAttribute>
        {
            MetaAttribute.Album,
            MetaAttribute.Year
        };

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
            if (valueList.Contains(MetaAttribute.Album))
            {
                metaList = metaList.Except(AlbumRule).ToList();
            }
            if (valueList.Contains(MetaAttribute.AlbumArt))
            {
                metaList = metaList.Except(AlbumArtRule).ToList();
            }

            return metaList;
        }
    }
}
