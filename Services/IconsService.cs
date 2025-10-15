using System;
using System.Drawing;

namespace WindowsMicToggle.Services
{
    public class IconsService
    {
        public Icon MicIcon { get; private set; }
        public Icon MicMutedIcon { get; private set; }
        public Image Mic { get; private set; }
        public Image MicMuted { get; private set; }

        public IconsService()
        {
            LoadIcons();
        }

        private void LoadIcons()
        {
            Mic = Properties.Resources.Mic;
            MicMuted = Properties.Resources.MicMuted;

            MicIcon = Icon.FromHandle(Properties.Resources.Mic.GetHicon());
            MicMutedIcon = Icon.FromHandle(Properties.Resources.MicMuted.GetHicon());
        }
    }
}
