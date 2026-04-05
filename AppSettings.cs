namespace IconChop
{
    public class AppSettings
    {
        public string? LastInputPath { get; set; }
        /// <summary>Recently opened source image files (newest first), max <see cref="InputImageMruMax"/>.</summary>
        public List<string> InputImageMru { get; set; } = [];
        public const int InputImageMruMax = 10;
        public bool AutoReloadInput { get; set; } = true;
        public HashSet<int> CheckedSizes { get; set; } = [32, 48, 64];
        public string? LastOutputDir { get; set; }
        /// <summary>One of: "Png", "Ico", "Both"</summary>
        public string OutputFormat { get; set; } = "Png";
        public string OutputPrefix { get; set; } = "icon";
        public bool AutoNameIcons { get; set; }
        /// <summary>Last app/context text for Auto-name (empty = none).</summary>
        public string? LastAutoNameAppContext { get; set; }
        /// <summary>Recent app descriptions for Auto-name (newest first), max <see cref="AutoNameAppContextMruMax"/>.</summary>
        public List<string> AutoNameAppContextMru { get; set; } = [];
        public const int AutoNameAppContextMruMax = 10;
        public List<string> OutputDirMru { get; set; } = [];
        public const int MruMax = 12;

        /// <summary>Last-used image-generation prompts (newest first).</summary>
        public List<string> PromptMru { get; set; } = [];
        public const int PromptMruMax = 10;
        /// <summary>Image → Generate: append Auto-name app description to the API prompt when checked.</summary>
        public bool IncludeAutoNameContextInImagePrompt { get; set; }

        public int? FormX { get; set; }
        public int? FormY { get; set; }
        public int? FormWidth { get; set; }
        public int? FormHeight { get; set; }
        /// <summary>One of: "Normal", "Maximized", "Minimized"</summary>
        public string? FormWindowState { get; set; }

        // OpenAI Images API (POST {BaseUrl}/images/generations)
        public string? OpenAiApiKey { get; set; }
        public string OpenAiApiBaseUrl { get; set; } = "https://api.openai.com/v1";
        public string OpenAiImageModel { get; set; } = "dall-e-3";
        public string OpenAiImageSize { get; set; } = "1024x1024";
        /// <summary>For dall-e-3: standard or hd.</summary>
        public string OpenAiImageQuality { get; set; } = "standard";
        /// <summary>Vision model used by Auto-name to describe icons.</summary>
        public string OpenAiNamingModel { get; set; } = "gpt-4o-mini";

        // OpenAI Text/Chat API overrides (fall back to image API values when blank)
        public string? OpenAiTextApiKey { get; set; }
        public string? OpenAiTextBaseUrl { get; set; }

        /// <summary>~/.icon-chop/config.json on Unix; %USERPROFILE%\icon-chop\config.json on Windows.</summary>
        public static string ConfigPath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "icon-chop",
                "config.json");

        /// <summary>Subfolder of the config directory for temporary files (e.g. generated source images).</summary>
        public static string TempDirectoryPath =>
            Path.Combine(
                Path.GetDirectoryName(ConfigPath)
                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "icon-chop"),
                "tmp");

        /// <summary>Previous location; read once for migration if <see cref="ConfigPath"/> is missing.</summary>
        public static string LegacySettingsPath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "IconChop",
                "settings.json");

        private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var settings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                    var loaded = settings ?? new AppSettings();
                    loaded.InputImageMru ??= [];
                    return loaded;
                }

                if (File.Exists(LegacySettingsPath))
                {
                    var json = File.ReadAllText(LegacySettingsPath);
                    var settings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                    var result = settings ?? new AppSettings();
                    result.InputImageMru ??= [];
                    try
                    {
                        result.Save();
                    }
                    catch
                    {
                        // keep in-memory settings even if new path is not writable
                    }

                    return result;
                }

                return new AppSettings();
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
                var path = ConfigPath;
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
                var json = System.Text.Json.JsonSerializer.Serialize(this, JsonOptions);
                File.WriteAllText(path, json);
            }
            catch
            {
                // ignore persistence errors
            }
        }
    }
}
