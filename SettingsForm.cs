namespace IconChop
{
    public partial class SettingsForm : Form
    {
        private readonly AppSettings _settings;

        public SettingsForm(AppSettings settings)
        {
            _settings = settings;
            InitializeComponent();
            Load += SettingsForm_Load;
            btnSave.Click += BtnSave_Click;
            btnExplorerContext.Click += BtnExplorerContext_Click;
        }

        private void SettingsForm_Load(object? sender, EventArgs e)
        {
            chkAutoReload.Checked = _settings.AutoReloadInput;
            RefreshExplorerButtonText();

            txtApiKey.Text = _settings.OpenAiApiKey ?? "";
            txtBaseUrl.Text = string.IsNullOrWhiteSpace(_settings.OpenAiApiBaseUrl)
                ? "https://api.openai.com/v1"
                : _settings.OpenAiApiBaseUrl.TrimEnd('/');

            SelectOrAdd(cboModel, (_settings.OpenAiImageModel ?? "dall-e-3").Trim());
            SelectOrAdd(cboSize, _settings.OpenAiImageSize ?? "1024x1024");

            var q = (_settings.OpenAiImageQuality ?? "standard").ToLowerInvariant();
            cboQuality.SelectedIndex = q == "hd" ? 1 : 0;
        }

        private void RefreshExplorerButtonText()
        {
            btnExplorerContext.Text = ExplorerContextMenu.IsRegistered()
                ? "Remove from Explorer context menu"
                : "Add to Explorer context menu";
        }

        private void BtnExplorerContext_Click(object? sender, EventArgs e)
        {
            bool ok = ExplorerContextMenu.IsRegistered()
                ? ExplorerContextMenu.Unregister()
                : ExplorerContextMenu.Register();

            if (ok)
            {
                RefreshExplorerButtonText();
                lblExplorerStatus.Text = ExplorerContextMenu.IsRegistered()
                    ? "\"Open with IconChop\" is on image files in File Explorer."
                    : "\"Open with IconChop\" has been removed from image files.";
                lblExplorerStatus.ForeColor = Color.FromArgb(80, 120, 80);
            }
            else
            {
                lblExplorerStatus.Text =
                    "Could not update the context menu. Try running as administrator or check permissions.";
                lblExplorerStatus.ForeColor = Color.FromArgb(180, 80, 80);
                MessageBox.Show(
                    "Could not update the Explorer context menu. Try running as administrator or check permissions.",
                    "Context menu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static void SelectOrAdd(ComboBox cbo, string value)
        {
            for (int i = 0; i < cbo.Items.Count; i++)
            {
                if (string.Equals(cbo.Items[i]?.ToString(), value, StringComparison.OrdinalIgnoreCase))
                {
                    cbo.SelectedIndex = i;
                    return;
                }
            }

            cbo.Items.Add(value);
            cbo.SelectedIndex = cbo.Items.Count - 1;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var model = cboModel.Text.Trim();
            if (string.IsNullOrEmpty(model))
            {
                MessageBox.Show("Please enter or select an image model.", "Settings",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedTab = tabOpenAi;
                DialogResult = DialogResult.None;
                return;
            }

            var baseUrl = txtBaseUrl.Text.Trim().TrimEnd('/');
            if (string.IsNullOrEmpty(baseUrl))
                baseUrl = "https://api.openai.com/v1";

            var size = cboSize.Text.Trim();
            if (string.IsNullOrEmpty(size))
                size = "1024x1024";

            _settings.AutoReloadInput = chkAutoReload.Checked;
            _settings.OpenAiApiKey = string.IsNullOrEmpty(txtApiKey.Text) ? null : txtApiKey.Text;
            _settings.OpenAiApiBaseUrl = baseUrl;
            _settings.OpenAiImageModel = model;
            _settings.OpenAiImageSize = size;
            _settings.OpenAiImageQuality = cboQuality.SelectedIndex == 1 ? "hd" : "standard";

            _settings.Save();
            DialogResult = DialogResult.OK;
        }
    }
}
