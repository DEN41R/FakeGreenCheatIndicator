using System.IO;
using System.Text.Json;

namespace FakeCheatIndicator
{
    public class Settings
    {
        private static readonly string SettingsFilePath = 
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        
        private static Settings? _instance;
        public static Settings Instance => _instance ??= Load();

       
        public int Opacity { get; set; } = 100;
        public int Size { get; set; } = 21;
        public int BorderThickness { get; set; } = 3;
        public int ColorIndex { get; set; } = 0; 

        public int PositionX { get; set; } = 25;
        public int PositionY { get; set; } = 15;

        
        public bool AutoStart { get; set; } = false;
        public bool MinimizeToTray { get; set; } = false;
        public bool StartMinimized { get; set; } = false;
        public bool SavePosition { get; set; } = true;

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения настроек: {ex.Message}");
            }
        }

        private static Settings Load()
        {
            if (!File.Exists(SettingsFilePath))
                return new Settings();

            try
            {
                var json = File.ReadAllText(SettingsFilePath);
                return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
            catch
            {
                return new Settings();
            }
        }

        public System.Windows.Media.Color GetIndicatorColor()
        {
            return ColorIndex switch
            {
                0 => System.Windows.Media.Color.FromRgb(0, 255, 0),    // Green
                1 => System.Windows.Media.Color.FromRgb(255, 0, 0),    // Red
                2 => System.Windows.Media.Color.FromRgb(0, 100, 255),  // Blue
                3 => System.Windows.Media.Color.FromRgb(255, 255, 0),  // Yellow
                4 => System.Windows.Media.Color.FromRgb(255, 255, 255),// White
                _ => System.Windows.Media.Color.FromRgb(0, 255, 0)     // Default Green
            };
        }
    }
}
