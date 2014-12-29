using TagLib;

namespace MSOE.MediaComplete.Lib
{
    public static class MetaDataFrontend
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

        public static void SetSongTitle(this File file, string value)
        {
            file.SetMetaAttribute(MetaAttribute.SongTitle, value);
        }
        public static void SetAlbum(this File file, string value)
        {
            file.SetMetaAttribute(MetaAttribute.Album, value);
        }
        public static void SetArtist(this File file, string value)
        {
            file.SetMetaAttribute(MetaAttribute.Artist, value);
        }
        public static void SetGenre(this File file, string value)
        {
            file.SetMetaAttribute(MetaAttribute.Genre, value);
        }
        public static void SetYear(this File file, string value)
        {
            file.SetMetaAttribute(MetaAttribute.Year, value);
        }
        public static void SetSupportingArtists(this File file, string value)
        {
            file.SetMetaAttribute(MetaAttribute.SupportingArtist, value);
        }
        public static void SetRating(this File file, string value)
        {
            file.SetMetaAttribute(MetaAttribute.Rating, value);
        }
        public static void SetTrack(this File file, string value)
        {
            file.SetMetaAttribute(MetaAttribute.TrackNumber, value);
        }
    }

}


