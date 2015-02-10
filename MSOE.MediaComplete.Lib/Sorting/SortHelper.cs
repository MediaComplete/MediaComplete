using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MSOE.MediaComplete.Lib.Metadata;

namespace MSOE.MediaComplete.Lib.Sorting
{
    public class SortHelper
    {
        private static bool _shouldSort;

        public static void SetSorting(List<MetaAttribute> oldSort, List<MetaAttribute> newSort)
        {
            _shouldSort = !oldSort.SequenceEqual(newSort) && SettingWrapper.IsSorting;
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
        public static List<string> GetDefaultStringValues()
        {
            var listOfMetaAttributes = new List<MetaAttribute>
            {
                MetaAttribute.Artist, 
                MetaAttribute.Album
            };
            return listOfMetaAttributes.Select(GetDescription).ToList();
        } 
        public static List<string> GetAllUnusedMetaAttributes(List<string> valueList)
        {
            
            var metaList = Enum.GetValues(typeof(MetaAttribute)).Cast<MetaAttribute>().ToList();
            var stringList = metaList.Select(GetDescription).Except(valueList).OrderBy(x => x).ToList();

            return stringList;
        }

        public static List<string> GetAllValidAttributes(List<string> valueList, string metaAttribute)
        {
            var stringList = GetAllUnusedMetaAttributes(valueList);
            stringList.Add(metaAttribute);
            return stringList.OrderBy(x => x).ToList();
        }

        public static List<string> ConvertToString(List<MetaAttribute> valueList)
        {
            var stringList = valueList.Select(GetDescription).ToList();
            return stringList;
        }
        public static List<MetaAttribute> ConvertToMetaAttribute(List<string> valueList)
        {
            var metaAttributeList = valueList.Select(GetValue).ToList();
            return metaAttributeList;
        }

        private static string GetDescription(MetaAttribute metaAttribute)
        {
            var type = typeof(MetaAttribute);
            var memInfo = type.GetMember(metaAttribute.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute),
                false);
            return ((DescriptionAttribute)attributes[0]).Description;
        }
        private static MetaAttribute GetValue(string value)
        {
            var type = typeof(MetaAttribute);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == value)
                        return (MetaAttribute)field.GetValue(null);
                }
                else
                {
                    if (field.Name == value)
                        return (MetaAttribute)field.GetValue(null);
                }
            }
            return default(MetaAttribute);
        }
    }
}
