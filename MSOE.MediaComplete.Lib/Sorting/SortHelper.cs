using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib.Sorting
{
    public class SortHelper
    {
        public static List<string> GetDefault()
        {
            return new List<string>
            {
                MetaAttribute.Artist.ToString(), 
                MetaAttribute.Album.ToString()
            };
        } 
        public static List<String> GetAllMetaAttributes(List<String> valueList)
        {
            return Enum.GetNames(typeof (MetaAttribute)).ToList().Except(valueList).ToList();
        }

        public static List<MetaAttribute> MetaAttributesFromString(List<String> stringList)
        {
            return stringList
                .Select(a => (MetaAttribute)Enum.Parse(typeof(MetaAttribute), a))
                .ToList();
        } 
    }
}
