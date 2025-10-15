using System.Runtime.InteropServices;
using WindowsMicToggle.Services;
using System.Windows.Forms;
using System;
using System.Drawing;

namespace WindowsMicToggle.Forms
{
    public partial class OverlayForm : Form
    {
        private readonly IconsService _iconsService;

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int HWND_TOPMOST = -1;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private bool _isMuted;

        public OverlayForm(IconsService iconsService)
        {
            _iconsService = iconsService;
            InitializeComponent();
            SetupForm();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        private void SetupForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            Size = new Size(40, 40);
            StartPosition = FormStartPosition.Manual;
            Location = new Point(10, 10); // Верхний левый угол
            TopMost = true;
            ShowInTaskbar = false;
            BackColor = Color.Magenta;
            TransparencyKey = Color.Magenta;
            Opacity = 0.8;

            // Устанавливаем стили для оверлея поверх всех окон
            SetWindowStyles();
        }

        private void SetWindowStyles()
        {
            IntPtr initialStyle = GetWindowLong(Handle, GWL_EXSTYLE);
            SetWindowLong(Handle, GWL_EXSTYLE,
                (IntPtr)(initialStyle.ToInt64() | WS_EX_LAYERED | WS_EX_TRANSPARENT));

            SetWindowPos(Handle, (IntPtr)HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }

        public void SetMicrophoneState(bool isMuted)
        {
            Invoke((Action)(() =>
            {
                _isMuted = isMuted;
                Visible = _isMuted;
                Invalidate();
            }));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var icon = _isMuted ? _iconsService.MicMuted : _iconsService.Mic;
            e.Graphics.DrawImage(icon, 4, 4);
        }
    }
}
