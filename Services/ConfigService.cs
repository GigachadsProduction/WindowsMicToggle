using System;
using System.IO;
using Newtonsoft.Json;
using WindowsMicToggle.Configuration;

namespace WindowsMicToggle.Services
{
    public class ConfigService
    {
        private readonly string _configPath;
        private AppConfig _config;

        public AppConfig Config => _config;

        public ConfigService()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

            LoadConfig();
        }

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    _config = JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
                    
                }
                else
                {
                    _config = new AppConfig();
                    SaveConfig();
                }
            }
            catch
            {
                _config = new AppConfig();
            }
        }

        public void SaveConfig()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                System.Diagnostics.Debug.WriteLine($"Error saving config: {ex.Message}");
            }
        }

        public void UpdateConfig(Action<AppConfig> updateAction)
        {
            updateAction?.Invoke(_config);
            SaveConfig();
        }
    }
}
