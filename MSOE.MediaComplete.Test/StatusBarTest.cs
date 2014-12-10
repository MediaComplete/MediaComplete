using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib;

namespace MSOE.MediaComplete.Test
{
    [TestClass]
    public class StatusBarTest
    {
        private string _message = "";
        private StatusBarHandler.StatusIcon _icon = StatusBarHandler.StatusIcon.None;
        private int _count;

        [TestMethod]
        public void RaiseStatusBarEvent_StatusMessage_ValidStatusMessage()
        {
            const string testMessage = "Test is successful";
            const StatusBarHandler.StatusIcon testIcon = StatusBarHandler.StatusIcon.Success;
            const string clearMessage = "";
            const StatusBarHandler.StatusIcon clearIcon = StatusBarHandler.StatusIcon.None;
            StatusBarHandler.Instance.Interval = .1;
            
            StatusBarHandler.Instance.RaiseStatusBarEvent += HandleStatusBarChangeEvent;
            StatusBarHandler.Instance.ChangeStatusBarMessage(testMessage, testIcon);

            Assert.AreEqual(testMessage, _message);
            Assert.AreEqual(testIcon, _icon);

            while (_count < 2)
            {
            }

            Assert.AreEqual(clearMessage, _message);
            Assert.AreEqual(clearIcon, _icon);
        }

        private void HandleStatusBarChangeEvent(string status, StatusBarHandler.StatusIcon statusIcon)
        {
            _message = status;
            _icon = statusIcon;
            _count++;
        }
    }
}
