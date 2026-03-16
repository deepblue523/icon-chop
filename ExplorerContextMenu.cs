using Microsoft.Win32;

namespace IconChop
{
    /// <summary>
    /// Registers or unregisters "Open with IconChop" in Windows Explorer context menu for common image file types only.
    /// </summary>
    public static class ExplorerContextMenu
    {
        private const string MenuKeyName = "Open with IconChop";
        private const string MenuLabel = "Open with IconChop";

        /// <summary>Extensions that get the context menu (without leading dot in registry; we add it).</summary>
        public static readonly string[] ImageExtensions =
        [
            ".png", ".bmp", ".jpg", ".jpeg", ".gif", ".tiff", ".tif", ".ico", ".webp"
        ];

        /// <summary>Legacy key when context menu was for all files (*).</summary>
        private const string LegacyAllFilesKey = @"Software\Classes\*\shell\Open with IconChop";

        public static bool IsRegistered()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{ImageExtensions[0]}\shell\{MenuKeyName}");
                return key != null;
            }
            catch
            {
                return false;
            }
        }

        private static void RemoveLegacyAllFilesKey()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(LegacyAllFilesKey, throwOnMissingSubKey: false);
            }
            catch { /* ignore */ }
        }

        public static bool Register()
        {
            string? exe = Application.ExecutablePath;
            if (string.IsNullOrEmpty(exe)) return false;

            try
            {
                RemoveLegacyAllFilesKey();
                foreach (var ext in ImageExtensions)
                {
                    string basePath = $@"Software\Classes\{ext}\shell\{MenuKeyName}";
                    using (var key = Registry.CurrentUser.CreateSubKey(basePath, true))
                    {
                        key?.SetValue(null, MenuLabel);
                    }
                    using (var cmdKey = Registry.CurrentUser.CreateSubKey($@"{basePath}\command", true))
                    {
                        cmdKey?.SetValue(null, $"\"{exe}\" \"%1\"");
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Unregister()
        {
            try
            {
                RemoveLegacyAllFilesKey();
                foreach (var ext in ImageExtensions)
                {
                    string path = $@"Software\Classes\{ext}\shell\{MenuKeyName}";
                    Registry.CurrentUser.DeleteSubKeyTree(path, throwOnMissingSubKey: false);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
