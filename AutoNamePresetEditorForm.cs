namespace IconChop
{
    /// <summary>Modal editor for one Auto-name description preset (name + long description).</summary>
    internal sealed class AutoNamePresetEditorForm : Form
    {
        private readonly TextBox _txtName = new();
        private readonly TextBox _txtDescription = new();
        private readonly Button _btnOk = new();
        private readonly Button _btnCancel = new();

        public string ResultName => (_txtName.Text ?? "").Trim();
        public string ResultDescription => (_txtDescription.Text ?? "").Trim();

        public AutoNamePresetEditorForm(
            string title,
            string? initialName,
            string? initialDescription,
            IReadOnlyCollection<string> forbiddenNamesCaseInsensitive)
        {
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(520, 320);
            Padding = new Padding(12);

            var lblName = new Label
            {
                Text = "Name",
                AutoSize = true,
                Location = new Point(12, 12)
            };
            _txtName.Location = new Point(12, 32);
            _txtName.Size = new Size(496, 23);
            _txtName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _txtName.Text = initialName ?? "";

            var lblDesc = new Label
            {
                Text = "Description (sent to the model)",
                AutoSize = true,
                Location = new Point(12, 64)
            };
            _txtDescription.Location = new Point(12, 84);
            _txtDescription.Size = new Size(496, 180);
            _txtDescription.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _txtDescription.Multiline = true;
            _txtDescription.ScrollBars = ScrollBars.Vertical;
            _txtDescription.AcceptsReturn = true;
            _txtDescription.Text = initialDescription ?? "";

            _btnOk.Text = "OK";
            _btnOk.DialogResult = DialogResult.OK;
            _btnOk.Location = new Point(328, 278);
            _btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btnOk.Size = new Size(88, 26);
            _btnOk.FlatStyle = FlatStyle.Flat;
            _btnOk.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnOk.ImageAlign = ContentAlignment.MiddleLeft;
            _btnOk.TextAlign = ContentAlignment.MiddleCenter;
            _btnOk.Padding = new Padding(6, 0, 4, 0);

            _btnCancel.Text = "Cancel";
            _btnCancel.DialogResult = DialogResult.Cancel;
            _btnCancel.Location = new Point(420, 278);
            _btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btnCancel.Size = new Size(88, 26);
            _btnCancel.FlatStyle = FlatStyle.Flat;
            _btnCancel.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnCancel.ImageAlign = ContentAlignment.MiddleLeft;
            _btnCancel.TextAlign = ContentAlignment.MiddleCenter;
            _btnCancel.Padding = new Padding(6, 0, 4, 0);

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;

            Controls.Add(lblName);
            Controls.Add(_txtName);
            Controls.Add(lblDesc);
            Controls.Add(_txtDescription);
            Controls.Add(_btnOk);
            Controls.Add(_btnCancel);

            Shown += (_, _) => _txtName.Focus();

            Load += (_, _) =>
            {
                IconButtonImages.Set(_btnOk, "document-check-icon_32x32.png", 18);
                IconButtonImages.Set(_btnCancel, "cancel-icon_32x32.png", 18);
            };
            FormClosed += (_, _) =>
            {
                IconButtonImages.Clear(_btnOk);
                IconButtonImages.Clear(_btnCancel);
            };

            FormClosing += (_, e) =>
            {
                if (DialogResult != DialogResult.OK) return;
                var name = ResultName;
                if (string.IsNullOrEmpty(name))
                {
                    e.Cancel = true;
                    MessageBox.Show(this, "Please enter a name.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (forbiddenNamesCaseInsensitive.Any(n =>
                        string.Equals(n, name, StringComparison.OrdinalIgnoreCase)))
                {
                    e.Cancel = true;
                    MessageBox.Show(this, "Another preset already uses this name.", Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.Equals(name, "<manage presets>", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, "<no description>", StringComparison.OrdinalIgnoreCase))
                {
                    e.Cancel = true;
                    MessageBox.Show(this, "This name is reserved. Choose a different name.", Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(ResultDescription))
                {
                    e.Cancel = true;
                    MessageBox.Show(this, "Please enter a description.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
        }
    }
}
