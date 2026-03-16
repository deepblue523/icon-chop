namespace IconChop
{
    public class AppSettings
    {
        public string? LastInputPath { get; set; }
        public bool AutoReloadInput { get; set; } = true;
        public HashSet<int> CheckedSizes { get; set; } = [32, 48, 64];
        public string? LastOutputDir { get; set; }
        /// <summary>One of: "Png", "Ico", "Both"</summary>
        public string OutputFormat { get; set; } = "Png";
        public string OutputPrefix { get; set; } = "icon";
        public List<string> OutputDirMru { get; set; } = [];
        public const int MruMax = 12;

        public int? FormX { get; set; }
        public int? FormY { get; set; }
        public int? FormWidth { get; set; }
        public int? FormHeight { get; set; }
        /// <summary>One of: "Normal", "Maximized", "Minimized"</summary>
        public string? FormWindowState { get; set; }

        public static string SettingsPath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "IconChop",
                "settings.json");

        public static AppSettings Load()
        {
            try
            {
                var path = SettingsPath;
                if (!File.Exists(path)) return new AppSettings();
                var json = File.ReadAllText(path);
                var settings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                var path = SettingsPath;
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
                var json = System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch
            {
                // ignore persistence errors
            }
        }
    }
}
