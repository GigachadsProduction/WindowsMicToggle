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
        private bool _isMuted;

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int HWND_TOPMOST = -1;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_SHOWWINDOW = 0x0040;

        public OverlayForm(IconsService iconsService)
        {
            _iconsService = iconsService;
            InitializeComponent();
            SetupForm();
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst,
            ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc,
            int crKey, ref BLENDFUNCTION pblend, int dwFlags);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [StructLayout(LayoutKind.Sequential)]
        private struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        private const int ULW_ALPHA = 2;

        private void SetupForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            Size = new Size(40, 40);
            StartPosition = FormStartPosition.Manual;
            Location = new Point(10, 10);
            TopMost = true;
            ShowInTaskbar = false;

            // Убираем фон и делаем прозрачным
            BackColor = Color.Magenta;
            TransparencyKey = Color.Magenta;

            // Изначально скрываем оверлей
            Visible = false;

            SetWindowStyles();
        }

        private void SetWindowStyles()
        {
            // Устанавливаем расширенные стили для прозрачного окна поверх всех окон
            int initialStyle = GetWindowLong(Handle, GWL_EXSTYLE);
            SetWindowLong(Handle, GWL_EXSTYLE,
                initialStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);

            SetWindowPos(Handle, (IntPtr)HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }

        public void SetMicrophoneState(bool isMuted)
        {
            if (InvokeRequired)
            {
                Invoke((Action)(() => SetMicrophoneState(isMuted)));
                return;
            }

            _isMuted = isMuted;
            Visible = _isMuted;

            if (_isMuted)
            {
                UpdateOverlayIcon();
            }
        }

        private void UpdateOverlayIcon()
        {
            if (!_isMuted) return;

            var icon = _iconsService.MicMuted;
            if (icon == null) return;

            // Создаем bitmap для отрисовки
            using (var bitmap = new Bitmap(40, 40))
            using (var g = Graphics.FromImage(bitmap))
            {
                // Заливаем прозрачным
                g.Clear(Color.Transparent);

                // Рисуем иконку с правильным размером
                var resizedIcon = new Bitmap(icon, new Size(32, 32));
                g.DrawImage(resizedIcon, 4, 4);

                // Обновляем окно
                UpdateLayeredWindow(bitmap);
            }
        }

        private void UpdateLayeredWindow(Bitmap bitmap)
        {
            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memDc = CreateCompatibleDC(screenDc);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr oldBitmap = IntPtr.Zero;

            try
            {
                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                oldBitmap = SelectObject(memDc, hBitmap);

                var size = new Size(bitmap.Width, bitmap.Height);
                var pointSource = new Point(0, 0);
                var topPos = new Point(Left, Top);
                var blend = new BLENDFUNCTION
                {
                    BlendOp = 0,
                    BlendFlags = 0,
                    SourceConstantAlpha = 255,
                    AlphaFormat = 1
                };

                UpdateLayeredWindow(Handle, screenDc, ref topPos, ref size,
                    memDc, ref pointSource, 0, ref blend, ULW_ALPHA);
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, screenDc);
                if (hBitmap != IntPtr.Zero)
                {
                    SelectObject(memDc, oldBitmap);
                    DeleteObject(hBitmap);
                }
                DeleteDC(memDc);
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_isMuted && _iconsService.MicMuted != null)
            {
                var resizedIcon = new Bitmap(_iconsService.MicMuted, new Size(32, 32));
                e.Graphics.DrawImage(resizedIcon, 4, 4);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00080000; // WS_EX_LAYERED
                return cp;
            }
        }
    }
}