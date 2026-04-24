using System.Drawing.Drawing2D;

namespace IconChop
{
    public partial class ImageGenerateForm : Form
    {
        private const string MarkupPromptSuffix =
            "See markup on the image for additional context and highlighted areas.";

        private const float MarkupPenWidthPx = 4f;

        private readonly AppSettings _settings;
        private readonly Func<string?>? _getAutoNameAppContextForApi;
        private readonly Func<Bitmap?>? _getMainSourceImage;

        private const int RefImageHistoryMax = 20;

        private Bitmap? _refBase1;
        private Bitmap? _refBase2;
        private Bitmap? _refBitmap1;
        private Bitmap? _refBitmap2;
        private readonly List<Bitmap> _refHistory1 = [];
        private readonly List<Bitmap> _refHistory2 = [];
        private readonly List<MarkupStroke> _markup1 = [];
        private readonly List<MarkupStroke> _markup2 = [];
        private Color _markupDrawColor = Color.FromArgb(220, 60, 60);
        private int _dragSlot;
        private bool _dragActive;
        private Point _dragStartImg;
        private Point _dragCurrentImg;
        private Bitmap? _previewBitmap;
        private bool _applyingIncludeAutoNameCheckFromSettings;

        private readonly struct MarkupStroke(Rectangle bounds, Color color)
        {
            public Rectangle Bounds { get; } = bounds;
            public Color Color { get; } = color;
        }

        private static readonly string[] CannedPrompts =
        [
            "Match the visual style of the image on the Style reference tab. Generate icons as described in the prompt. Generate images only, no text.",
            "Use the Missing icons tab to show gaps or areas to fill, and the Style reference tab for the look to imitate. Generate those missing icons. Generate images only, no text.",
            "Fill missing or highlighted slots on the Missing icons sheet so the result matches the style of the Style reference sheet. Generate images only, no text.",
        ];

        /// <summary>List divider between canned templates and MRU (not applied as a prompt).</summary>
        private const string TemplatesMruSeparator = "──────────────";

        /// <summary>Set when the user accepts a preview; caller disposes after applying.</summary>
        public Bitmap? AcceptedImage { get; private set; }

        public ImageGenerateForm(
            AppSettings settings,
            Func<string?>? getAutoNameAppContextForApi = null,
            Bitmap? initialMissingIconsImageFromSource = null,
            Func<Bitmap?>? getMainSourceImage = null)
        {
            _settings = settings;
            _getAutoNameAppContextForApi = getAutoNameAppContextForApi;
            _getMainSourceImage = getMainSourceImage;
            InitializeComponent();
            if (initialMissingIconsImageFromSource != null)
            {
                SetRef(1, new Bitmap(initialMissingIconsImageFromSource));
                tabRefs.SelectedIndex = 0;
            }

            if (_getMainSourceImage == null)
            {
                btnFromSource1.Visible = false;
                btnFromSource2.Visible = false;
            }

            Load += ImageGenerateForm_Load;
            FormClosing += ImageGenerateForm_FormClosing;
            FormClosed += (_, _) => DisposeToolbarButtonImages();
            cboTemplates.SelectedIndexChanged += CboTemplates_SelectedIndexChanged;
            btnGenerate.Click += BtnGenerate_Click;
            btnCancel.Click += (_, _) => Close();
            btnAccept.Click += BtnAccept_Click;
            btnReject.Click += BtnReject_Click;
            btnLoad1.Click += (_, _) => LoadRefFromFile(1);
            btnFromSource1.Click += (_, _) => LoadRefFromMainSource(1);
            btnHistory1.Click += (_, _) => ShowRefHistoryMenu(1, btnHistory1);
            btnPaste1.Click += (_, _) => PasteRef(1);
            btnClear1.Click += (_, _) => ClearRef(1);
            btnLoad2.Click += (_, _) => LoadRefFromFile(2);
            btnFromSource2.Click += (_, _) => LoadRefFromMainSource(2);
            btnHistory2.Click += (_, _) => ShowRefHistoryMenu(2, btnHistory2);
            btnPaste2.Click += (_, _) => PasteRef(2);
            btnClear2.Click += (_, _) => ClearRef(2);
            chkIncludeAutoNameDescription.CheckedChanged += ChkIncludeAutoNameDescription_CheckedChanged;
            btnMarkupColor.Click += BtnMarkupColor_Click;
            picRef1.MouseDown += PicRef_MouseDown;
            picRef1.MouseMove += PicRef_MouseMove;
            picRef1.MouseUp += PicRef_MouseUp;
            picRef1.PaintOverlay = g => PaintMarkupDragOverlay(g, picRef1, 1);
            picRef2.MouseDown += PicRef_MouseDown;
            picRef2.MouseMove += PicRef_MouseMove;
            picRef2.MouseUp += PicRef_MouseUp;
            picRef2.PaintOverlay = g => PaintMarkupDragOverlay(g, picRef2, 2);
            SyncMarkupColorButton();
            SyncHistoryButtonsEnabled();
        }

        private void ImageGenerateForm_Load(object? sender, EventArgs e)
        {
            _applyingIncludeAutoNameCheckFromSettings = true;
            try
            {
                chkIncludeAutoNameDescription.Checked = _settings.IncludeAutoNameContextInImagePrompt;
            }
            finally
            {
                _applyingIncludeAutoNameCheckFromSettings = false;
            }

            RebuildTemplateCombo();
            ApplyToolbarButtonImages();
        }

        private void ApplyToolbarButtonImages()
        {
            const int r = 18;
            IconButtonImages.Set(btnLoad1, "folder-icon_32x32.png", r);
            IconButtonImages.Set(btnLoad2, "folder-icon_32x32.png", r);
            IconButtonImages.Set(btnFromSource1, "image-preview-icon_32x32.png", r);
            IconButtonImages.Set(btnFromSource2, "image-preview-icon_32x32.png", r);
            IconButtonImages.Set(btnHistory1, "clock-icon_32x32.png", r);
            IconButtonImages.Set(btnHistory2, "clock-icon_32x32.png", r);
            IconButtonImages.Set(btnPaste1, "clipboard-icon_32x32.png", r);
            IconButtonImages.Set(btnPaste2, "clipboard-icon_32x32.png", r);
            IconButtonImages.Set(btnClear1, "delete-icon_32x32.png", r);
            IconButtonImages.Set(btnClear2, "delete-icon_32x32.png", r);
            IconButtonImages.Set(btnGenerate, "performance-metrics-icon_32x32.png", 22);
            IconButtonImages.Set(btnCancel, "cancel-icon_32x32.png", 22);
            IconButtonImages.Set(btnAccept, "document-check-icon_32x32.png", 22);
            IconButtonImages.Set(btnReject, "delete-icon_32x32.png", 22);
        }

        private void DisposeToolbarButtonImages()
        {
            foreach (var b in new[]
                     {
                         btnLoad1, btnLoad2, btnFromSource1, btnFromSource2, btnHistory1, btnHistory2, btnPaste1,
                         btnPaste2, btnClear1, btnClear2, btnGenerate, btnCancel, btnAccept, btnReject
                     })
                IconButtonImages.Clear(b);
        }

        private void ChkIncludeAutoNameDescription_CheckedChanged(object? sender, EventArgs e)
        {
            if (_applyingIncludeAutoNameCheckFromSettings) return;
            _settings.IncludeAutoNameContextInImagePrompt = chkIncludeAutoNameDescription.Checked;
            _settings.Save();
        }

        private void ImageGenerateForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            DisposeRefSlot(1);
            DisposeRefSlot(2);
            DisposeRefHistories();
            _previewBitmap?.Dispose();
            _previewBitmap = null;
        }

        private void RebuildTemplateCombo()
        {
            cboTemplates.SelectedIndexChanged -= CboTemplates_SelectedIndexChanged;
            try
            {
                cboTemplates.Items.Clear();
                cboTemplates.Items.Add("(Choose starter prompt or recent…)");
                foreach (var p in CannedPrompts)
                    cboTemplates.Items.Add(p);

                var mruToShow = _settings.PromptMru
                    .Where(p => !CannedPrompts.Any(c => string.Equals(c, p, StringComparison.OrdinalIgnoreCase)))
                    .Where(p => !string.Equals(p, TemplatesMruSeparator, StringComparison.Ordinal))
                    .ToList();
                if (mruToShow.Count > 0)
                {
                    cboTemplates.Items.Add(TemplatesMruSeparator);
                    foreach (var p in mruToShow)
                        cboTemplates.Items.Add(p);
                }

                cboTemplates.SelectedIndex = 0;
            }
            finally
            {
                cboTemplates.SelectedIndexChanged += CboTemplates_SelectedIndexChanged;
            }
        }

        private void CboTemplates_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cboTemplates.SelectedItem is not string s) return;

            if (s == TemplatesMruSeparator)
            {
                cboTemplates.SelectedIndexChanged -= CboTemplates_SelectedIndexChanged;
                try
                {
                    cboTemplates.SelectedIndex = 0;
                }
                finally
                {
                    cboTemplates.SelectedIndexChanged += CboTemplates_SelectedIndexChanged;
                }

                return;
            }

            if (cboTemplates.SelectedIndex <= 0) return;
            txtPrompt.Text = s;
        }

        private void LoadRefFromFile(int slot)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.bmp;*.jpg;*.jpeg;*.gif;*.tiff|All Files|*.*",
                Title = slot == 1 ? "Missing icons image" : "Style reference image"
            };
            if (ofd.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                using var stream = File.OpenRead(ofd.FileName);
                var bmp = new Bitmap(stream);
                SetRef(slot, bmp);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not load image:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRefFromMainSource(int slot)
        {
            if (_getMainSourceImage == null) return;

            Bitmap? src;
            try
            {
                src = _getMainSourceImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not read the main source image:\n{ex.Message}", "From Source Image",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (src == null)
            {
                MessageBox.Show(
                    "There is no source image on the main window. Load or paste an image there first.",
                    "From Source Image",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                SetRef(slot, new Bitmap(src));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not copy the source image:\n{ex.Message}", "From Source Image",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PasteRef(int slot)
        {
            try
            {
                if (!Clipboard.ContainsImage())
                {
                    MessageBox.Show("The clipboard does not contain an image.", "Paste",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var img = Clipboard.GetImage();
                if (img == null) return;
                var bmp = new Bitmap(img);
                SetRef(slot, bmp);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Paste failed:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearRef(int slot) => DisposeRefSlot(slot);

        private void DisposeRefHistories()
        {
            foreach (var b in _refHistory1)
                b.Dispose();
            _refHistory1.Clear();
            foreach (var b in _refHistory2)
                b.Dispose();
            _refHistory2.Clear();
        }

        private void PushRefHistorySnapshot(int slot)
        {
            var b = slot == 1 ? _refBase1 : _refBase2;
            if (b == null) return;

            var list = slot == 1 ? _refHistory1 : _refHistory2;
            list.Insert(0, new Bitmap(b));
            while (list.Count > RefImageHistoryMax)
            {
                var removed = list[^1];
                list.RemoveAt(list.Count - 1);
                removed.Dispose();
            }

            SyncHistoryButtonsEnabled();
        }

        private void SyncHistoryButtonsEnabled()
        {
            btnHistory1.Enabled = _refHistory1.Count > 0;
            btnHistory2.Enabled = _refHistory2.Count > 0;
        }

        private static Bitmap MakeRefHistoryThumb(Bitmap source, int maxW, int maxH)
        {
            var tw = Math.Max(1, maxW);
            var th = Math.Max(1, maxH);
            var scale = Math.Min(tw / (float)source.Width, th / (float)source.Height);
            var w = Math.Max(1, (int)Math.Round(source.Width * scale));
            var h = Math.Max(1, (int)Math.Round(source.Height * scale));
            var thumb = new Bitmap(tw, th);
            using (var g = Graphics.FromImage(thumb))
            {
                g.Clear(Color.FromArgb(240, 240, 240));
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                var x = (tw - w) / 2f;
                var y = (th - h) / 2f;
                g.DrawImage(source, x, y, w, h);
            }

            return thumb;
        }

        private static void DisposeContextMenuItemImages(ContextMenuStrip menu)
        {
            foreach (ToolStripItem it in menu.Items)
            {
                if (it is ToolStripMenuItem mi && mi.Image != null)
                {
                    mi.Image.Dispose();
                    mi.Image = null;
                }
            }
        }

        private void ShowRefHistoryMenu(int slot, Button anchor)
        {
            var list = slot == 1 ? _refHistory1 : _refHistory2;
            if (list.Count == 0)
            {
                MessageBox.Show(
                    "No images in history for this tab yet. Load, paste, or copy from source to build history (up to 20).",
                    "Image history",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var menu = new ContextMenuStrip();
            for (var i = 0; i < list.Count; i++)
            {
                var entry = list[i];
                var thumb = MakeRefHistoryThumb(entry, 56, 56);
                var label = $"  #{i + 1}  ({entry.Width}×{entry.Height})  ";
                var item = new ToolStripMenuItem(label, thumb) { Tag = i };
                var capturedIndex = i;
                item.Click += (_, _) => ApplyRefFromHistory(slot, capturedIndex);
                menu.Items.Add(item);
            }

            menu.Closed += OnRefHistoryMenuClosed;
            menu.Show(anchor, new Point(0, anchor.Height));
        }

        private static void OnRefHistoryMenuClosed(object? sender, ToolStripDropDownClosedEventArgs e)
        {
            if (sender is not ContextMenuStrip menu) return;
            menu.Closed -= OnRefHistoryMenuClosed;
            DisposeContextMenuItemImages(menu);
            menu.Dispose();
        }

        private void ApplyRefFromHistory(int slot, int historyIndex)
        {
            var list = slot == 1 ? _refHistory1 : _refHistory2;
            if (historyIndex < 0 || historyIndex >= list.Count) return;

            var bmp = list[historyIndex];
            list.RemoveAt(historyIndex);
            SetRef(slot, bmp);
        }

        private void DisposeRefSlot(int slot)
        {
            if (slot == 1)
            {
                _markup1.Clear();
                _refBitmap1?.Dispose();
                _refBitmap1 = null;
                _refBase1?.Dispose();
                _refBase1 = null;
                ClearPicImage(picRef1);
            }
            else
            {
                _markup2.Clear();
                _refBitmap2?.Dispose();
                _refBitmap2 = null;
                _refBase2?.Dispose();
                _refBase2 = null;
                ClearPicImage(picRef2);
            }
        }

        private static void ClearPicImage(PictureBox pic)
        {
            if (pic.Image == null) return;
            pic.Image.Dispose();
            pic.Image = null;
        }

        private void SetRef(int slot, Bitmap bmp)
        {
            if (slot == 1)
            {
                DisposeRefSlot(1);
                _refBase1 = bmp;
                RebuildRefComposite(1);
                PushRefHistorySnapshot(1);
            }
            else
            {
                DisposeRefSlot(2);
                _refBase2 = bmp;
                RebuildRefComposite(2);
                PushRefHistorySnapshot(2);
            }
        }

        private void RebuildRefComposite(int slot)
        {
            var baseBmp = slot == 1 ? _refBase1 : _refBase2;
            var markup = slot == 1 ? _markup1 : _markup2;
            var pic = slot == 1 ? picRef1 : picRef2;

            if (baseBmp == null)
            {
                if (slot == 1)
                {
                    _refBitmap1?.Dispose();
                    _refBitmap1 = null;
                }
                else
                {
                    _refBitmap2?.Dispose();
                    _refBitmap2 = null;
                }

                ClearPicImage(pic);
                return;
            }

            var composite = new Bitmap(baseBmp.Width, baseBmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(composite))
            {
                g.DrawImageUnscaled(baseBmp, 0, 0);
                foreach (var m in markup)
                {
                    using var pen = new Pen(m.Color, MarkupPenWidthPx);
                    g.DrawRectangle(pen, m.Bounds);
                }
            }

            if (slot == 1)
            {
                _refBitmap1?.Dispose();
                _refBitmap1 = composite;
            }
            else
            {
                _refBitmap2?.Dispose();
                _refBitmap2 = composite;
            }

            ClearPicImage(pic);
            pic.Image = composite;
        }

        private static RectangleF GetDisplayedImageRect(PictureBox pic, Size imgSize)
        {
            var cw = pic.ClientSize.Width;
            var ch = pic.ClientSize.Height;
            if (cw <= 0 || ch <= 0 || imgSize.Width <= 0 || imgSize.Height <= 0)
                return RectangleF.Empty;

            var scale = Math.Min(cw / (float)imgSize.Width, ch / (float)imgSize.Height);
            var w = imgSize.Width * scale;
            var h = imgSize.Height * scale;
            var x = (cw - w) / 2f;
            var y = (ch - h) / 2f;
            return new RectangleF(x, y, w, h);
        }

        private static bool TryImagePointFromClient(PictureBox pic, Bitmap img, Point client, out Point imagePoint)
        {
            imagePoint = default;
            var dr = GetDisplayedImageRect(pic, img.Size);
            if (dr.Width <= 0 || dr.Height <= 0) return false;
            if (client.X < dr.Left || client.X > dr.Right || client.Y < dr.Top || client.Y > dr.Bottom)
                return false;

            var ix = (int)Math.Round((client.X - dr.X) / dr.Width * img.Width);
            var iy = (int)Math.Round((client.Y - dr.Y) / dr.Height * img.Height);
            imagePoint = new Point(Math.Clamp(ix, 0, img.Width - 1), Math.Clamp(iy, 0, img.Height - 1));
            return true;
        }

        private static Point ImagePointFromClientClamped(PictureBox pic, Bitmap img, Point client)
        {
            var dr = GetDisplayedImageRect(pic, img.Size);
            if (dr.Width <= 0) return Point.Empty;
            var cx = Math.Clamp(client.X, dr.Left, dr.Right);
            var cy = Math.Clamp(client.Y, dr.Top, dr.Bottom);
            var ix = (int)Math.Round((cx - dr.X) / dr.Width * img.Width);
            var iy = (int)Math.Round((cy - dr.Y) / dr.Height * img.Height);
            return new Point(Math.Clamp(ix, 0, img.Width - 1), Math.Clamp(iy, 0, img.Height - 1));
        }

        private static Point ImagePointToClient(PictureBox pic, Bitmap img, Point imagePoint)
        {
            var dr = GetDisplayedImageRect(pic, img.Size);
            var x = dr.X + imagePoint.X * (dr.Width / img.Width);
            var y = dr.Y + imagePoint.Y * (dr.Height / img.Height);
            return new Point((int)Math.Round(x), (int)Math.Round(y));
        }

        private static Rectangle NormalizeImageRect(Point a, Point b, Size maxSize)
        {
            var x1 = Math.Clamp(Math.Min(a.X, b.X), 0, maxSize.Width - 1);
            var y1 = Math.Clamp(Math.Min(a.Y, b.Y), 0, maxSize.Height - 1);
            var x2 = Math.Clamp(Math.Max(a.X, b.X), 0, maxSize.Width - 1);
            var y2 = Math.Clamp(Math.Max(a.Y, b.Y), 0, maxSize.Height - 1);
            return Rectangle.FromLTRB(x1, y1, x2, y2);
        }

        private void PicRef_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (sender is not ReferenceImagePictureBox pic) return;
            var slot = ReferencePictureSlot(pic);
            var b = slot == 1 ? _refBase1 : _refBase2;
            if (b == null || !TryImagePointFromClient(pic, b, e.Location, out var ip)) return;

            _dragActive = true;
            _dragSlot = slot;
            _dragStartImg = ip;
            _dragCurrentImg = ip;
            pic.Capture = true;
            pic.Invalidate();
        }

        private void PicRef_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_dragActive || sender is not ReferenceImagePictureBox pic) return;
            var slot = ReferencePictureSlot(pic);
            if (slot != _dragSlot) return;
            var b = slot == 1 ? _refBase1 : _refBase2;
            if (b == null) return;

            _dragCurrentImg = ImagePointFromClientClamped(pic, b, e.Location);
            pic.Invalidate();
        }

        private void PicRef_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (sender is not ReferenceImagePictureBox pic) return;
            var slot = ReferencePictureSlot(pic);
            if (!_dragActive || slot != _dragSlot)
            {
                pic.Capture = false;
                return;
            }

            _dragActive = false;
            _dragSlot = 0;
            pic.Capture = false;

            var b = slot == 1 ? _refBase1 : _refBase2;
            if (b != null)
            {
                var r = NormalizeImageRect(_dragStartImg, _dragCurrentImg, b.Size);
                if (r.Width >= 2 && r.Height >= 2)
                {
                    var list = slot == 1 ? _markup1 : _markup2;
                    list.Add(new MarkupStroke(r, _markupDrawColor));
                    RebuildRefComposite(slot);
                }
            }

            pic.Invalidate();
        }

        private int ReferencePictureSlot(ReferenceImagePictureBox pic) => ReferenceEquals(pic, picRef1) ? 1 : 2;

        private void PaintMarkupDragOverlay(Graphics g, ReferenceImagePictureBox pic, int slot)
        {
            if (!_dragActive || _dragSlot != slot) return;
            var b = slot == 1 ? _refBase1 : _refBase2;
            if (b == null) return;

            var rImg = NormalizeImageRect(_dragStartImg, _dragCurrentImg, b.Size);
            if (rImg.Width < 1 || rImg.Height < 1) return;

            var tl = ImagePointToClient(pic, b, new Point(rImg.Left, rImg.Top));
            var br = ImagePointToClient(pic, b, new Point(rImg.Right, rImg.Bottom));
            var rc = Rectangle.FromLTRB(
                Math.Min(tl.X, br.X), Math.Min(tl.Y, br.Y),
                Math.Max(tl.X, br.X), Math.Max(tl.Y, br.Y));
            using var pen = new Pen(Color.FromArgb(220, _markupDrawColor), MarkupPenWidthPx) { DashStyle = DashStyle.Dash };
            g.DrawRectangle(pen, rc);
        }

        private void BtnMarkupColor_Click(object? sender, EventArgs e)
        {
            using var dlg = new ColorDialog { Color = _markupDrawColor, FullOpen = true };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            _markupDrawColor = dlg.Color;
            SyncMarkupColorButton();
        }

        private void SyncMarkupColorButton()
        {
            btnMarkupColor.BackColor = _markupDrawColor;
            var bright = _markupDrawColor.GetBrightness();
            btnMarkupColor.ForeColor = bright > 0.55 ? Color.Black : Color.White;
        }

        private bool HasAnyMarkup() => _markup1.Count > 0 || _markup2.Count > 0;

        private bool UndoLastMarkupForSelectedTab()
        {
            var idx = tabRefs.SelectedIndex;
            if (idx == 0 && _markup1.Count > 0)
            {
                _markup1.RemoveAt(_markup1.Count - 1);
                RebuildRefComposite(1);
                return true;
            }

            if (idx == 1 && _markup2.Count > 0)
            {
                _markup2.RemoveAt(_markup2.Count - 1);
                RebuildRefComposite(2);
                return true;
            }

            return false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Z) && panelGenerate.Visible && UndoLastMarkupForSelectedTab())
                return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private string? ResolveAutoNameAppContextForApi()
        {
            if (_getAutoNameAppContextForApi != null)
            {
                var fromMain = (_getAutoNameAppContextForApi.Invoke() ?? "").Trim();
                return string.IsNullOrEmpty(fromMain) ? null : fromMain;
            }

            var fromSettings = _settings.LastAutoNameAppContext?.Trim();
            return string.IsNullOrEmpty(fromSettings) ? null : fromSettings;
        }

        private async void BtnGenerate_Click(object? sender, EventArgs e)
        {
            var prompt = (txtPrompt.Text ?? "").Trim();
            if (string.IsNullOrEmpty(prompt))
            {
                MessageBox.Show("Enter a prompt describing the image you want.", "Prompt required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var apiPrompt = chkIncludeAutoNameDescription.Checked
                ? OpenAiImageClient.MergeImagePromptWithAutoNameAppContext(prompt, ResolveAutoNameAppContextForApi())
                : prompt;

            if (HasAnyMarkup())
            {
                apiPrompt = string.IsNullOrWhiteSpace(apiPrompt)
                    ? MarkupPromptSuffix
                    : apiPrompt + "\n\n" + MarkupPromptSuffix;
            }

            var refs = new List<Bitmap>();
            if (_refBitmap1 != null) refs.Add(_refBitmap1);
            if (_refBitmap2 != null) refs.Add(_refBitmap2);

            btnGenerate.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(6));
                var result = await OpenAiImageClient.GenerateAsync(_settings, apiPrompt, refs, cts.Token)
                    .ConfigureAwait(true);

                AddPromptToMru(prompt);

                _previewBitmap?.Dispose();
                _previewBitmap = result;
                picPreview.Image = result;
                panelGenerate.Visible = false;
                panelPreview.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "OpenAI image generation",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnGenerate.Enabled = true;
            }
        }

        private void AddPromptToMru(string prompt)
        {
            var t = prompt.Trim();
            if (string.IsNullOrEmpty(t)) return;

            var list = _settings.PromptMru
                .Where(x => !string.Equals(x, t, StringComparison.OrdinalIgnoreCase))
                .Prepend(t)
                .Take(AppSettings.PromptMruMax)
                .ToList();
            _settings.PromptMru = list;
            _settings.Save();
            RebuildTemplateCombo();
        }

        private void BtnAccept_Click(object? sender, EventArgs e)
        {
            if (_previewBitmap == null) return;
            AcceptedImage = new Bitmap(_previewBitmap);
            picPreview.Image = null;
            _previewBitmap.Dispose();
            _previewBitmap = null;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnReject_Click(object? sender, EventArgs e)
        {
            _previewBitmap?.Dispose();
            _previewBitmap = null;
            picPreview.Image = null;
            panelPreview.Visible = false;
            panelGenerate.Visible = true;
        }
    }
}
