using System.Drawing.Drawing2D;

namespace IconChop
{
    public partial class ImageGenerateForm : Form
    {
        private readonly AppSettings _settings;

        private Bitmap? _refBitmap1;
        private Bitmap? _refBitmap2;
        private Bitmap? _previewBitmap;

        private static readonly string[] CannedPrompts =
        [
            "Create icon using style of Image 1. Generate images only, no text.",
            "Create icons using style of the ones in Image 1 and items in Image 2. Generate images only, no text.",
            "Create missing icons using style of the ones in Image 1 and items in Image 2. Generate images only, no text.",
        ];

        private sealed class TemplateComboItem
        {
            public static readonly TemplateComboItem Separator = new() { IsSeparator = true };

            public bool IsSeparator { get; private init; }
            public string DisplayText { get; private init; } = "";
            public string? Prompt { get; private init; }

            public static TemplateComboItem Header(string display) =>
                new() { DisplayText = display, Prompt = null };

            public static TemplateComboItem PromptLine(string text) =>
                new() { DisplayText = text, Prompt = text };

            public override string ToString() => DisplayText;
        }

        /// <summary>Set when the user accepts a preview; caller disposes after applying.</summary>
        public Bitmap? AcceptedImage { get; private set; }

        public ImageGenerateForm(AppSettings settings)
        {
            _settings = settings;
            InitializeComponent();
            Load += ImageGenerateForm_Load;
            FormClosing += ImageGenerateForm_FormClosing;
            cboTemplates.SelectedIndexChanged += CboTemplates_SelectedIndexChanged;
            cboTemplates.MeasureItem += CboTemplates_MeasureItem;
            cboTemplates.DrawItem += CboTemplates_DrawItem;
            btnGenerate.Click += BtnGenerate_Click;
            btnCancel.Click += (_, _) => Close();
            btnAccept.Click += BtnAccept_Click;
            btnReject.Click += BtnReject_Click;
            btnLoad1.Click += (_, _) => LoadRefFromFile(1);
            btnPaste1.Click += (_, _) => PasteRef(1);
            btnClear1.Click += (_, _) => ClearRef(1);
            btnLoad2.Click += (_, _) => LoadRefFromFile(2);
            btnPaste2.Click += (_, _) => PasteRef(2);
            btnClear2.Click += (_, _) => ClearRef(2);
        }

        private void ImageGenerateForm_Load(object? sender, EventArgs e)
        {
            RebuildTemplateCombo();
        }

        private void ImageGenerateForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            DisposeRef(ref _refBitmap1, picRef1);
            DisposeRef(ref _refBitmap2, picRef2);
            _previewBitmap?.Dispose();
            _previewBitmap = null;
        }

        private void RebuildTemplateCombo()
        {
            cboTemplates.SelectedIndexChanged -= CboTemplates_SelectedIndexChanged;
            try
            {
                cboTemplates.Items.Clear();
                cboTemplates.Items.Add(TemplateComboItem.Header("(Choose template or recent…)"));
                foreach (var p in CannedPrompts)
                    cboTemplates.Items.Add(TemplateComboItem.PromptLine(p));

                var mruToShow = _settings.PromptMru
                    .Where(p => !CannedPrompts.Any(c => string.Equals(c, p, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                if (mruToShow.Count > 0)
                {
                    cboTemplates.Items.Add(TemplateComboItem.Separator);
                    foreach (var p in mruToShow)
                        cboTemplates.Items.Add(TemplateComboItem.PromptLine(p));
                }

                cboTemplates.SelectedIndex = 0;
                UpdateTemplateDropDownWidth();
            }
            finally
            {
                cboTemplates.SelectedIndexChanged += CboTemplates_SelectedIndexChanged;
            }
        }

        private void UpdateTemplateDropDownWidth()
        {
            var font = cboTemplates.Font;
            var pad = 24;
            var measureW = Math.Max(cboTemplates.Width - pad, 320);
            var max = cboTemplates.Width;
            foreach (var obj in cboTemplates.Items)
            {
                if (obj is not TemplateComboItem item || item.IsSeparator) continue;
                var sz = TextRenderer.MeasureText(item.DisplayText, font,
                    new Size(measureW, int.MaxValue),
                    TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
                max = Math.Max(max, sz.Width + pad);
            }

            cboTemplates.DropDownWidth = Math.Min(max, 1200);
        }

        private void CboTemplates_MeasureItem(object? sender, MeasureItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= cboTemplates.Items.Count) return;
            if (cboTemplates.Items[e.Index] is not TemplateComboItem item) return;

            if (item.IsSeparator)
            {
                e.ItemHeight = 9;
                return;
            }

            // TextRenderer.MeasureText(..., int.MaxValue height) + TextBoxControl often returns
            // wildly inflated heights for combo rows; MeasureString with a wrap width matches DrawText.
            var wrapWidth = Math.Max(cboTemplates.DropDownWidth - 16, Math.Max(cboTemplates.ClientSize.Width - 8, 200));
            using var g = cboTemplates.CreateGraphics();
            // GenericTypographic is a shared StringFormat; do not dispose.
            var sizeF = g.MeasureString(item.DisplayText, cboTemplates.Font, wrapWidth, StringFormat.GenericTypographic);
            var lineH = (int)Math.Ceiling(cboTemplates.Font.GetHeight());
            var h = (int)Math.Ceiling(sizeF.Height) + 6;
            e.ItemHeight = Math.Max(h, lineH + 4);
        }

        private void CboTemplates_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= cboTemplates.Items.Count) return;
            if (cboTemplates.Items[e.Index] is not TemplateComboItem item) return;

            if (item.IsSeparator)
            {
                e.DrawBackground();
                var y = e.Bounds.Top + e.Bounds.Height / 2;
                using var pen = new Pen(SystemColors.ControlDark, 1f) { DashStyle = DashStyle.Solid };
                e.Graphics.DrawLine(pen, e.Bounds.Left + 6, y, e.Bounds.Right - 6, y);
                return;
            }

            var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var back = selected ? SystemColors.Highlight : SystemColors.Window;
            var fore = selected ? SystemColors.HighlightText : SystemColors.WindowText;
            using var backBrush = new SolidBrush(back);
            e.Graphics.FillRectangle(backBrush, e.Bounds);
            var textRect = new Rectangle(e.Bounds.Left + 4, e.Bounds.Top + 3, e.Bounds.Width - 8, e.Bounds.Height - 6);
            TextRenderer.DrawText(e.Graphics, item.DisplayText, e.Font, textRect, fore,
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus && !selected)
                e.DrawFocusRectangle();
        }

        private void CboTemplates_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cboTemplates.SelectedItem is not TemplateComboItem item) return;

            if (item.IsSeparator)
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

            if (item.Prompt != null)
                txtPrompt.Text = item.Prompt;
        }

        private void LoadRefFromFile(int slot)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.bmp;*.jpg;*.jpeg;*.gif;*.tiff|All Files|*.*",
                Title = slot == 1 ? "Reference image 1" : "Reference image 2"
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

        private void ClearRef(int slot)
        {
            if (slot == 1)
                DisposeRef(ref _refBitmap1, picRef1);
            else
                DisposeRef(ref _refBitmap2, picRef2);
        }

        private static void DisposeRef(ref Bitmap? bmp, PictureBox pic)
        {
            bmp?.Dispose();
            bmp = null;
            if (pic.Image != null)
            {
                pic.Image.Dispose();
                pic.Image = null;
            }
        }

        private void SetRef(int slot, Bitmap bmp)
        {
            if (slot == 1)
            {
                DisposeRef(ref _refBitmap1, picRef1);
                _refBitmap1 = bmp;
                picRef1.Image = bmp;
            }
            else
            {
                DisposeRef(ref _refBitmap2, picRef2);
                _refBitmap2 = bmp;
                picRef2.Image = bmp;
            }
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

            var refs = new List<Bitmap>();
            if (_refBitmap1 != null) refs.Add(_refBitmap1);
            if (_refBitmap2 != null) refs.Add(_refBitmap2);

            btnGenerate.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(6));
                var result = await OpenAiImageClient.GenerateAsync(_settings, prompt, refs, cts.Token)
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
