using TagLib;

namespace MSOE.MediaComplete.Lib
{
    public static class MetaDataGetter
    {
        public static string GetSongTitle(this File file)
        {
            return file.StringForMetaAttribute(MetaAttribute.SongTitle);
        }
        public static string GetAlbum(this File file)
        {
            return file.StringForMetaAttribute(MetaAttribute.Album);
        }
        public static string GetArtist(this File file)
        {
            return file.StringForMetaAttribute(MetaAttribute.Artist);
        }
        public static string GetGenre(this File file)
        {
            return file.StringForMetaAttribute(MetaAttribute.Genre);
        }
        public static string GetSupportingArtist(this File file)
        {
            return file.StringForMetaAttribute(MetaAttribute.SupportingArtist);
        }
        public static string GetTrack(this File file)
        {
            return file.StringForMetaAttribute(MetaAttribute.TrackNumber);
        }
        public static string GetYear(this File file)
        {
            return file.StringForMetaAttribute(MetaAttribute.Year);
        }
        public static string GetRating(this File file)
        {
            return file.StringForMetaAttribute(MetaAttribute.Rating);
        }
    }
}
