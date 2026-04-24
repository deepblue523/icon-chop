namespace IconChop
{
    partial class ImageGenerateForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            panelGenerate = new Panel();
            tableLayoutGenerate = new TableLayoutPanel();
            lblIntro = new Label();
            txtPrompt = new TextBox();
            chkIncludeAutoNameDescription = new CheckBox();
            lblTemplates = new Label();
            cboTemplates = new ComboBox();
            flowMarkupTools = new FlowLayoutPanel();
            lblMarkupDrawHint = new Label();
            btnMarkupColor = new Button();
            lblMarkupUndo = new Label();
            tabRefs = new TabControl();
            tabPageRef1 = new TabPage();
            picRef1 = new ReferenceImagePictureBox();
            flowRef1Buttons = new FlowLayoutPanel();
            btnLoad1 = new Button();
            btnFromSource1 = new Button();
            btnHistory1 = new Button();
            btnPaste1 = new Button();
            btnClear1 = new Button();
            tabPageRef2 = new TabPage();
            picRef2 = new ReferenceImagePictureBox();
            flowRef2Buttons = new FlowLayoutPanel();
            btnLoad2 = new Button();
            btnFromSource2 = new Button();
            btnHistory2 = new Button();
            btnPaste2 = new Button();
            btnClear2 = new Button();
            panelGenButtons = new FlowLayoutPanel();
            btnGenerate = new Button();
            btnCancel = new Button();
            panelPreview = new Panel();
            tableLayoutPreview = new TableLayoutPanel();
            lblPreviewTitle = new Label();
            picPreview = new PictureBox();
            flowPreviewButtons = new FlowLayoutPanel();
            btnAccept = new Button();
            btnReject = new Button();
            panelGenerate.SuspendLayout();
            tableLayoutGenerate.SuspendLayout();
            flowMarkupTools.SuspendLayout();
            tabRefs.SuspendLayout();
            tabPageRef1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picRef1).BeginInit();
            flowRef1Buttons.SuspendLayout();
            tabPageRef2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picRef2).BeginInit();
            flowRef2Buttons.SuspendLayout();
            panelGenButtons.SuspendLayout();
            panelPreview.SuspendLayout();
            tableLayoutPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            flowPreviewButtons.SuspendLayout();
            SuspendLayout();

            // panelGenerate
            panelGenerate.Controls.Add(tableLayoutGenerate);
            panelGenerate.Dock = DockStyle.Fill;
            panelGenerate.Padding = new Padding(12);

            // tableLayoutGenerate
            tableLayoutGenerate.ColumnCount = 1;
            tableLayoutGenerate.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutGenerate.RowCount = 8;
            tableLayoutGenerate.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
            tableLayoutGenerate.RowStyles.Add(new RowStyle(SizeType.Absolute, 84F));
            tableLayoutGenerate.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tableLayoutGenerate.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tableLayoutGenerate.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tableLayoutGenerate.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tableLayoutGenerate.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutGenerate.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            tableLayoutGenerate.Dock = DockStyle.Fill;
            tableLayoutGenerate.Controls.Add(lblIntro, 0, 0);
            tableLayoutGenerate.Controls.Add(txtPrompt, 0, 1);
            tableLayoutGenerate.Controls.Add(chkIncludeAutoNameDescription, 0, 2);
            tableLayoutGenerate.Controls.Add(lblTemplates, 0, 3);
            tableLayoutGenerate.Controls.Add(cboTemplates, 0, 4);
            tableLayoutGenerate.Controls.Add(flowMarkupTools, 0, 5);
            tableLayoutGenerate.Controls.Add(tabRefs, 0, 6);
            tableLayoutGenerate.Controls.Add(panelGenButtons, 0, 7);

            // lblIntro
            lblIntro.AutoSize = false;
            lblIntro.Dock = DockStyle.Fill;
            lblIntro.Margin = new Padding(0, 0, 0, 8);
            lblIntro.Text =
                "This feature generates a new image using OpenAI from your prompt below. " +
                "The Missing icons tab (optional) can show your sheet with gaps or areas to fill; " +
                "the Style reference tab (optional) can show an image whose look the model should imitate. " +
                "Loaded tab images are sent to the model in that order when present. " +
                "Drag on a loaded image to draw highlight rectangles. " +
                "Configure your API key and model under Tools → Settings.";
            lblIntro.ForeColor = Color.FromArgb(80, 80, 80);
            lblIntro.TextAlign = ContentAlignment.TopLeft;

            // txtPrompt
            txtPrompt.Dock = DockStyle.Fill;
            txtPrompt.Margin = new Padding(0, 0, 0, 8);
            txtPrompt.Multiline = true;
            txtPrompt.ScrollBars = ScrollBars.Vertical;
            txtPrompt.TabIndex = 1;

            // chkIncludeAutoNameDescription
            chkIncludeAutoNameDescription.AutoSize = true;
            chkIncludeAutoNameDescription.Dock = DockStyle.Top;
            chkIncludeAutoNameDescription.Margin = new Padding(0, 0, 0, 8);
            chkIncludeAutoNameDescription.Text = "Automatically include the Auto-name description in the prompt";
            chkIncludeAutoNameDescription.ForeColor = Color.FromArgb(80, 80, 80);
            chkIncludeAutoNameDescription.Cursor = Cursors.Hand;
            chkIncludeAutoNameDescription.TabIndex = 2;

            // lblTemplates
            lblTemplates.AutoSize = true;
            lblTemplates.Dock = DockStyle.Top;
            lblTemplates.Margin = new Padding(0, 0, 0, 4);
            lblTemplates.Text = "Starter prompts and recent history:";

            // cboTemplates
            cboTemplates.Dock = DockStyle.Fill;
            cboTemplates.DropDownHeight = 480;
            cboTemplates.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTemplates.Margin = new Padding(0, 0, 0, 8);
            cboTemplates.TabIndex = 3;

            // flowMarkupTools
            flowMarkupTools.AutoSize = true;
            flowMarkupTools.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowMarkupTools.Dock = DockStyle.Fill;
            flowMarkupTools.Margin = new Padding(0, 0, 0, 8);
            flowMarkupTools.WrapContents = false;
            flowMarkupTools.Controls.Add(lblMarkupDrawHint);
            flowMarkupTools.Controls.Add(btnMarkupColor);
            flowMarkupTools.Controls.Add(lblMarkupUndo);

            lblMarkupDrawHint.AutoSize = true;
            lblMarkupDrawHint.Margin = new Padding(0, 6, 12, 6);
            lblMarkupDrawHint.Text = "Markup color:";
            lblMarkupDrawHint.ForeColor = Color.FromArgb(80, 80, 80);

            btnMarkupColor.FlatStyle = FlatStyle.Flat;
            btnMarkupColor.Size = new Size(44, 24);
            btnMarkupColor.Margin = new Padding(0, 4, 16, 4);
            btnMarkupColor.Cursor = Cursors.Hand;
            btnMarkupColor.TabIndex = 4;
            btnMarkupColor.UseVisualStyleBackColor = false;

            lblMarkupUndo.AutoSize = true;
            lblMarkupUndo.Margin = new Padding(0, 6, 0, 6);
            lblMarkupUndo.Text = "Ctrl+Z: undo last rectangle on the selected tab";
            lblMarkupUndo.ForeColor = Color.FromArgb(80, 80, 80);

            // tabRefs
            tabRefs.Dock = DockStyle.Fill;
            tabRefs.Margin = new Padding(0);
            tabRefs.TabIndex = 5;
            tabRefs.Controls.Add(tabPageRef1);
            tabRefs.Controls.Add(tabPageRef2);

            // tabPageRef1
            tabPageRef1.Text = "Missing icons";
            tabPageRef1.Padding = new Padding(8);
            tabPageRef1.Controls.Add(flowRef1Buttons);
            tabPageRef1.Controls.Add(picRef1);

            // picRef1
            picRef1.Dock = DockStyle.Fill;
            picRef1.BackColor = Color.FromArgb(240, 240, 240);
            picRef1.BorderStyle = BorderStyle.FixedSingle;
            picRef1.SizeMode = PictureBoxSizeMode.Zoom;
            picRef1.TabStop = false;
            picRef1.Cursor = Cursors.Cross;

            // flowRef1Buttons
            flowRef1Buttons.Dock = DockStyle.Bottom;
            flowRef1Buttons.Height = 44;
            flowRef1Buttons.Padding = new Padding(0, 8, 0, 0);
            flowRef1Buttons.WrapContents = false;
            flowRef1Buttons.Controls.Add(btnLoad1);
            flowRef1Buttons.Controls.Add(btnFromSource1);
            flowRef1Buttons.Controls.Add(btnHistory1);
            flowRef1Buttons.Controls.Add(btnPaste1);
            flowRef1Buttons.Controls.Add(btnClear1);

            btnLoad1.Text = "Load from file…";
            btnLoad1.AutoSize = true;
            btnLoad1.FlatStyle = FlatStyle.Flat;
            btnLoad1.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnLoad1.ImageAlign = ContentAlignment.MiddleLeft;
            btnLoad1.TextAlign = ContentAlignment.MiddleCenter;
            btnLoad1.Padding = new Padding(6, 0, 6, 0);
            btnLoad1.Margin = new Padding(0, 0, 8, 0);

            btnFromSource1.Text = "From Source Image";
            btnFromSource1.AutoSize = true;
            btnFromSource1.FlatStyle = FlatStyle.Flat;
            btnFromSource1.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnFromSource1.ImageAlign = ContentAlignment.MiddleLeft;
            btnFromSource1.TextAlign = ContentAlignment.MiddleCenter;
            btnFromSource1.Padding = new Padding(6, 0, 6, 0);
            btnFromSource1.Margin = new Padding(0, 0, 8, 0);

            btnHistory1.Text = "History";
            btnHistory1.AutoSize = true;
            btnHistory1.FlatStyle = FlatStyle.Flat;
            btnHistory1.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnHistory1.ImageAlign = ContentAlignment.MiddleLeft;
            btnHistory1.TextAlign = ContentAlignment.MiddleCenter;
            btnHistory1.Padding = new Padding(6, 0, 6, 0);
            btnHistory1.Margin = new Padding(0, 0, 8, 0);

            btnPaste1.Text = "Paste";
            btnPaste1.AutoSize = true;
            btnPaste1.FlatStyle = FlatStyle.Flat;
            btnPaste1.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnPaste1.ImageAlign = ContentAlignment.MiddleLeft;
            btnPaste1.TextAlign = ContentAlignment.MiddleCenter;
            btnPaste1.Padding = new Padding(6, 0, 6, 0);
            btnPaste1.Margin = new Padding(0, 0, 8, 0);

            btnClear1.Text = "Clear";
            btnClear1.AutoSize = true;
            btnClear1.FlatStyle = FlatStyle.Flat;
            btnClear1.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnClear1.ImageAlign = ContentAlignment.MiddleLeft;
            btnClear1.TextAlign = ContentAlignment.MiddleCenter;
            btnClear1.Padding = new Padding(6, 0, 6, 0);
            btnClear1.Margin = new Padding(0);

            // tabPageRef2
            tabPageRef2.Text = "Style reference";
            tabPageRef2.Padding = new Padding(8);
            tabPageRef2.Controls.Add(flowRef2Buttons);
            tabPageRef2.Controls.Add(picRef2);

            picRef2.Dock = DockStyle.Fill;
            picRef2.BackColor = Color.FromArgb(240, 240, 240);
            picRef2.BorderStyle = BorderStyle.FixedSingle;
            picRef2.SizeMode = PictureBoxSizeMode.Zoom;
            picRef2.TabStop = false;
            picRef2.Cursor = Cursors.Cross;

            flowRef2Buttons.Dock = DockStyle.Bottom;
            flowRef2Buttons.Height = 44;
            flowRef2Buttons.Padding = new Padding(0, 8, 0, 0);
            flowRef2Buttons.WrapContents = false;
            flowRef2Buttons.Controls.Add(btnLoad2);
            flowRef2Buttons.Controls.Add(btnFromSource2);
            flowRef2Buttons.Controls.Add(btnHistory2);
            flowRef2Buttons.Controls.Add(btnPaste2);
            flowRef2Buttons.Controls.Add(btnClear2);

            btnLoad2.Text = "Load from file…";
            btnLoad2.AutoSize = true;
            btnLoad2.FlatStyle = FlatStyle.Flat;
            btnLoad2.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnLoad2.ImageAlign = ContentAlignment.MiddleLeft;
            btnLoad2.TextAlign = ContentAlignment.MiddleCenter;
            btnLoad2.Padding = new Padding(6, 0, 6, 0);
            btnLoad2.Margin = new Padding(0, 0, 8, 0);

            btnFromSource2.Text = "From Source Image";
            btnFromSource2.AutoSize = true;
            btnFromSource2.FlatStyle = FlatStyle.Flat;
            btnFromSource2.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnFromSource2.ImageAlign = ContentAlignment.MiddleLeft;
            btnFromSource2.TextAlign = ContentAlignment.MiddleCenter;
            btnFromSource2.Padding = new Padding(6, 0, 6, 0);
            btnFromSource2.Margin = new Padding(0, 0, 8, 0);

            btnHistory2.Text = "History";
            btnHistory2.AutoSize = true;
            btnHistory2.FlatStyle = FlatStyle.Flat;
            btnHistory2.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnHistory2.ImageAlign = ContentAlignment.MiddleLeft;
            btnHistory2.TextAlign = ContentAlignment.MiddleCenter;
            btnHistory2.Padding = new Padding(6, 0, 6, 0);
            btnHistory2.Margin = new Padding(0, 0, 8, 0);

            btnPaste2.Text = "Paste";
            btnPaste2.AutoSize = true;
            btnPaste2.FlatStyle = FlatStyle.Flat;
            btnPaste2.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnPaste2.ImageAlign = ContentAlignment.MiddleLeft;
            btnPaste2.TextAlign = ContentAlignment.MiddleCenter;
            btnPaste2.Padding = new Padding(6, 0, 6, 0);
            btnPaste2.Margin = new Padding(0, 0, 8, 0);

            btnClear2.Text = "Clear";
            btnClear2.AutoSize = true;
            btnClear2.FlatStyle = FlatStyle.Flat;
            btnClear2.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnClear2.ImageAlign = ContentAlignment.MiddleLeft;
            btnClear2.TextAlign = ContentAlignment.MiddleCenter;
            btnClear2.Padding = new Padding(6, 0, 6, 0);
            btnClear2.Margin = new Padding(0);

            // panelGenButtons
            panelGenButtons.Dock = DockStyle.Fill;
            panelGenButtons.FlowDirection = FlowDirection.RightToLeft;
            panelGenButtons.WrapContents = false;
            panelGenButtons.Padding = new Padding(0, 8, 0, 0);
            panelGenButtons.Controls.Add(btnCancel);
            panelGenButtons.Controls.Add(btnGenerate);

            btnGenerate.Text = "Generate";
            btnGenerate.AutoSize = false;
            btnGenerate.Size = new Size(120, 30);
            btnGenerate.FlatStyle = FlatStyle.Flat;
            btnGenerate.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnGenerate.ImageAlign = ContentAlignment.MiddleLeft;
            btnGenerate.TextAlign = ContentAlignment.MiddleCenter;
            btnGenerate.Padding = new Padding(8, 0, 4, 0);
            btnGenerate.BackColor = Color.FromArgb(46, 160, 67);
            btnGenerate.ForeColor = Color.White;
            btnGenerate.UseVisualStyleBackColor = false;
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Margin = new Padding(12, 0, 0, 0);
            btnGenerate.TabIndex = 20;

            btnCancel.Text = "Close";
            btnCancel.AutoSize = false;
            btnCancel.Size = new Size(90, 30);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnCancel.ImageAlign = ContentAlignment.MiddleLeft;
            btnCancel.TextAlign = ContentAlignment.MiddleCenter;
            btnCancel.Padding = new Padding(8, 0, 4, 0);
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnCancel.Margin = new Padding(0);
            btnCancel.TabIndex = 21;

            // panelPreview
            panelPreview.Controls.Add(tableLayoutPreview);
            panelPreview.Dock = DockStyle.Fill;
            panelPreview.Visible = false;
            panelPreview.Padding = new Padding(12);

            // tableLayoutPreview
            tableLayoutPreview.ColumnCount = 1;
            tableLayoutPreview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPreview.RowCount = 3;
            tableLayoutPreview.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            tableLayoutPreview.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPreview.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            tableLayoutPreview.Dock = DockStyle.Fill;
            tableLayoutPreview.Controls.Add(lblPreviewTitle, 0, 0);
            tableLayoutPreview.Controls.Add(picPreview, 0, 1);
            tableLayoutPreview.Controls.Add(flowPreviewButtons, 0, 2);

            lblPreviewTitle.AutoSize = false;
            lblPreviewTitle.Dock = DockStyle.Fill;
            lblPreviewTitle.Margin = new Padding(0, 0, 0, 8);
            lblPreviewTitle.Text =
                "Preview — accept to replace the source image on the main window, or reject to adjust your prompt.";
            lblPreviewTitle.TextAlign = ContentAlignment.TopLeft;
            lblPreviewTitle.ForeColor = Color.FromArgb(60, 60, 60);

            picPreview.Dock = DockStyle.Fill;
            picPreview.BackColor = Color.FromArgb(32, 32, 32);
            picPreview.SizeMode = PictureBoxSizeMode.Zoom;
            picPreview.TabStop = false;
            picPreview.Margin = new Padding(0);

            flowPreviewButtons.Dock = DockStyle.Fill;
            flowPreviewButtons.FlowDirection = FlowDirection.RightToLeft;
            flowPreviewButtons.WrapContents = false;
            flowPreviewButtons.Padding = new Padding(0, 8, 0, 0);
            flowPreviewButtons.Controls.Add(btnReject);
            flowPreviewButtons.Controls.Add(btnAccept);

            btnAccept.Text = "Accept";
            btnAccept.Size = new Size(100, 32);
            btnAccept.FlatStyle = FlatStyle.Flat;
            btnAccept.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnAccept.ImageAlign = ContentAlignment.MiddleLeft;
            btnAccept.TextAlign = ContentAlignment.MiddleCenter;
            btnAccept.Padding = new Padding(8, 0, 4, 0);
            btnAccept.BackColor = Color.FromArgb(46, 160, 67);
            btnAccept.ForeColor = Color.White;
            btnAccept.FlatAppearance.BorderSize = 0;
            btnAccept.Margin = new Padding(12, 0, 0, 0);

            btnReject.Text = "Reject";
            btnReject.Size = new Size(100, 32);
            btnReject.FlatStyle = FlatStyle.Flat;
            btnReject.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnReject.ImageAlign = ContentAlignment.MiddleLeft;
            btnReject.TextAlign = ContentAlignment.MiddleCenter;
            btnReject.Padding = new Padding(8, 0, 4, 0);
            btnReject.BackColor = Color.FromArgb(230, 230, 230);
            btnReject.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);

            // ImageGenerateForm
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(760, 700);
            Controls.Add(panelPreview);
            Controls.Add(panelGenerate);
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(680, 620);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Generate image";
            CancelButton = btnCancel;
            KeyPreview = true;

            flowMarkupTools.ResumeLayout(false);
            flowMarkupTools.PerformLayout();
            tabPageRef1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picRef1).EndInit();
            flowRef1Buttons.ResumeLayout(false);
            tabPageRef2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picRef2).EndInit();
            flowRef2Buttons.ResumeLayout(false);
            tabRefs.ResumeLayout(false);
            panelGenButtons.ResumeLayout(false);
            tableLayoutGenerate.ResumeLayout(false);
            tableLayoutGenerate.PerformLayout();
            panelGenerate.ResumeLayout(false);
            flowPreviewButtons.ResumeLayout(false);
            tableLayoutPreview.ResumeLayout(false);
            tableLayoutPreview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            panelPreview.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelGenerate;
        private TableLayoutPanel tableLayoutGenerate;
        private Label lblIntro;
        private TextBox txtPrompt;
        private CheckBox chkIncludeAutoNameDescription;
        private Label lblTemplates;
        private ComboBox cboTemplates;
        private FlowLayoutPanel flowMarkupTools;
        private Label lblMarkupDrawHint;
        private Button btnMarkupColor;
        private Label lblMarkupUndo;
        private TabControl tabRefs;
        private TabPage tabPageRef1;
        private ReferenceImagePictureBox picRef1;
        private FlowLayoutPanel flowRef1Buttons;
        private Button btnLoad1;
        private Button btnFromSource1;
        private Button btnHistory1;
        private Button btnPaste1;
        private Button btnClear1;
        private TabPage tabPageRef2;
        private ReferenceImagePictureBox picRef2;
        private FlowLayoutPanel flowRef2Buttons;
        private Button btnLoad2;
        private Button btnFromSource2;
        private Button btnHistory2;
        private Button btnPaste2;
        private Button btnClear2;
        private FlowLayoutPanel panelGenButtons;
        private Button btnGenerate;
        private Button btnCancel;
        private Panel panelPreview;
        private TableLayoutPanel tableLayoutPreview;
        private Label lblPreviewTitle;
        private PictureBox picPreview;
        private FlowLayoutPanel flowPreviewButtons;
        private Button btnAccept;
        private Button btnReject;
    }
}
