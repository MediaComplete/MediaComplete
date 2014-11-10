using System;

namespace MSOE.MediaComplete.Lib
{
    public class SettingPublisher
    {
        public event EventHandler<SettingChanged> RaiseSettingEvent;

        public void ChangeSetting(string homeDir)
        {
            OnRaiseEvent(new SettingChanged(homeDir));
        }

        protected virtual void OnRaiseEvent(SettingChanged settingChanged)
        {
            var handler = RaiseSettingEvent;

            if (handler != null)
            {
                handler(this, settingChanged);
            }
        }
    }
}
