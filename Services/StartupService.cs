using System.Windows.Forms;
using Microsoft.Win32;

namespace WindowsMicToggle.Services
{
    public class StartupService
    {
        private const string AppName = "MicrophoneToggle";
        private readonly string _executablePath = Application.ExecutablePath;

        public void EnableStartup()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(
                       "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue(AppName, _executablePath);
            }
        }

        public void DisableStartup()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(
                       "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue(AppName, false);
            }
        }

        public bool IsStartupEnabled()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(
                       "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false))
            {
                return key?.GetValue(AppName) != null;
            }
        }
    }
}
