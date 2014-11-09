using TagLib;

namespace MSOE.MediaComplete.Lib
{
    public class Mp3MetadataEditor
    {
        private File mp3File;

        /// <exception cref="CorruptFileException">If the file is not a valid MP3 file</exception>
        /// <param name="filename"></param>
        public Mp3MetadataEditor(string filename)
        {
            mp3File = File.Create(filename);
        }

        //TODO this is just an example getter/setter - add in more as needed
        public uint GetYear()
        {
            return mp3File.Tag.Year;
        }
        public void SetYear(uint year)
        {
            mp3File.Tag.Year = year;
            mp3File.Save();
        }

    }
}
