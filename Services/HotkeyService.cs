using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowsMicToggle.Services
{
    public class HotkeyService : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private readonly int _hotkeyId = 9000;
        private uint _key;
        private uint _modifiers;
        private HotkeyWindow _window;

        public HotkeyService()
        {
            CreateHiddenWindow();
        }

        public void Dispose()
        {
            UnregisterHotkey();
            _window?.DestroyHandle();
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public event EventHandler HotkeyPressed;

        private void CreateHiddenWindow()
        {
            _window = new HotkeyWindow();
            _window.HotkeyPressed += (s, e) => HotkeyPressed?.Invoke(this, EventArgs.Empty);
        }

        public bool RegisterHotkey(string hotkeyString)
        {
            UnregisterHotkey();

            if (string.IsNullOrEmpty(hotkeyString))
                return false;

            try
            {
                var keys = (Keys)new KeysConverter().ConvertFromString(hotkeyString);
                return RegisterHotkey(keys);
            }
            catch
            {
                return false;
            }
        }

        public bool RegisterHotkey(Keys keys)
        {
            UnregisterHotkey();

            _modifiers = 0;
            _key = (uint)(keys & ~Keys.Modifiers);

            if ((keys & Keys.Control) == Keys.Control)
                _modifiers |= 0x0002; // MOD_CONTROL
            if ((keys & Keys.Shift) == Keys.Shift)
                _modifiers |= 0x0004; // MOD_SHIFT
            if ((keys & Keys.Alt) == Keys.Alt)
                _modifiers |= 0x0001; // MOD_ALT

            return RegisterHotKey(_window.Handle, _hotkeyId, _modifiers, _key);
        }

        public void UnregisterHotkey()
        {
            UnregisterHotKey(_window.Handle, _hotkeyId);
        }

        private class HotkeyWindow : NativeWindow
        {
            public event EventHandler HotkeyPressed;

            public HotkeyWindow()
            {
                CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    HotkeyPressed?.Invoke(this, EventArgs.Empty);
                }
                base.WndProc(ref m);
            }
        }
    }
}