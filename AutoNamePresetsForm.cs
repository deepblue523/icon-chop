namespace IconChop
{
    /// <summary>Manage Auto-name description presets (name + long description).</summary>
    internal sealed class AutoNamePresetsForm : Form
    {
        public const string ManageMenuItem = "<manage presets>";

        private readonly AppSettings _settings;
        private readonly ListBox _list = new();
        private readonly Button _btnAdd = new();
        private readonly Button _btnEdit = new();
        private readonly Button _btnRemove = new();
        private readonly Button _btnClose = new();

        public AutoNamePresetsForm(AppSettings settings)
        {
            _settings = settings;
            Text = "Auto-name description presets";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(440, 320);

            var lbl = new Label
            {
                Text =
                    "Each preset describes the application you are generating icons for. When you use Auto-name (AI), " +
                    "the selected preset's description is sent to the model so suggested filenames fit your app in context.",
                AutoSize = false,
                Location = new Point(12, 10),
                Size = new Size(416, 58),
                ForeColor = Color.FromArgb(80, 80, 80)
            };

            _list.Location = new Point(12, 74);
            _list.Size = new Size(416, 186);
            _list.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _list.IntegralHeight = false;
            _list.DisplayMember = nameof(AutoNameAppDescriptionPreset.Name);

            _btnAdd.Text = "Add…";
            _btnAdd.Location = new Point(12, 270);
            _btnAdd.Size = new Size(88, 26);
            _btnAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            _btnAdd.FlatStyle = FlatStyle.Flat;
            _btnAdd.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnAdd.ImageAlign = ContentAlignment.MiddleLeft;
            _btnAdd.TextAlign = ContentAlignment.MiddleCenter;
            _btnAdd.Padding = new Padding(6, 0, 4, 0);
            _btnAdd.Click += BtnAdd_Click;

            _btnEdit.Text = "Edit…";
            _btnEdit.Location = new Point(108, 270);
            _btnEdit.Size = new Size(88, 26);
            _btnEdit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            _btnEdit.FlatStyle = FlatStyle.Flat;
            _btnEdit.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnEdit.ImageAlign = ContentAlignment.MiddleLeft;
            _btnEdit.TextAlign = ContentAlignment.MiddleCenter;
            _btnEdit.Padding = new Padding(6, 0, 4, 0);
            _btnEdit.Click += BtnEdit_Click;

            _btnRemove.Text = "Remove";
            _btnRemove.Location = new Point(204, 270);
            _btnRemove.Size = new Size(88, 26);
            _btnRemove.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            _btnRemove.FlatStyle = FlatStyle.Flat;
            _btnRemove.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnRemove.ImageAlign = ContentAlignment.MiddleLeft;
            _btnRemove.TextAlign = ContentAlignment.MiddleCenter;
            _btnRemove.Padding = new Padding(6, 0, 4, 0);
            _btnRemove.Click += BtnRemove_Click;

            _btnClose.Text = "Close";
            _btnClose.Location = new Point(340, 270);
            _btnClose.Size = new Size(88, 26);
            _btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btnClose.DialogResult = DialogResult.OK;
            _btnClose.FlatStyle = FlatStyle.Flat;
            _btnClose.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnClose.ImageAlign = ContentAlignment.MiddleLeft;
            _btnClose.TextAlign = ContentAlignment.MiddleCenter;
            _btnClose.Padding = new Padding(6, 0, 4, 0);

            Controls.Add(lbl);
            Controls.Add(_list);
            Controls.Add(_btnAdd);
            Controls.Add(_btnEdit);
            Controls.Add(_btnRemove);
            Controls.Add(_btnClose);

            Load += (_, _) =>
            {
                RefreshList();
                IconButtonImages.Set(_btnAdd, "add-icon_32x32.png", 18);
                IconButtonImages.Set(_btnEdit, "edit-icon_32x32.png", 18);
                IconButtonImages.Set(_btnRemove, "delete-icon_32x32.png", 18);
                IconButtonImages.Set(_btnClose, "cancel-icon_32x32.png", 18);
            };
            FormClosed += (_, _) =>
            {
                IconButtonImages.Clear(_btnAdd);
                IconButtonImages.Clear(_btnEdit);
                IconButtonImages.Clear(_btnRemove);
                IconButtonImages.Clear(_btnClose);
            };
        }

        private void RefreshList()
        {
            _list.Items.Clear();
            foreach (var p in _settings.AutoNameAppDescriptionPresets.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
                _list.Items.Add(p);
            _btnEdit.Enabled = _btnRemove.Enabled = _list.Items.Count > 0;
        }

        private static IReadOnlyList<string> ForbiddenNamesExcept(AppSettings settings, AutoNameAppDescriptionPreset? except)
        {
            return settings.AutoNameAppDescriptionPresets
                .Where(p => except == null || p.Id != except.Id)
                .Select(p => p.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var dlg = new AutoNamePresetEditorForm(
                "New preset",
                initialName: null,
                initialDescription: null,
                ForbiddenNamesExcept(_settings, except: null));
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var preset = new AutoNameAppDescriptionPreset
            {
                Id = Guid.NewGuid(),
                Name = dlg.ResultName,
                Description = dlg.ResultDescription
            };
            _settings.AutoNameAppDescriptionPresets.Add(preset);
            _settings.Save();
            RefreshList();
            SelectPreset(preset);
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (_list.SelectedItem is not AutoNameAppDescriptionPreset preset) return;

            using var dlg = new AutoNamePresetEditorForm(
                "Edit preset",
                preset.Name,
                preset.Description,
                ForbiddenNamesExcept(_settings, preset));
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            preset.Name = dlg.ResultName;
            preset.Description = dlg.ResultDescription;
            _settings.Save();
            RefreshList();
            SelectPreset(preset);
        }

        private void BtnRemove_Click(object? sender, EventArgs e)
        {
            if (_list.SelectedItem is not AutoNameAppDescriptionPreset preset) return;

            if (MessageBox.Show(this,
                    $"Remove preset \"{preset.Name}\"?",
                    Text,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var idStr = preset.Id.ToString();
            _settings.AutoNameAppDescriptionPresets.RemoveAll(p => p.Id == preset.Id);
            _settings.AutoNamePresetMru = _settings.AutoNamePresetMru
                .Where(x => !string.Equals(x, idStr, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (string.Equals(_settings.LastAutoNamePresetId, idStr, StringComparison.OrdinalIgnoreCase))
            {
                _settings.LastAutoNamePresetId = null;
                if (string.Equals(_settings.LastAutoNameAppContext?.Trim(), preset.Name, StringComparison.Ordinal))
                    _settings.LastAutoNameAppContext = null;
            }

            _settings.Save();
            RefreshList();
        }

        private void SelectPreset(AutoNameAppDescriptionPreset preset)
        {
            for (int i = 0; i < _list.Items.Count; i++)
            {
                if (_list.Items[i] is AutoNameAppDescriptionPreset p && p.Id == preset.Id)
                {
                    _list.SelectedIndex = i;
                    return;
                }
            }
        }
    }
}
