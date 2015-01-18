using System;
using System.Collections.Generic;
using System.Linq;

namespace MSOE.MediaComplete.Lib.Sorting
{
    public class SortHelper
    {
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
            return Enum.GetValues(typeof(MetaAttribute)).Cast<MetaAttribute>().ToList().Except(valueList).ToList();
        }

        public static List<MetaAttribute> MetaAttributesFromString(List<String> stringList)
        {
            return stringList
                .Select(a => (MetaAttribute)Enum.Parse(typeof(MetaAttribute), a))
                .ToList();
        } 
    }
}
