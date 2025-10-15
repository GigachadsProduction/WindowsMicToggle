using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsMicToggle.Forms;
using WindowsMicToggle.Services;

namespace WindowsMicToggle
{
    internal static class Program
    {
        private static NotifyIcon _notifyIcon;
        private static HotkeyService _hotkeyService;
        private static MicService _microphoneService;
        private static StartupService _startupService;
        private static OverlayForm _overlayForm;
        private static ConfigService _configService;
        private static IconsService _iconsService;
        private static WindowForm _settingsForm;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ShowError(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                ShowError((Exception)e.ExceptionObject);

            InitializeApplication();
            Application.Run();
        }

        private static void InitializeApplication()
        {
            // Инициализация сервисов
            _configService = new ConfigService();
            _microphoneService = new MicService();
            _hotkeyService = new HotkeyService();
            _startupService = new StartupService();
            _iconsService = new IconsService();

            _overlayForm = new OverlayForm(_iconsService);

            // Создание иконки в трее
            CreateTrayIcon();

            // Загрузка сохраненных настроек
            ApplySettings();

            // Подписка на события
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;
            _microphoneService.MicrophoneStateChanged += OnMicrophoneStateChanged;
        }

        private static void ApplySettings()
        {
            var config = _configService.Config;

            if (!string.IsNullOrEmpty(config.Hotkey))
            {
                _hotkeyService.RegisterHotkey(config.Hotkey);
            }

            if (config.StartWithWindows)
            {
                _startupService.EnableStartup();
            }
        }

        private static void CreateTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = _iconsService.MicIcon, // Добавьте иконку в ресурсы
                Text = "Microphone Toggle",
                Visible = true,
                ContextMenuStrip = CreateContextMenu()
            };

            _notifyIcon.DoubleClick += (s, e) => ShowSettings();
        }

        private static ContextMenuStrip CreateContextMenu()
        {
            var menu = new ContextMenuStrip();

            menu.Items.Add("Settings", null, (s, e) => ShowSettings());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit", null, (s, e) => ExitApplication());

            return menu;
        }

        private static void OnHotkeyPressed(object sender, EventArgs e)
        {
            _microphoneService.ToggleMicrophone();
        }

        private static void OnMicrophoneStateChanged(object sender, bool isMuted)
        {
            // Обновляем оверлей
            _overlayForm.SetMicrophoneState(isMuted);

            // Обновляем иконку в трее
            _notifyIcon.Icon = isMuted ? _iconsService.MicMutedIcon : _iconsService.MicIcon;
        }

        private static void ShowSettings()
        {
            if (_settingsForm == null || _settingsForm.IsDisposed)
            {
                _settingsForm = new WindowForm(_hotkeyService, _startupService, _configService);
                _settingsForm.FormClosed += (s, e) => _settingsForm = null;
            }

            _settingsForm.Show();
            _settingsForm.BringToFront();
        }

        private static void ExitApplication()
        {
            _notifyIcon.Visible = false;
            _hotkeyService?.Dispose();
            _overlayForm?.Close();
            Application.Exit();
        }

        private static void ShowError(Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Microphone Toggle Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
