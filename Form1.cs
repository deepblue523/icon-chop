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
        /// <summary>Dropdown sentinel: choosing it clears context (no extra prompt text).</summary>
        private const string AutoNameAppContextNoneItem = "<no description>";
        /// <summary>Opens the preset manager; not sent to the model.</summary>
        private const string AutoNameAppContextManageItem = "<manage presets>";
        private bool _namingAppContextComboProgrammatic;
        private Guid? _selectedAutoNamePresetId;
        private string? _namingAppContextSnapshotText;
        private Guid? _namingAppContextSnapshotPresetId;

        private readonly string? _initialFilePath;
        private readonly ContextMenuStrip _sourceHistoryMenu = new();
        private readonly ContextMenuStrip _previewIconContextMenu = new();

        public Form1(string? initialFilePath = null)
        {
            _initialFilePath = initialFilePath;
            InitializeComponent();
            _sourceHistoryMenu.ImageScalingSize = new Size(40, 40);
            _sourceHistoryMenu.ItemClicked += SourceHistoryMenu_ItemClicked;
            _sourceHistoryMenu.Closed += SourceHistoryMenu_Closed;
            var usePreviewAsSource = new ToolStripMenuItem("Use as Source Image");
            usePreviewAsSource.Click += PreviewUseAsSource_Click;
            _previewIconContextMenu.Items.Add(usePreviewAsSource);
            try
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? Icon;
            }
            catch { /* use default if exe icon unavailable */ }
            SetupDragDrop();
            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
            FormClosed += Form1_FormClosed;
        }

        private void Form1_FormClosed(object? sender, FormClosedEventArgs e)
        {
            DisposeMainToolbarButtonImages();
            cboNamingAppContext.SelectedIndexChanged -= CboNamingAppContext_SelectedIndexChanged;
            cboNamingAppContext.DropDown -= CboNamingAppContext_DropDown;
            cboNamingAppContext.TextChanged -= CboNamingAppContext_TextChanged;
            _sourceHistoryMenu.ItemClicked -= SourceHistoryMenu_ItemClicked;
            _sourceHistoryMenu.Closed -= SourceHistoryMenu_Closed;
            DisposeSourceHistoryMenuImages();
            _sourceHistoryMenu.Items.Clear();
            _sourceHistoryMenu.Dispose();
            foreach (ToolStripItem item in _previewIconContextMenu.Items)
                if (item is ToolStripMenuItem mi)
                    mi.Click -= PreviewUseAsSource_Click;
            _previewIconContextMenu.Items.Clear();
            _previewIconContextMenu.Dispose();
        }

        private void ApplyMainToolbarButtonImages()
        {
            IconButtonImages.Set(btnBrowse, "folder-icon_32x32.png", 18);
            IconButtonImages.Set(btnSourcePaste, "clipboard-icon_32x32.png", 24);
            IconButtonImages.Set(btnSourceHistory, "clock-icon_32x32.png", 24);
            IconButtonImages.Set(btnSourceGenerate, "performance-metrics-icon_32x32.png", 24);
            IconButtonImages.Set(btnSourceVary, "refresh-icon_32x32.png", 24);
            IconButtonImages.Set(btnSelectAll, "checkmark-icon_32x32.png", 18);
            IconButtonImages.Set(btnDeselectAll, "cancel-icon_32x32.png", 18);
        }

        private void DisposeMainToolbarButtonImages()
        {
            foreach (var btn in new[]
                     {
                         btnBrowse, btnSourcePaste, btnSourceHistory, btnSourceGenerate, btnSourceVary, btnSelectAll,
                         btnDeselectAll
                     })
                IconButtonImages.Clear(btn);
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            ApplyMainToolbarButtonImages();
            RestoreFormBounds();
            var fmt = _settings.OutputFormat;
            cboOutputFormat.SelectedIndex = fmt == "Ico" ? 1 : fmt == "Both" ? 2 : 0;
            txtOutputPrefix.Text = _settings.OutputPrefix ?? "icon";
            foreach (Control c in panelSizes.Controls)
                if (c is CheckBox cb && cb.Tag is int sz)
                    cb.Checked = _settings.CheckedSizes.Contains(sz);
            chkAutoName.Checked = _settings.AutoNameIcons;
            chkAutoName.CheckedChanged += ChkAutoName_CheckedChanged;
            txtOutputPrefix.Enabled = !chkAutoName.Checked;
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

            RebuildNamingAppContextCombo();
            cboNamingAppContext.SelectedIndexChanged += CboNamingAppContext_SelectedIndexChanged;
            cboNamingAppContext.DropDown += CboNamingAppContext_DropDown;
            cboNamingAppContext.TextChanged += CboNamingAppContext_TextChanged;
            if (!string.IsNullOrWhiteSpace(_settings.LastAutoNamePresetId) &&
                Guid.TryParse(_settings.LastAutoNamePresetId, out var savedPresetGuid) &&
                FindPresetById(savedPresetGuid) is { } restoredPreset)
            {
                _selectedAutoNamePresetId = savedPresetGuid;
                _namingAppContextComboProgrammatic = true;
                try
                {
                    cboNamingAppContext.Text = restoredPreset.Name;
                }
                finally
                {
                    _namingAppContextComboProgrammatic = false;
                }
            }
            else
            {
                var lastCtx = _settings.LastAutoNameAppContext?.Trim();
                if (!string.IsNullOrEmpty(lastCtx))
                    cboNamingAppContext.Text = lastCtx;
            }

            cboNamingAppContext.Enabled = chkAutoName.Checked;
            UpdateOpenAiGenerateUiEnabled();
            RefreshStatusFromDocument();
        }

        private void SetStatus(string text) => statusLabelMain.Text = text;

        private void RefreshStatusFromDocument()
        {
            if (_sourceImage == null)
            {
                SetStatus("Ready.");
                return;
            }

            int n = _detectedIcons.Count;
            SetStatus($"Ready — {n} icon(s) detected, source {_sourceImage.Width}×{_sourceImage.Height}.");
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            StopFileWatcher();
            _settings.OutputFormat = cboOutputFormat.SelectedIndex switch { 1 => "Ico", 2 => "Both", _ => "Png" };
            var pre = (txtOutputPrefix.Text ?? "").Trim();
            _settings.OutputPrefix = string.IsNullOrEmpty(pre) ? "icon" : pre;
            _settings.AutoNameIcons = chkAutoName.Checked;
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
            _settings.InputImageMru = _settings.InputImageMru
                .Where(p => File.Exists(p))
                .Take(AppSettings.InputImageMruMax)
                .ToList();
            var appCtx = (cboNamingAppContext.Text ?? "").Trim();
            if (_selectedAutoNamePresetId is { } pid && FindPresetById(pid) is { } pr &&
                string.Equals(pr.Name, appCtx, StringComparison.Ordinal))
            {
                _settings.LastAutoNamePresetId = pid.ToString("D");
                _settings.LastAutoNameAppContext = pr.Name;
                PrependAutoNamePresetMru(_settings, pid);
            }
            else
            {
                _settings.LastAutoNamePresetId = null;
                _settings.LastAutoNameAppContext = string.IsNullOrEmpty(appCtx) ? null : appCtx;
                PrependAutoNameAppContextMru(appCtx);
            }
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

        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            using var dlg = new SettingsForm(_settings);
            dlg.ShowDialog(this);
            UpdateOpenAiGenerateUiEnabled();
        }

        /// <summary>Enables Generate when the OpenAI image API key is set (same as generation).</summary>
        private void UpdateOpenAiGenerateUiEnabled()
        {
            var configured = !string.IsNullOrWhiteSpace(_settings.OpenAiApiKey);
            btnSourceGenerate.Enabled = configured;
            btnSourceVary.Enabled = configured && _sourceImage != null;
            mnuImageGenerate.Enabled = configured;
        }

        private void OpenImageGenerateDialog(Bitmap? initialMissingIconsImageFromSource = null)
        {
            using var dlg = new ImageGenerateForm(
                _settings,
                GetAutoNameAppContextForApi,
                initialMissingIconsImageFromSource,
                () => _sourceImage);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.AcceptedImage != null)
                ApplyGeneratedImage(dlg.AcceptedImage);
        }

        private void MnuImageGenerate_Click(object? sender, EventArgs e) => OpenImageGenerateDialog();

        private void BtnSourceGenerate_Click(object? sender, EventArgs e) => OpenImageGenerateDialog();

        private void BtnSourceVary_Click(object? sender, EventArgs e)
        {
            if (_sourceImage == null)
            {
                MessageBox.Show("Load or paste a source image first.", "Vary",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            OpenImageGenerateDialog(_sourceImage);
        }

        private void BtnSourcePaste_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!Clipboard.ContainsImage())
                {
                    MessageBox.Show("The clipboard does not contain an image.", "Paste",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var clip = Clipboard.GetImage();
                if (clip is null)
                {
                    MessageBox.Show("The clipboard does not contain an image.", "Paste",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var pasted = new Bitmap(clip);
                StopFileWatcher();
                _sourceImage?.Dispose();
                _sourceImage = pasted;
                picSource.Image = _sourceImage;
                _currentInputPath = null;
                Text = "IconChop — (pasted image)";
                DetectAndPreview();
                UpdateOpenAiGenerateUiEnabled();
            }
            catch (Exception ex)
            {
                SetStatus($"Paste failed: {ex.Message}");
                MessageBox.Show($"Could not paste from clipboard:\n{ex.Message}", "Paste",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyGeneratedImage(Bitmap newImage)
        {
            StopFileWatcher();

            string? backedPath = null;
            try
            {
                Directory.CreateDirectory(AppSettings.TempDirectoryPath);
                var fileName = $"generated-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.png";
                var path = Path.Combine(AppSettings.TempDirectoryPath, fileName);
                newImage.Save(path, ImageFormat.Png);
                backedPath = Path.GetFullPath(path);
            }
            catch
            {
                // Still adopt the bitmap; MRU/path only if backup succeeds.
            }

            _sourceImage?.Dispose();
            _sourceImage = newImage;
            picSource.Image = _sourceImage;

            if (backedPath != null)
            {
                _currentInputPath = backedPath;
                RecordInputImageMru(_settings, backedPath);
                Text = $"IconChop — {Path.GetFileName(backedPath)}";
                if (_settings.AutoReloadInput)
                    StartFileWatcher(backedPath);
            }
            else
            {
                _currentInputPath = null;
                Text = "IconChop — (generated image)";
            }

            DetectAndPreview();
            UpdateOpenAiGenerateUiEnabled();
        }

        private void PreviewUseAsSource_Click(object? sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem mi || mi.Owner is not ContextMenuStrip strip)
                return;
            int idx = strip.SourceControl switch
            {
                Panel p when p.Tag is int i => i,
                PictureBox pb when pb.Parent is Panel par && par.Tag is int j => j,
                _ => -1
            };
            if (idx < 0 || idx >= _detectedIcons.Count) return;
            var clone = new Bitmap(_detectedIcons[idx]);
            StopFileWatcher();
            _sourceImage?.Dispose();
            _sourceImage = clone;
            picSource.Image = _sourceImage;
            _currentInputPath = null;
            Text = "IconChop — (preview icon)";
            DetectAndPreview();
            UpdateOpenAiGenerateUiEnabled();
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

        private void BtnSourceHistory_Click(object? sender, EventArgs e)
        {
            if (_sourceHistoryMenu.Visible)
            {
                _sourceHistoryMenu.Close();
                return;
            }

            DisposeSourceHistoryMenuImages();
            _sourceHistoryMenu.Items.Clear();

            var paths = _settings.InputImageMru.Where(File.Exists).Take(AppSettings.InputImageMruMax).ToList();
            if (paths.Count == 0)
            {
                _sourceHistoryMenu.Items.Add(new ToolStripMenuItem("(No recent images)") { Enabled = false });
            }
            else
            {
                foreach (var path in paths)
                {
                    Image? thumb = null;
                    try
                    {
                        thumb = CreateSourceHistoryThumbnail(path, 40);
                    }
                    catch
                    {
                        // skip thumbnail; item still usable
                    }

                    var item = new ToolStripMenuItem(Path.GetFileName(path))
                    {
                        Image = thumb,
                        Tag = path,
                        ToolTipText = path
                    };
                    _sourceHistoryMenu.Items.Add(item);
                }
            }

            var btn = btnSourceHistory;
            _sourceHistoryMenu.Show(btn, new Point(0, btn.Height));
        }

        private void SourceHistoryMenu_ItemClicked(object? sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem is not ToolStripMenuItem mi || !mi.Enabled)
                return;
            if (mi.Tag is not string path || string.IsNullOrEmpty(path) || !File.Exists(path))
                return;
            // Run after the dropdown finishes closing so it does not fight teardown / event order.
            BeginInvoke(() => LoadImage(path));
        }

        private void SourceHistoryMenu_Closed(object? sender, ToolStripDropDownClosedEventArgs e)
        {
            DisposeSourceHistoryMenuImages();
            _sourceHistoryMenu.Items.Clear();
        }

        private void DisposeSourceHistoryMenuImages()
        {
            foreach (ToolStripItem item in _sourceHistoryMenu.Items)
            {
                if (item is ToolStripMenuItem mi && mi.Image != null)
                {
                    mi.Image.Dispose();
                    mi.Image = null;
                }
            }
        }

        private static void RecordInputImageMru(AppSettings settings, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            string full;
            try
            {
                full = Path.GetFullPath(path);
            }
            catch
            {
                return;
            }

            settings.InputImageMru = settings.InputImageMru
                .Where(p => !string.Equals(p, full, StringComparison.OrdinalIgnoreCase))
                .Prepend(full)
                .Take(AppSettings.InputImageMruMax)
                .ToList();
        }

        private static Image CreateSourceHistoryThumbnail(string path, int boxSize)
        {
            using var stream = File.OpenRead(path);
            using var src = new Bitmap(stream);
            var thumb = new Bitmap(boxSize, boxSize, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(thumb))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.Clear(Color.FromArgb(45, 45, 48));
                var dest = FitInside(src.Width, src.Height, boxSize, boxSize);
                g.DrawImage(src, dest);
            }

            return thumb;
        }

        private static Rectangle FitInside(int srcW, int srcH, int boxW, int boxH)
        {
            if (srcW <= 0 || srcH <= 0) return new Rectangle(0, 0, boxW, boxH);
            float scale = Math.Min((float)boxW / srcW, (float)boxH / srcH);
            int w = Math.Max(1, (int)Math.Round(srcW * scale));
            int h = Math.Max(1, (int)Math.Round(srcH * scale));
            int x = (boxW - w) / 2;
            int y = (boxH - h) / 2;
            return new Rectangle(x, y, w, h);
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
                RecordInputImageMru(_settings, path);
                DetectAndPreview();
                if (_settings.AutoReloadInput && !string.IsNullOrEmpty(path))
                    StartFileWatcher(path);
                UpdateOpenAiGenerateUiEnabled();
            }
            catch (Exception ex)
            {
                SetStatus($"Load failed: {ex.Message}");
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
                panel.ContextMenuStrip = _previewIconContextMenu;
                pb.ContextMenuStrip = _previewIconContextMenu;
                flowPreview.Controls.Add(panel);
            }

            UpdatePreviewCountLabel();
            RefreshStatusFromDocument();
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

        private void ChkAutoName_CheckedChanged(object? sender, EventArgs e)
        {
            txtOutputPrefix.Enabled = !chkAutoName.Checked;
            cboNamingAppContext.Enabled = chkAutoName.Checked;
        }

        private void RebuildNamingAppContextCombo()
        {
            var saved = cboNamingAppContext.Text ?? "";
            var savedPresetId = _selectedAutoNamePresetId;
            _namingAppContextComboProgrammatic = true;
            try
            {
                cboNamingAppContext.Items.Clear();
                cboNamingAppContext.Items.Add(AutoNameAppContextNoneItem);
                cboNamingAppContext.Items.Add(AutoNameAppContextManageItem);
                foreach (var preset in PresetsInDropdownOrder())
                    cboNamingAppContext.Items.Add(preset.Name);

                var presetNames = new HashSet<string>(
                    _settings.AutoNameAppDescriptionPresets.Select(p => (p.Name ?? "").Trim()),
                    StringComparer.OrdinalIgnoreCase);

                foreach (var entry in _settings.AutoNameAppContextMru
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Where(x => !string.Equals(x.Trim(), AutoNameAppContextNoneItem, StringComparison.OrdinalIgnoreCase))
                    .Where(x => !string.Equals(x.Trim(), AutoNameAppContextManageItem, StringComparison.OrdinalIgnoreCase))
                    .Where(x => !presetNames.Contains(x.Trim()))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(AppSettings.AutoNameAppContextMruMax))
                    cboNamingAppContext.Items.Add(entry);
                cboNamingAppContext.Text = saved;
                _selectedAutoNamePresetId = savedPresetId;
                if (_selectedAutoNamePresetId is { } g && FindPresetById(g) is { } pr &&
                    !string.Equals(pr.Name, (cboNamingAppContext.Text ?? "").Trim(), StringComparison.Ordinal))
                    _selectedAutoNamePresetId = null;
            }
            finally
            {
                _namingAppContextComboProgrammatic = false;
            }
        }

        private IEnumerable<AutoNameAppDescriptionPreset> PresetsInDropdownOrder()
        {
            var presets = _settings.AutoNameAppDescriptionPresets;
            var seen = new HashSet<Guid>();
            foreach (var idStr in _settings.AutoNamePresetMru)
            {
                if (!Guid.TryParse(idStr, out var gid)) continue;
                var p = presets.FirstOrDefault(x => x.Id == gid);
                if (p != null && seen.Add(p.Id))
                    yield return p;
            }

            foreach (var p in presets.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
            {
                if (seen.Add(p.Id))
                    yield return p;
            }
        }

        private AutoNameAppDescriptionPreset? FindPresetById(Guid id) =>
            _settings.AutoNameAppDescriptionPresets.FirstOrDefault(p => p.Id == id);

        private void CboNamingAppContext_DropDown(object? sender, EventArgs e)
        {
            _namingAppContextSnapshotText = cboNamingAppContext.Text;
            _namingAppContextSnapshotPresetId = _selectedAutoNamePresetId;
        }

        private void RestoreNamingAppContextFromSnapshot()
        {
            _namingAppContextComboProgrammatic = true;
            try
            {
                _selectedAutoNamePresetId = _namingAppContextSnapshotPresetId;
                cboNamingAppContext.Text = _namingAppContextSnapshotText ?? "";
            }
            finally
            {
                _namingAppContextComboProgrammatic = false;
            }
        }

        private void CboNamingAppContext_TextChanged(object? sender, EventArgs e)
        {
            if (_namingAppContextComboProgrammatic) return;
            if (_selectedAutoNamePresetId is not { } gid || FindPresetById(gid) is not { } pr) return;
            var t = (cboNamingAppContext.Text ?? "").Trim();
            if (!string.Equals(pr.Name, t, StringComparison.Ordinal))
                _selectedAutoNamePresetId = null;
        }

        private void CboNamingAppContext_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_namingAppContextComboProgrammatic) return;
            if (cboNamingAppContext.SelectedIndex < 0) return;
            if (cboNamingAppContext.SelectedItem is not string s) return;

            if (string.Equals(s, AutoNameAppContextManageItem, StringComparison.OrdinalIgnoreCase))
            {
                using (var dlg = new AutoNamePresetsForm(_settings))
                    dlg.ShowDialog(this);
                RebuildNamingAppContextCombo();
                RestoreNamingAppContextFromSnapshot();
                return;
            }

            if (string.Equals(s, AutoNameAppContextNoneItem, StringComparison.OrdinalIgnoreCase))
            {
                _selectedAutoNamePresetId = null;
                _namingAppContextComboProgrammatic = true;
                try
                {
                    cboNamingAppContext.Text = "";
                    cboNamingAppContext.SelectedIndex = -1;
                }
                finally
                {
                    _namingAppContextComboProgrammatic = false;
                }

                return;
            }

            var preset = _settings.AutoNameAppDescriptionPresets.FirstOrDefault(p =>
                string.Equals(p.Name, s, StringComparison.Ordinal));
            if (preset != null)
            {
                _selectedAutoNamePresetId = preset.Id;
                PrependAutoNamePresetMru(_settings, preset.Id);
                _namingAppContextComboProgrammatic = true;
                try
                {
                    cboNamingAppContext.Text = preset.Name;
                }
                finally
                {
                    _namingAppContextComboProgrammatic = false;
                }

                return;
            }

            _selectedAutoNamePresetId = null;
        }

        private static void PrependAutoNameAppContextMru(AppSettings settings, string trimmedContext)
        {
            if (string.IsNullOrEmpty(trimmedContext)) return;
            if (string.Equals(trimmedContext, AutoNameAppContextNoneItem, StringComparison.OrdinalIgnoreCase)) return;
            if (string.Equals(trimmedContext, AutoNameAppContextManageItem, StringComparison.OrdinalIgnoreCase)) return;
            settings.AutoNameAppContextMru = settings.AutoNameAppContextMru
                .Where(x => !string.Equals(x, trimmedContext, StringComparison.OrdinalIgnoreCase))
                .Prepend(trimmedContext)
                .Take(AppSettings.AutoNameAppContextMruMax)
                .ToList();
        }

        private static void PrependAutoNamePresetMru(AppSettings settings, Guid presetId)
        {
            var idStr = presetId.ToString("D");
            settings.AutoNamePresetMru = settings.AutoNamePresetMru
                .Where(x => !string.Equals(x, idStr, StringComparison.OrdinalIgnoreCase))
                .Prepend(idStr)
                .Take(AppSettings.AutoNamePresetMruMax)
                .ToList();
        }

        private void PrependAutoNameAppContextMru(string trimmedContext)
        {
            PrependAutoNameAppContextMru(_settings, trimmedContext);
        }

        private string? GetAutoNameAppContextForApi()
        {
            if (_selectedAutoNamePresetId is { } gid)
            {
                var preset = FindPresetById(gid);
                if (preset != null)
                {
                    var d = (preset.Description ?? "").Trim();
                    return string.IsNullOrEmpty(d) ? null : d;
                }

                _selectedAutoNamePresetId = null;
            }

            var t = (cboNamingAppContext.Text ?? "").Trim();
            return string.IsNullOrEmpty(t) ? null : t;
        }

        // -------------------------------------------------------------------
        //  Chop & export
        // -------------------------------------------------------------------

        private async void BtnChop_Click(object? sender, EventArgs e)
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
                SetStatus("Exporting…");

                string[] autoNames = [];

                if (chkAutoName.Checked)
                {
                    var key = _settings.OpenAiApiKey?.Trim();
                    if (string.IsNullOrEmpty(key))
                    {
                        Cursor = Cursors.Default;
                        RefreshStatusFromDocument();
                        MessageBox.Show(
                            "Auto-name requires an OpenAI API key.\nConfigure it in Tools \u2192 Settings (Open AI tab).",
                            "API key required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    try
                    {
                        var iconsToName = indicesToExport.Select(i => _detectedIcons[i]).ToList();
                        var appCtx = GetAutoNameAppContextForApi();
                        var names = await OpenAiImageClient.SuggestFilenamesAsync(
                            _settings, iconsToName, appCtx, CancellationToken.None);
                        autoNames = [.. names];
                        if (_selectedAutoNamePresetId is { } chopPresetId && FindPresetById(chopPresetId) is { } chopPreset)
                        {
                            var d = (chopPreset.Description ?? "").Trim();
                            if (!string.IsNullOrEmpty(d))
                            {
                                PrependAutoNamePresetMru(_settings, chopPresetId);
                                RebuildNamingAppContextCombo();
                                _namingAppContextComboProgrammatic = true;
                                try
                                {
                                    cboNamingAppContext.Text = chopPreset.Name;
                                }
                                finally
                                {
                                    _namingAppContextComboProgrammatic = false;
                                }
                            }
                        }
                        else if (appCtx != null)
                        {
                            PrependAutoNameAppContextMru(appCtx);
                            RebuildNamingAppContextCombo();
                            cboNamingAppContext.Text = appCtx;
                        }
                    }
                    catch (Exception ex)
                    {
                        Cursor = Cursors.Default;
                        var result = MessageBox.Show(
                            $"Auto-naming failed:\n{ex.Message}\n\nExport with default naming instead?",
                            "Auto-name error", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result != DialogResult.Yes)
                        {
                            RefreshStatusFromDocument();
                            return;
                        }

                        Cursor = Cursors.WaitCursor;
                        SetStatus("Exporting…");
                    }
                }

                Directory.CreateDirectory(outDir);
                int written = 0;
                int exportIndex = 0;

                bool savePng = cboOutputFormat.SelectedIndex is 0 or 2;
                bool saveIco = cboOutputFormat.SelectedIndex is 1 or 2;
                string prefix = GetOutputPrefix();
                bool singleIcon = indicesToExport.Count == 1;

                foreach (int idx in indicesToExport)
                {
                    var iconBitmaps = new List<(int size, Bitmap bmp)>();

                    string baseName;
                    if (exportIndex < autoNames.Length)
                        baseName = autoNames[exportIndex];
                    else if (singleIcon)
                        baseName = prefix;
                    else
                        baseName = $"{prefix}_{exportIndex + 1:D3}";

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
                SetStatus($"Export complete — {written} file(s) written.");
                MessageBox.Show($"Successfully exported {written} icon files.",
                    "Export complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                SetStatus("Export failed.");
                MessageBox.Show($"Export failed:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                RefreshStatusFromDocument();
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
