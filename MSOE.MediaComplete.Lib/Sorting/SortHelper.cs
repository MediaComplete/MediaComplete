using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MSOE.MediaComplete.Lib.Metadata;

namespace MSOE.MediaComplete.Lib.Sorting
{
    /// <summary>
    /// Helper class to assist with control of the Sort Order option and whether to Sort
    /// </summary>
    public static class SortHelper
    {
        private static bool _shouldSort;
        /// <summary>
        /// Checks whether to sort based on past and new configurations
        /// </summary>
        /// <param name="pastSort"> The past sort order</param>
        /// <param name="newSort"> The new sort order</param>
        /// <param name="pastIsSorted"> The past automatic sort bool</param>
        /// <param name="newIsSorted"> The new automatic sort bool</param>
        public static void SetSorting(IEnumerable<MetaAttribute> pastSort, IEnumerable<MetaAttribute> newSort, bool pastIsSorted, bool newIsSorted)
        {
            _shouldSort = (!pastSort.SequenceEqual(newSort) || !pastIsSorted) && newIsSorted;
        }

        /// <summary>
        /// Gets whether a sort should occur
        /// </summary>
        /// <returns>A bool for whether to sort</returns>
        public static bool GetSorting()
        {
            return _shouldSort;
        }

        /// <summary>
        /// Gets the Default Sort order
        /// </summary>
        /// <returns> Returns list of default MetaAttributes</returns>
        public static List<MetaAttribute> GetDefault()
        {
            return new List<MetaAttribute>
            {
                MetaAttribute.Artist, 
                MetaAttribute.Album
            };
        }
        /// <summary>
        /// Gets the Default Sort Order as String values
        /// </summary>
        /// <returns>The default list of strings</returns>
        public static List<string> GetDefaultStringValues()
        {
            var listOfMetaAttributes = new List<MetaAttribute>
            {
                MetaAttribute.Artist, 
                MetaAttribute.Album
            };
            return listOfMetaAttributes.Select(GetDescription).ToList();
        } 

        /// <summary>
        /// All Unused MetaAttributes based on the valueList
        /// </summary>
        /// <param name="valueList"> The current sort order</param>
        /// <returns> The unused values not in the sort order</returns>
        public static List<string> GetAllUnusedMetaAttributes(List<string> valueList)
        {
            
            var metaList = Enum.GetValues(typeof(MetaAttribute)).Cast<MetaAttribute>().ToList();
            var stringList = metaList.Select(GetDescription).Except(valueList).OrderBy(x => x).ToList();

            return stringList;
        }

        /// <summary>
        /// Gets all the valid Attributes to display includes one that make be selected currently
        /// </summary>
        /// <param name="valueList"> The current sort order</param>
        /// <param name="metaAttribute"> The selected value that should also be included</param>
        /// <returns> The unused values not in the sort order plus the aditional value</returns>
        public static List<string> GetAllValidAttributes(List<string> valueList, string metaAttribute)
        {
            var stringList = GetAllUnusedMetaAttributes(valueList);
            stringList.Add(metaAttribute);
            return stringList.OrderBy(x => x).ToList();
        }

        /// <summary>
        /// Converts the list of MetaAttributes to a list of String
        /// </summary>
        /// <param name="valueList">The list of Metattributes to convert</param>
        /// <returns> The converted list</returns>
        public static List<string> ConvertToString(List<MetaAttribute> valueList)
        {
            var stringList = valueList.Select(GetDescription).ToList();
            return stringList;
        }

        /// <summary>
        /// Converts a list of Strings to a list of MetaAttributes
        /// </summary>
        /// <param name="valueList"> The list of strings to converts</param>
        /// <returns> The converted list</returns>
        public static List<MetaAttribute> ConvertToMetaAttribute(List<string> valueList)
        {
            var metaAttributeList = valueList.Select(GetValue).ToList();
            return metaAttributeList;
        }

        /// <summary>
        /// Method to get the string decription of a MetaAttribute
        /// </summary>
        /// <param name="metaAttribute"> The attribute you want a description for</param>
        /// <returns> The Description</returns>
        private static string GetDescription(MetaAttribute metaAttribute)
        {
            var type = typeof(MetaAttribute);
            var memInfo = type.GetMember(metaAttribute.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute),
                false);
            return ((DescriptionAttribute)attributes[0]).Description;
        }

        /// <summary>
        /// Method to get the MetaAttribute from a description
        /// </summary>
        /// <param name="value"> The string value you want an MetaAttribute for</param>
        /// <returns>The MetaAttribute </returns>
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
