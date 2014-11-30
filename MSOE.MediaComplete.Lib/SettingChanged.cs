using System;

namespace MSOE.MediaComplete.Lib
{
    public class SettingChanged : EventArgs
    {
        public string HomeDir { get; set; }

        public SettingChanged(string dir)
        {
            HomeDir = dir;
        }
    }
}
