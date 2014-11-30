using TagLib;

namespace MSOE.MediaComplete.Lib
{
    public class Mp3MetadataEditor
    {
        private readonly File _mp3File;

        /// <exception cref="CorruptFileException">If the file is not a valid MP3 file</exception>
        /// <param name="filename"></param>
        public Mp3MetadataEditor(string filename)
        {
            _mp3File = File.Create(filename);
        }

        //TODO this is just an example getter/setter - add in more as needed
        public uint Year
        {
            get
            {
                return _mp3File.Tag.Year;
            }
            set
            {
                _mp3File.Tag.Year = value;
                _mp3File.Save();
            }
        }

    }
}
