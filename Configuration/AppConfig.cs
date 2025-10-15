using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMicToggle.Configuration
{
    public class AppConfig
    {
        public string Hotkey { get; set; } = "Ctrl+Shift+M";

        public bool StartWithWindows { get; set; } = true;
    }
}
