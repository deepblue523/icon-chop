using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace IconChop
{
    public partial class Form1 : Form
    {
        private Bitmap? _sourceImage;
        private List<Bitmap> _detectedIcons = [];
        private Color _detectedBgColor;
        private AppSettings _settings = AppSettings.Load();
        private string? _currentInputPath;
        private FileSystemWatcher? _fileWatcher;
        private readonly HashSet<int> _selectedIndices = [];

        private const int BgTolerance = 30;
        private const int PreviewTileSize = 72;
        private const int CheckerCell = 8;
        private const int MinGapPx = 3;
        private static readonly Color SentinelPink = Color.FromArgb(255, 255, 0, 255);
        private static readonly Color SelectionGlowColor = Color.FromArgb(255, 200, 80);
        private const int SelectionBorderPadding = 4;

        private readonly string? _initialFilePath;

        public Form1(string? initialFilePath = null)
        {
            _initialFilePath = initialFilePath;
            InitializeComponent();
            try
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? Icon;
            }
            catch { /* use default if exe icon unavailable */ }
            SetupDragDrop();
            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            RestoreFormBounds();
            UpdateContextMenuButtonText();
            chkAutoReload.Checked = _settings.AutoReloadInput;
            var fmt = _settings.OutputFormat;
            cboOutputFormat.SelectedIndex = fmt == "Ico" ? 1 : fmt == "Both" ? 2 : 0;
            txtOutputPrefix.Text = _settings.OutputPrefix ?? "icon";
            foreach (Control c in panelSizes.Controls)
                if (c is CheckBox cb && cb.Tag is int sz)
                    cb.Checked = _settings.CheckedSizes.Contains(sz);
            cboOutputDir.Items.Clear();
            foreach (var path in _settings.OutputDirMru)
                if (Directory.Exists(path))
                    cboOutputDir.Items.Add(path);
            if (!string.IsNullOrWhiteSpace(_settings.LastOutputDir) && Directory.Exists(_settings.LastOutputDir))
                cboOutputDir.Text = _settings.LastOutputDir;
            if (!string.IsNullOrWhiteSpace(_initialFilePath) && File.Exists(_initialFilePath))
                LoadImage(_initialFilePath);
            else if (!string.IsNullOrWhiteSpace(_settings.LastInputPath) && File.Exists(_settings.LastInputPath))
                LoadImage(_settings.LastInputPath);
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            StopFileWatcher();
            _settings.AutoReloadInput = chkAutoReload.Checked;
            _settings.OutputFormat = cboOutputFormat.SelectedIndex switch { 1 => "Ico", 2 => "Both", _ => "Png" };
            var pre = (txtOutputPrefix.Text ?? "").Trim();
            _settings.OutputPrefix = string.IsNullOrEmpty(pre) ? "icon" : pre;
            _settings.CheckedSizes = panelSizes.Controls
                .OfType<CheckBox>()
                .Where(cb => cb.Checked && cb.Tag is int)
                .Select(cb => (int)cb.Tag!)
                .ToHashSet();
            var outDir = (cboOutputDir.Text ?? "").Trim();
            if (!string.IsNullOrEmpty(outDir))
            {
                _settings.LastOutputDir = outDir;
                var mru = new List<string> { outDir };
                foreach (var item in cboOutputDir.Items.Cast<string>())
                    if (item != outDir && Directory.Exists(item))
                        mru.Add(item);
                _settings.OutputDirMru = mru.Take(AppSettings.MruMax).ToList();
            }
            _settings.LastInputPath = _currentInputPath;
            SaveFormBounds();
            _settings.Save();
        }

        private void RestoreFormBounds()
        {
            if (_settings.FormWidth is not { } w || _settings.FormHeight is not { } h || w < 100 || h < 100)
                return;
            var x = _settings.FormX ?? 0;
            var y = _settings.FormY ?? 0;
            var bounds = new Rectangle(x, y, w, h);
            var working = Screen.GetWorkingArea(bounds);
            bounds.X = Math.Clamp(bounds.X, working.Left, working.Right - Math.Min(bounds.Width, working.Width));
            bounds.Y = Math.Clamp(bounds.Y, working.Top, working.Bottom - Math.Min(bounds.Height, working.Height));
            StartPosition = FormStartPosition.Manual;
            Bounds = bounds;
            if (_settings.FormWindowState == "Maximized")
                WindowState = FormWindowState.Maximized;
        }

        private void SaveFormBounds()
        {
            var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            _settings.FormX = bounds.X;
            _settings.FormY = bounds.Y;
            _settings.FormWidth = bounds.Width;
            _settings.FormHeight = bounds.Height;
            _settings.FormWindowState = WindowState switch
            {
                FormWindowState.Maximized => "Maximized",
                FormWindowState.Minimized => "Minimized",
                _ => "Normal"
            };
        }

        private void SetupDragDrop()
        {
            picSource.AllowDrop = true;

            void OnDragEnter(object? s, DragEventArgs e)
            {
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                    e.Effect = DragDropEffects.Copy;
            }

            void OnDragDrop(object? s, DragEventArgs e)
            {
                if (e.Data?.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } files)
                    LoadImage(files[0]);
            }

            DragEnter += OnDragEnter;
            DragDrop += OnDragDrop;
            picSource.DragEnter += OnDragEnter;
            picSource.DragDrop += OnDragDrop;
        }

        // -------------------------------------------------------------------
        //  Image loading
        // -------------------------------------------------------------------

        private void UpdateContextMenuButtonText()
        {
            btnContextMenu.Text = ExplorerContextMenu.IsRegistered()
                ? "Remove from Explorer context menu"
                : "Add to Explorer context menu";
        }

        private void BtnContextMenu_Click(object? sender, EventArgs e)
        {
            bool ok;
            if (ExplorerContextMenu.IsRegistered())
                ok = ExplorerContextMenu.Unregister();
            else
                ok = ExplorerContextMenu.Register();

            if (ok)
            {
                UpdateContextMenuButtonText();
                string msg = ExplorerContextMenu.IsRegistered()
                    ? "Context menu \"Open with IconChop\" is now on image files (e.g. .png, .jpg)."
                    : "Context menu \"Open with IconChop\" has been removed from image files.";
                lblStatus.Text = msg;
            }
            else
            {
                MessageBox.Show("Could not update the Explorer context menu. Try running as administrator or check permissions.",
                    "Context menu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnLoadImage_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.bmp;*.jpg;*.jpeg;*.gif;*.tiff|All Files|*.*",
                Title = "Select Icon Sheet"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
                LoadImage(ofd.FileName);
        }

        private void LoadImage(string path)
        {
            try
            {
                StopFileWatcher();
                _sourceImage?.Dispose();
                using var stream = File.OpenRead(path);
                _sourceImage = new Bitmap(stream);
                picSource.Image = _sourceImage;
                _currentInputPath = path;
                Text = $"IconChop — {Path.GetFileName(path)}";
                lblStatus.Text = $"{_sourceImage.Width}\u00d7{_sourceImage.Height}";
                DetectAndPreview();
                if (chkAutoReload.Checked && !string.IsNullOrEmpty(path))
                    StartFileWatcher(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load image:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartFileWatcher(string filePath)
        {
            StopFileWatcher();
            var dir = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);
            if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(fileName)) return;
            _fileWatcher = new FileSystemWatcher(dir)
            {
                Filter = fileName,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };
            _fileWatcher.Changed += (_, _) =>
            {
                try { _fileWatcher.EnableRaisingEvents = false; } catch { }
                if (InvokeRequired)
                    BeginInvoke(ReloadCurrentImage);
                else
                    ReloadCurrentImage();
            };
            _fileWatcher.EnableRaisingEvents = true;
        }

        private void ReloadCurrentImage()
        {
            if (string.IsNullOrEmpty(_currentInputPath) || !File.Exists(_currentInputPath)) return;
            try
            {
                LoadImage(_currentInputPath);
            }
            catch
            {
                // ignore; watcher may fire during save or file locked
            }
        }

        private void StopFileWatcher()
        {
            _fileWatcher?.Dispose();
            _fileWatcher = null;
        }

        // -------------------------------------------------------------------
        //  Detection + preview
        // -------------------------------------------------------------------

        private void DetectAndPreview()
        {
            if (_sourceImage is null) return;

            Cursor = Cursors.WaitCursor;

            foreach (var icon in _detectedIcons) icon.Dispose();
            _detectedIcons.Clear();
            ClearPreviewPanel();

            _detectedBgColor = DetectBackgroundColor(_sourceImage);

            if (_detectedBgColor.A < 10)
            {
                _detectedIcons = ExtractIcons(_sourceImage, _detectedBgColor);
            }
            else
            {
                using var working = RecolorBackground(_sourceImage, _detectedBgColor, SentinelPink);
                _detectedIcons = ExtractIcons(working, SentinelPink);
            }

            _selectedIndices.Clear();
            int tileTotal = PreviewTileSize + 2 + SelectionBorderPadding * 2;
            for (int idx = 0; idx < _detectedIcons.Count; idx++)
            {
                var icon = _detectedIcons[idx];
                var preview = CompositeOnCheckerboard(icon, PreviewTileSize);
                var pb = new PictureBox
                {
                    Size = new Size(PreviewTileSize + 2, PreviewTileSize + 2),
                    SizeMode = PictureBoxSizeMode.CenterImage,
                    Image = preview,
                    BorderStyle = BorderStyle.None,
                    Location = new Point(SelectionBorderPadding, SelectionBorderPadding),
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand
                };
                var panel = new Panel
                {
                    Size = new Size(tileTotal, tileTotal),
                    BackColor = Color.White,
                    Padding = new Padding(SelectionBorderPadding),
                    Margin = new Padding(4),
                    Tag = idx,
                    Cursor = Cursors.Hand
                };
                panel.Controls.Add(pb);
                panel.Click += PreviewTile_Click;
                pb.Click += PreviewTile_Click;
                flowPreview.Controls.Add(panel);
            }

            UpdatePreviewCountLabel();
            lblStatus.Text = $"{_sourceImage.Width}\u00d7{_sourceImage.Height}  —  {_detectedIcons.Count} icons";
            Cursor = Cursors.Default;
        }

        private void ClearPreviewPanel()
        {
            foreach (Control c in flowPreview.Controls)
            {
                foreach (PictureBox child in c.Controls.OfType<PictureBox>())
                    child.Image?.Dispose();
                c.Dispose();
            }
            flowPreview.Controls.Clear();
            _selectedIndices.Clear();
        }

        private void PreviewTile_Click(object? sender, EventArgs e)
        {
            int idx = sender is Panel p ? (p.Tag is int i ? i : -1) : (sender is PictureBox pb && pb.Parent?.Tag is int j ? j : -1);
            if (idx < 0) return;
            if (_selectedIndices.Contains(idx))
                _selectedIndices.Remove(idx);
            else
                _selectedIndices.Add(idx);
            RefreshSelectionAppearance();
            UpdatePreviewCountLabel();
        }

        private void RefreshSelectionAppearance()
        {
            foreach (Control c in flowPreview.Controls)
            {
                if (c is Panel panel && panel.Tag is int i)
                    panel.BackColor = _selectedIndices.Contains(i) ? SelectionGlowColor : Color.White;
            }
        }

        private void BtnSelectAll_Click(object? sender, EventArgs e)
        {
            _selectedIndices.Clear();
            for (int i = 0; i < _detectedIcons.Count; i++)
                _selectedIndices.Add(i);
            RefreshSelectionAppearance();
            UpdatePreviewCountLabel();
        }

        private void BtnDeselectAll_Click(object? sender, EventArgs e)
        {
            _selectedIndices.Clear();
            RefreshSelectionAppearance();
            UpdatePreviewCountLabel();
        }

        private void UpdatePreviewCountLabel()
        {
            int n = _detectedIcons.Count;
            if (_selectedIndices.Count == 0)
                lblPreviewCount.Text = $"  Preview  ({n} icons detected) — click to toggle selection; export all if none selected";
            else
                lblPreviewCount.Text = $"  Preview  ({n} icons) — {_selectedIndices.Count} selected for export";
        }

        // -------------------------------------------------------------------
        //  Background colour detection
        // -------------------------------------------------------------------

        private static Color DetectBackgroundColor(Bitmap bmp)
        {
            var samples = new[]
            {
                bmp.GetPixel(0, 0),
                bmp.GetPixel(bmp.Width - 1, 0),
                bmp.GetPixel(0, bmp.Height - 1),
                bmp.GetPixel(bmp.Width - 1, bmp.Height - 1),
                bmp.GetPixel(bmp.Width / 2, 0),
                bmp.GetPixel(0, bmp.Height / 2),
                bmp.GetPixel(bmp.Width - 1, bmp.Height / 2),
                bmp.GetPixel(bmp.Width / 2, bmp.Height - 1)
            };

            int transparentCount = samples.Count(c => c.A < 10);
            if (transparentCount > samples.Length / 2)
                return Color.FromArgb(0, 0, 0, 0);

            return samples
                .GroupBy(c => c.ToArgb())
                .OrderByDescending(g => g.Count())
                .First().First();
        }

        // -------------------------------------------------------------------
        //  Flood-fill background with sentinel colour
        // -------------------------------------------------------------------

        private static Bitmap RecolorBackground(Bitmap source, Color originalBg, Color replacement)
        {
            int w = source.Width, h = source.Height;
            var result = source.Clone(new Rectangle(0, 0, w, h), PixelFormat.Format32bppArgb);

            var data = result.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            byte[] px = new byte[data.Stride * h];
            Marshal.Copy(data.Scan0, px, 0, px.Length);
            int stride = data.Stride;

            bool bgIsTransparent = originalBg.A < 10;
            bool[] visited = new bool[w * h];
            var queue = new Queue<(int x, int y)>();

            for (int x = 0; x < w; x++)
            {
                TryEnqueue(x, 0);
                TryEnqueue(x, h - 1);
            }
            for (int y = 1; y < h - 1; y++)
            {
                TryEnqueue(0, y);
                TryEnqueue(w - 1, y);
            }

            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();
                int idx = cy * stride + cx * 4;
                px[idx] = replacement.B;
                px[idx + 1] = replacement.G;
                px[idx + 2] = replacement.R;
                px[idx + 3] = replacement.A;

                if (cx > 0) TryEnqueue(cx - 1, cy);
                if (cx < w - 1) TryEnqueue(cx + 1, cy);
                if (cy > 0) TryEnqueue(cx, cy - 1);
                if (cy < h - 1) TryEnqueue(cx, cy + 1);
            }

            Marshal.Copy(px, 0, data.Scan0, px.Length);
            result.UnlockBits(data);
            return result;

            void TryEnqueue(int x, int y)
            {
                int flat = y * w + x;
                if (visited[flat]) return;
                int i = y * stride + x * 4;
                if (!IsBg(px[i + 2], px[i + 1], px[i], px[i + 3], originalBg, bgIsTransparent))
                    return;
                visited[flat] = true;
                queue.Enqueue((x, y));
            }
        }

        // -------------------------------------------------------------------
        //  Grid-based icon extraction
        // -------------------------------------------------------------------

        private static List<Bitmap> ExtractIcons(Bitmap source, Color bgColor)
        {
            int w = source.Width, h = source.Height;

            var data = source.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte[] px = new byte[data.Stride * h];
            Marshal.Copy(data.Scan0, px, 0, px.Length);
            int stride = data.Stride;
            source.UnlockBits(data);

            bool bgIsTransparent = bgColor.A < 10;

            bool[] colHit = new bool[w];
            bool[] rowHit = new bool[h];

            for (int y = 0; y < h; y++)
            {
                int rowOff = y * stride;
                for (int x = 0; x < w; x++)
                {
                    int i = rowOff + x * 4;
                    if (!IsBg(px[i + 2], px[i + 1], px[i], px[i + 3], bgColor, bgIsTransparent))
                    {
                        colHit[x] = true;
                        rowHit[y] = true;
                    }
                }
            }

            var colBands = MergeBands(colHit, MinGapPx);
            var rowBands = MergeBands(rowHit, MinGapPx);

            var icons = new List<Bitmap>();

            foreach (var (ry1, ry2) in rowBands)
            {
                foreach (var (cx1, cx2) in colBands)
                {
                    var rect = TightBounds(px, stride, cx1, ry1,
                        cx2 - cx1 + 1, ry2 - ry1 + 1, bgColor, bgIsTransparent);

                    if (rect.Width < 3 || rect.Height < 3)
                        continue;

                    int sq = Math.Max(rect.Width, rect.Height);
                    var icon = new Bitmap(sq, sq, PixelFormat.Format32bppArgb);

                    using (var g = Graphics.FromImage(icon))
                    {
                        g.Clear(Color.Transparent);
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.PixelOffsetMode = PixelOffsetMode.Half;
                        int ox = (sq - rect.Width) / 2;
                        int oy = (sq - rect.Height) / 2;
                        g.DrawImage(source,
                            new Rectangle(ox, oy, rect.Width, rect.Height),
                            rect, GraphicsUnit.Pixel);
                    }

                    StripBackground(icon, bgColor, bgIsTransparent);
                    icons.Add(icon);
                }
            }

            return icons;
        }

        private static bool IsBg(byte r, byte g, byte b, byte a,
            Color bg, bool bgTransparent, int tol = BgTolerance)
        {
            if (bgTransparent)
                return a < tol;

            if (a < 10) return true;
            return Math.Abs(r - bg.R) <= tol
                && Math.Abs(g - bg.G) <= tol
                && Math.Abs(b - bg.B) <= tol;
        }

        private static List<(int start, int end)> MergeBands(bool[] hits, int minGap)
        {
            var raw = new List<(int s, int e)>();
            int i = 0;
            while (i < hits.Length)
            {
                if (hits[i])
                {
                    int s = i;
                    while (i < hits.Length && hits[i]) i++;
                    raw.Add((s, i - 1));
                }
                else i++;
            }

            if (raw.Count == 0) return raw;

            var merged = new List<(int start, int end)>();
            var cur = raw[0];
            for (int j = 1; j < raw.Count; j++)
            {
                if (raw[j].s - cur.e <= minGap)
                    cur = (cur.s, raw[j].e);
                else
                {
                    merged.Add(cur);
                    cur = raw[j];
                }
            }
            merged.Add(cur);
            return merged;
        }

        private static Rectangle TightBounds(byte[] px, int stride,
            int cellX, int cellY, int cellW, int cellH,
            Color bg, bool bgTransparent)
        {
            int minX = cellX + cellW, maxX = cellX - 1;
            int minY = cellY + cellH, maxY = cellY - 1;

            for (int y = cellY; y < cellY + cellH; y++)
            {
                int rowOff = y * stride;
                for (int x = cellX; x < cellX + cellW; x++)
                {
                    int i = rowOff + x * 4;
                    if (!IsBg(px[i + 2], px[i + 1], px[i], px[i + 3], bg, bgTransparent))
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (maxX < minX || maxY < minY)
                return Rectangle.Empty;

            return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        private static void StripBackground(Bitmap bmp, Color bg, bool bgTransparent, int tol = BgTolerance)
        {
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            byte[] px = new byte[data.Stride * bmp.Height];
            Marshal.Copy(data.Scan0, px, 0, px.Length);

            for (int i = 0; i < px.Length; i += 4)
            {
                if (IsBg(px[i + 2], px[i + 1], px[i], px[i + 3], bg, bgTransparent, tol))
                {
                    px[i] = 0;
                    px[i + 1] = 0;
                    px[i + 2] = 0;
                    px[i + 3] = 0;
                }
            }

            Marshal.Copy(px, 0, data.Scan0, px.Length);
            bmp.UnlockBits(data);
        }

        // -------------------------------------------------------------------
        //  Preview helpers
        // -------------------------------------------------------------------

        private static Bitmap CompositeOnCheckerboard(Bitmap icon, int tileSize)
        {
            var bmp = new Bitmap(tileSize, tileSize, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);

            using var light = new SolidBrush(Color.FromArgb(255, 255, 255));
            using var dark = new SolidBrush(Color.FromArgb(210, 210, 210));

            for (int y = 0; y < tileSize; y += CheckerCell)
                for (int x = 0; x < tileSize; x += CheckerCell)
                    g.FillRectangle(((x / CheckerCell + y / CheckerCell) & 1) == 0 ? light : dark,
                        x, y, CheckerCell, CheckerCell);

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.DrawImage(icon, 0, 0, tileSize, tileSize);
            return bmp;
        }

        // -------------------------------------------------------------------
        //  Output directory
        // -------------------------------------------------------------------

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            var initial = (cboOutputDir.Text ?? "").Trim();
            using var dlg = new FolderBrowserDialog
            {
                Description = "Select output directory for chopped icons",
                UseDescriptionForTitle = true,
                SelectedPath = Directory.Exists(initial) ? initial : ""
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var path = dlg.SelectedPath;
                cboOutputDir.Text = path;
                var items = cboOutputDir.Items.Cast<string>().ToList();
                items.Remove(path);
                items.Insert(0, path);
                cboOutputDir.Items.Clear();
                foreach (var p in items.Take(AppSettings.MruMax))
                    cboOutputDir.Items.Add(p);
            }
        }

        // -------------------------------------------------------------------
        //  Chop & export
        // -------------------------------------------------------------------

        private void BtnChop_Click(object? sender, EventArgs e)
        {
            if (_detectedIcons.Count == 0)
            {
                MessageBox.Show("No icons detected. Please load an icon sheet first.",
                    "Nothing to export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var outDir = (cboOutputDir.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(outDir))
            {
                MessageBox.Show("Please select an output folder first.",
                    "No output folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var sizes = SelectedSizes();
            if (sizes.Count == 0)
            {
                MessageBox.Show("Please select at least one output size.",
                    "No sizes selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var indicesToExport = _selectedIndices.Count > 0
                ? _selectedIndices.OrderBy(i => i).ToList()
                : Enumerable.Range(0, _detectedIcons.Count).ToList();

            try
            {
                Cursor = Cursors.WaitCursor;
                Directory.CreateDirectory(outDir);
                int written = 0;
                int exportIndex = 0;

                bool savePng = cboOutputFormat.SelectedIndex is 0 or 2; // PNG only or Both
                bool saveIco = cboOutputFormat.SelectedIndex is 1 or 2; // ICO only or Both
                string prefix = GetOutputPrefix();
                bool singleIcon = indicesToExport.Count == 1;

                foreach (int idx in indicesToExport)
                {
                    var iconBitmaps = new List<(int size, Bitmap bmp)>();
                    string baseName = singleIcon ? prefix : $"{prefix}_{exportIndex + 1:D3}";
                    try
                    {
                        foreach (int sz in sizes)
                        {
                            using var resized = new Bitmap(sz, sz, PixelFormat.Format32bppArgb);
                            using (var g = Graphics.FromImage(resized))
                            {
                                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                g.CompositingQuality = CompositingQuality.HighQuality;
                                g.SmoothingMode = SmoothingMode.HighQuality;
                                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                g.Clear(Color.Transparent);
                                g.DrawImage(_detectedIcons[idx], 0, 0, sz, sz);
                            }

                            if (savePng)
                            {
                                string name = $"{baseName}_{sz}x{sz}.png";
                                resized.Save(Path.Combine(outDir, name), ImageFormat.Png);
                                written++;
                            }
                            if (saveIco)
                                iconBitmaps.Add((sz, new Bitmap(resized)));
                        }

                        if (saveIco && iconBitmaps.Count > 0)
                        {
                            string icoPath = Path.Combine(outDir, $"{baseName}.ico");
                            SaveMultiSizeIco(icoPath, iconBitmaps);
                            written++;
                        }
                    }
                    finally
                    {
                        foreach (var (_, bmp) in iconBitmaps)
                            bmp.Dispose();
                    }
                    exportIndex++;
                }

                Cursor = Cursors.Default;
                lblStatus.Text = $"Exported {written} files to {outDir}";
                MessageBox.Show($"Successfully exported {written} icon files.",
                    "Export complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Export failed:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<int> SelectedSizes()
        {
            return panelSizes.Controls
                .OfType<CheckBox>()
                .Where(cb => cb.Checked && cb.Tag is int)
                .Select(cb => (int)cb.Tag!)
                .OrderBy(s => s)
                .ToList();
        }

        private string GetOutputPrefix()
        {
            var raw = (txtOutputPrefix.Text ?? "").Trim();
            if (string.IsNullOrEmpty(raw)) return "icon";
            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = new string(raw.Select(c => Array.IndexOf(invalid, c) >= 0 ? '_' : c).ToArray()).Trim(' ', '.');
            return string.IsNullOrEmpty(sanitized) ? "icon" : sanitized;
        }

        /// <summary>
        /// Writes a multi-size .ico file (PNG-compressed images; supported on Windows Vista+).
        /// </summary>
        private static void SaveMultiSizeIco(string path, List<(int size, Bitmap bmp)> images)
        {
            int count = images.Count;
            int headerSize = 6 + 16 * count;
            var pngBlobs = new List<byte[]>();
            foreach (var (_, bmp) in images)
            {
                using var ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Png);
                pngBlobs.Add(ms.ToArray());
            }

            int currentOffset = headerSize;
            using var fs = File.Create(path);
            using var bw = new BinaryWriter(fs);

            // ICONDIR
            bw.Write((ushort)0);
            bw.Write((ushort)1);
            bw.Write((ushort)count);

            // ICONDIRENTRY for each image
            for (int i = 0; i < count; i++)
            {
                int sz = images[i].size;
                int len = pngBlobs[i].Length;
                byte w = (byte)(sz == 256 ? 0 : sz);
                byte h = (byte)(sz == 256 ? 0 : sz);
                bw.Write(w);
                bw.Write(h);
                bw.Write((byte)0);
                bw.Write((byte)0);
                bw.Write((ushort)0);
                bw.Write((ushort)0);
                bw.Write(len);
                bw.Write(currentOffset);
                currentOffset += len;
            }

            foreach (var blob in pngBlobs)
                bw.Write(blob);
        }
    }
}
