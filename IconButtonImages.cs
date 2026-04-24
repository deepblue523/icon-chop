namespace IconChop
{
    /// <summary>Loads PNGs from the app output <c>images</c> folder and assigns them to buttons.</summary>
    internal static class IconButtonImages
    {
        internal static Image? Load(string fileName, int maxEdgePx)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "images", fileName);
            if (!File.Exists(path))
                return null;
            using var src = Image.FromFile(path);
            var w = src.Width;
            var h = src.Height;
            if (w <= maxEdgePx && h <= maxEdgePx)
                return new Bitmap(src);
            var scale = Math.Min((double)maxEdgePx / w, (double)maxEdgePx / h);
            var nw = Math.Max(1, (int)Math.Round(w * scale));
            var nh = Math.Max(1, (int)Math.Round(h * scale));
            return new Bitmap(src, new Size(nw, nh));
        }

        internal static void Set(Button button, string fileName, int maxEdgePx)
        {
            var img = Load(fileName, maxEdgePx);
            if (img == null)
                return;
            button.Image?.Dispose();
            button.Image = img;
        }

        internal static void Clear(Button button)
        {
            var img = button.Image;
            button.Image = null;
            img?.Dispose();
        }
    }
}
