using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsMicToggle.Services;

namespace WindowsMicToggle.Forms
{
    public partial class WindowForm : Form
    {
        private readonly HotkeyService _hotkeyService;
        private readonly StartupService _startupService;
        private readonly ConfigService _configService;

        private bool _isRecordingHotkey;

        public WindowForm(HotkeyService hotkeyService, StartupService startupService, ConfigService configService)
        {
            _hotkeyService = hotkeyService;
            _startupService = startupService;
            _configService = configService;
            Init();
            LoadSettings();
        }

        private void Init()
        {
            // Основные настройки формы
            this.Text = "Microphone Toggle Settings";
            this.Size = new System.Drawing.Size(350, 200);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Элементы управления
            var lblHotkey = new Label
            {
                Text = "Hotkey:",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(80, 20)
            };

            var txtHotkey = new TextBox
            {
                Location = new System.Drawing.Point(100, 20),
                Size = new System.Drawing.Size(150, 20),
                ReadOnly = true,
                Name = "txtHotkey"
            };

            var btnRecordHotkey = new Button
            {
                Text = "Record",
                Location = new System.Drawing.Point(260, 18),
                Size = new System.Drawing.Size(60, 25),
                Name = "btnRecordHotkey"
            };

            var chkStartup = new CheckBox
            {
                Text = "Start with Windows",
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(150, 20),
                Name = "chkStartup"
            };

            var btnSave = new Button
            {
                Text = "Save",
                Location = new System.Drawing.Point(150, 100),
                Size = new System.Drawing.Size(75, 30),
                DialogResult = DialogResult.OK
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(235, 100),
                Size = new System.Drawing.Size(75, 30),
                DialogResult = DialogResult.Cancel
            };

            // Добавление элементов на форму
            this.Controls.AddRange(new Control[] {
                lblHotkey, txtHotkey, btnRecordHotkey, chkStartup, btnSave, btnCancel
            });

            // Обработчики событий
            btnRecordHotkey.Click += BtnRecordHotkey_Click;
            btnSave.Click += BtnSave_Click;
            this.KeyPreview = true;
            this.KeyDown += SettingsForm_KeyDown;
            this.KeyUp += SettingsForm_KeyUp;
        }

        private void LoadSettings()
        {
            var settings = _configService.Config;
            ((TextBox)Controls["txtHotkey"]).Text = settings.Hotkey ?? "";
            ((CheckBox)Controls["chkStartup"]).Checked = settings.StartWithWindows;
        }

        private void BtnRecordHotkey_Click(object sender, EventArgs e)
        {
            _isRecordingHotkey = true;
            ((Button)sender).Text = "Press keys...";
            ((TextBox)Controls["txtHotkey"]).Text = "Press key combination...";
            ((TextBox)Controls["txtHotkey"]).BackColor = Color.LightYellow;
        }

        private void SettingsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_isRecordingHotkey) return;

            e.SuppressKeyPress = true;

            // Игнорируем одиночные модификаторы
            if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.ControlKey ||
                e.KeyCode == Keys.Menu || e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
                return;

            var modifiers = GetModifiers(e);
            var hotkeyString = modifiers + e.KeyCode.ToString();

            ((TextBox)Controls["txtHotkey"]).Text = hotkeyString;
            _isRecordingHotkey = false;
            ((Button)Controls["btnRecordHotkey"]).Text = "Record";
            ((TextBox)Controls["txtHotkey"]).BackColor = SystemColors.Window;
        }

        private string GetModifiers(KeyEventArgs e)
        {
            var modifiers = "";
            if (e.Control) modifiers += "Ctrl+";
            if (e.Shift) modifiers += "Shift+";
            if (e.Alt) modifiers += "Alt+";
            return modifiers;
        }

        private void SettingsForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (_isRecordingHotkey)
                e.SuppressKeyPress = true;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var hotkey = ((TextBox)Controls["txtHotkey"]).Text;
                var startWithWindows = ((CheckBox)Controls["chkStartup"]).Checked;

                _configService.Config.StartWithWindows = startWithWindows;
                _configService.Config.Hotkey = hotkey;
                _configService.SaveConfig();

                // Применяем настройки
                _hotkeyService.RegisterHotkey(hotkey);

                if (startWithWindows)
                    _startupService.EnableStartup();
                else
                    _startupService.DisableStartup();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
