namespace Freyja
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            grpInputs = new GroupBox();
            tlpInputs = new TableLayoutPanel();
            txtCirculationPath = new TextBox();
            btnBrowseCirculation = new Button();
            lblCirculation = new Label();
            btnBrowseUsers = new Button();
            btnBrowseItems = new Button();
            txtUsersPath = new TextBox();
            txtItemsPath = new TextBox();
            lblUsers = new Label();
            lblItems = new Label();
            folderBrowserDialogOutput = new FolderBrowserDialog();
            grpSettings = new GroupBox();
            clbPatronGroups = new CheckedListBox();
            lblPatronGroups = new Label();
            nudThreshold = new NumericUpDown();
            lblThreshold = new Label();
            grpOutput = new GroupBox();
            btnBrowseOutput = new Button();
            txtOutputFolder = new TextBox();
            lblOutputFolder = new Label();
            pnlActions = new Panel();
            btnExit = new Button();
            btnClear = new Button();
            btnRun = new Button();
            grpLog = new GroupBox();
            txtLog = new TextBox();
            openFileDialogCsv = new OpenFileDialog();
            toolTipMain = new ToolTip(components);
            grpInputs.SuspendLayout();
            tlpInputs.SuspendLayout();
            grpSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudThreshold).BeginInit();
            grpOutput.SuspendLayout();
            pnlActions.SuspendLayout();
            grpLog.SuspendLayout();
            SuspendLayout();
            // 
            // grpInputs
            // 
            grpInputs.Controls.Add(tlpInputs);
            grpInputs.Dock = DockStyle.Top;
            grpInputs.Location = new Point(0, 0);
            grpInputs.Name = "grpInputs";
            grpInputs.Size = new Size(884, 140);
            grpInputs.TabIndex = 0;
            grpInputs.TabStop = false;
            grpInputs.Text = "Input Files";
            // 
            // tlpInputs
            // 
            tlpInputs.AutoSize = true;
            tlpInputs.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpInputs.ColumnCount = 3;
            tlpInputs.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 172F));
            tlpInputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpInputs.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 107F));
            tlpInputs.Controls.Add(txtCirculationPath, 1, 0);
            tlpInputs.Controls.Add(btnBrowseCirculation, 2, 0);
            tlpInputs.Controls.Add(lblCirculation, 0, 0);
            tlpInputs.Controls.Add(btnBrowseUsers, 2, 1);
            tlpInputs.Controls.Add(btnBrowseItems, 2, 2);
            tlpInputs.Controls.Add(txtUsersPath, 1, 1);
            tlpInputs.Controls.Add(txtItemsPath, 1, 2);
            tlpInputs.Controls.Add(lblUsers, 0, 1);
            tlpInputs.Controls.Add(lblItems, 0, 2);
            tlpInputs.Dock = DockStyle.Fill;
            tlpInputs.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            tlpInputs.Location = new Point(3, 19);
            tlpInputs.Name = "tlpInputs";
            tlpInputs.Padding = new Padding(10);
            tlpInputs.RowCount = 3;
            tlpInputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tlpInputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tlpInputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tlpInputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tlpInputs.Size = new Size(878, 118);
            tlpInputs.TabIndex = 0;
            // 
            // txtCirculationPath
            // 
            txtCirculationPath.Dock = DockStyle.Fill;
            txtCirculationPath.Location = new Point(185, 16);
            txtCirculationPath.Margin = new Padding(3, 6, 3, 6);
            txtCirculationPath.Name = "txtCirculationPath";
            txtCirculationPath.ReadOnly = true;
            txtCirculationPath.Size = new Size(573, 23);
            txtCirculationPath.TabIndex = 1;
            // 
            // btnBrowseCirculation
            // 
            btnBrowseCirculation.Dock = DockStyle.Fill;
            btnBrowseCirculation.Location = new Point(764, 14);
            btnBrowseCirculation.Margin = new Padding(3, 4, 3, 4);
            btnBrowseCirculation.Name = "btnBrowseCirculation";
            btnBrowseCirculation.Size = new Size(101, 27);
            btnBrowseCirculation.TabIndex = 2;
            btnBrowseCirculation.Text = "Browse...";
            btnBrowseCirculation.UseVisualStyleBackColor = true;
            btnBrowseCirculation.Click += btnBrowseCirculation_Click;
            // 
            // lblCirculation
            // 
            lblCirculation.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblCirculation.AutoSize = true;
            lblCirculation.Location = new Point(13, 20);
            lblCirculation.Name = "lblCirculation";
            lblCirculation.Size = new Size(166, 15);
            lblCirculation.TabIndex = 0;
            lblCirculation.Text = "Circulation Log CSV:";
            // 
            // btnBrowseUsers
            // 
            btnBrowseUsers.Dock = DockStyle.Fill;
            btnBrowseUsers.Location = new Point(764, 49);
            btnBrowseUsers.Margin = new Padding(3, 4, 3, 4);
            btnBrowseUsers.Name = "btnBrowseUsers";
            btnBrowseUsers.Size = new Size(101, 27);
            btnBrowseUsers.TabIndex = 3;
            btnBrowseUsers.Text = "Browse...";
            btnBrowseUsers.UseVisualStyleBackColor = true;
            btnBrowseUsers.Click += btnBrowseUsers_Click;
            // 
            // btnBrowseItems
            // 
            btnBrowseItems.Dock = DockStyle.Fill;
            btnBrowseItems.Location = new Point(764, 84);
            btnBrowseItems.Margin = new Padding(3, 4, 3, 4);
            btnBrowseItems.Name = "btnBrowseItems";
            btnBrowseItems.Size = new Size(101, 27);
            btnBrowseItems.TabIndex = 4;
            btnBrowseItems.Text = "Browse...";
            btnBrowseItems.UseVisualStyleBackColor = true;
            btnBrowseItems.Click += btnBrowseItems_Click;
            // 
            // txtUsersPath
            // 
            txtUsersPath.Dock = DockStyle.Fill;
            txtUsersPath.Location = new Point(185, 51);
            txtUsersPath.Margin = new Padding(3, 6, 3, 6);
            txtUsersPath.Name = "txtUsersPath";
            txtUsersPath.ReadOnly = true;
            txtUsersPath.Size = new Size(573, 23);
            txtUsersPath.TabIndex = 5;
            // 
            // txtItemsPath
            // 
            txtItemsPath.Dock = DockStyle.Fill;
            txtItemsPath.Location = new Point(185, 86);
            txtItemsPath.Margin = new Padding(3, 6, 3, 6);
            txtItemsPath.Name = "txtItemsPath";
            txtItemsPath.ReadOnly = true;
            txtItemsPath.Size = new Size(573, 23);
            txtItemsPath.TabIndex = 6;
            // 
            // lblUsers
            // 
            lblUsers.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblUsers.AutoSize = true;
            lblUsers.Location = new Point(13, 55);
            lblUsers.Name = "lblUsers";
            lblUsers.Size = new Size(166, 15);
            lblUsers.TabIndex = 7;
            lblUsers.Text = "New Users CSV:";
            // 
            // lblItems
            // 
            lblItems.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblItems.AutoSize = true;
            lblItems.Location = new Point(13, 90);
            lblItems.Name = "lblItems";
            lblItems.Size = new Size(166, 15);
            lblItems.TabIndex = 8;
            lblItems.Text = "Item Matched CSV:";
            // 
            // folderBrowserDialogOutput
            // 
            folderBrowserDialogOutput.Description = "Select output folder";
            // 
            // grpSettings
            // 
            grpSettings.Controls.Add(clbPatronGroups);
            grpSettings.Controls.Add(lblPatronGroups);
            grpSettings.Controls.Add(nudThreshold);
            grpSettings.Controls.Add(lblThreshold);
            grpSettings.Dock = DockStyle.Top;
            grpSettings.Location = new Point(0, 140);
            grpSettings.Margin = new Padding(3, 6, 3, 6);
            grpSettings.Name = "grpSettings";
            grpSettings.Size = new Size(884, 160);
            grpSettings.TabIndex = 1;
            grpSettings.TabStop = false;
            grpSettings.Text = "Settings";
            // 
            // clbPatronGroups
            // 
            clbPatronGroups.AllowDrop = true;
            clbPatronGroups.CheckOnClick = true;
            clbPatronGroups.FormattingEnabled = true;
            clbPatronGroups.Items.AddRange(new object[] { "Undergraduate", "Graduate", "Faculty", "Staff" });
            clbPatronGroups.Location = new Point(186, 45);
            clbPatronGroups.Name = "clbPatronGroups";
            clbPatronGroups.Size = new Size(120, 94);
            clbPatronGroups.TabIndex = 3;
            clbPatronGroups.ItemCheck += clbPatronGroups_ItemCheck;
            // 
            // lblPatronGroups
            // 
            lblPatronGroups.AutoSize = true;
            lblPatronGroups.Location = new Point(3, 45);
            lblPatronGroups.Name = "lblPatronGroups";
            lblPatronGroups.Size = new Size(168, 15);
            lblPatronGroups.TabIndex = 2;
            lblPatronGroups.Text = "Patron groups to auto-forgive:";
            // 
            // nudThreshold
            // 
            nudThreshold.DecimalPlaces = 2;
            nudThreshold.Increment = new decimal(new int[] { 50, 0, 0, 131072 });
            nudThreshold.Location = new Point(186, 15);
            nudThreshold.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudThreshold.Name = "nudThreshold";
            nudThreshold.Size = new Size(120, 23);
            nudThreshold.TabIndex = 1;
            nudThreshold.Value = new decimal(new int[] { 1000, 0, 0, 131072 });
            // 
            // lblThreshold
            // 
            lblThreshold.AutoSize = true;
            lblThreshold.Dock = DockStyle.Fill;
            lblThreshold.Location = new Point(3, 19);
            lblThreshold.Name = "lblThreshold";
            lblThreshold.Size = new Size(154, 15);
            lblThreshold.TabIndex = 0;
            lblThreshold.Text = "Max fine amount to forgive:";
            // 
            // grpOutput
            // 
            grpOutput.Controls.Add(btnBrowseOutput);
            grpOutput.Controls.Add(txtOutputFolder);
            grpOutput.Controls.Add(lblOutputFolder);
            grpOutput.Dock = DockStyle.Top;
            grpOutput.Location = new Point(0, 300);
            grpOutput.Name = "grpOutput";
            grpOutput.Size = new Size(884, 57);
            grpOutput.TabIndex = 2;
            grpOutput.TabStop = false;
            grpOutput.Text = "Output";
            // 
            // btnBrowseOutput
            // 
            btnBrowseOutput.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseOutput.Location = new Point(767, 20);
            btnBrowseOutput.Name = "btnBrowseOutput";
            btnBrowseOutput.Size = new Size(101, 28);
            btnBrowseOutput.TabIndex = 2;
            btnBrowseOutput.Text = "Browse...";
            btnBrowseOutput.UseVisualStyleBackColor = true;
            btnBrowseOutput.Click += btnBrowseOutput_Click;
            // 
            // txtOutputFolder
            // 
            txtOutputFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtOutputFolder.Location = new Point(188, 21);
            txtOutputFolder.Name = "txtOutputFolder";
            txtOutputFolder.ReadOnly = true;
            txtOutputFolder.Size = new Size(573, 23);
            txtOutputFolder.TabIndex = 1;
            // 
            // lblOutputFolder
            // 
            lblOutputFolder.AutoSize = true;
            lblOutputFolder.Location = new Point(6, 27);
            lblOutputFolder.Name = "lblOutputFolder";
            lblOutputFolder.Size = new Size(82, 15);
            lblOutputFolder.TabIndex = 0;
            lblOutputFolder.Text = "Output folder:";
            // 
            // pnlActions
            // 
            pnlActions.Controls.Add(btnExit);
            pnlActions.Controls.Add(btnClear);
            pnlActions.Controls.Add(btnRun);
            pnlActions.Dock = DockStyle.Top;
            pnlActions.Location = new Point(0, 357);
            pnlActions.Name = "pnlActions";
            pnlActions.Size = new Size(884, 55);
            pnlActions.TabIndex = 3;
            // 
            // btnExit
            // 
            btnExit.Location = new Point(530, 17);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(101, 26);
            btnExit.TabIndex = 2;
            btnExit.Text = "Exit";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += btnExit_Click;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(385, 17);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(101, 26);
            btnClear.TabIndex = 1;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // btnRun
            // 
            btnRun.Location = new Point(240, 17);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(101, 26);
            btnRun.TabIndex = 0;
            btnRun.Text = "Run";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // grpLog
            // 
            grpLog.Controls.Add(txtLog);
            grpLog.Dock = DockStyle.Fill;
            grpLog.Location = new Point(0, 412);
            grpLog.Name = "grpLog";
            grpLog.Size = new Size(884, 199);
            grpLog.TabIndex = 4;
            grpLog.TabStop = false;
            grpLog.Text = "Status";
            // 
            // txtLog
            // 
            txtLog.Dock = DockStyle.Fill;
            txtLog.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtLog.Location = new Point(3, 19);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(878, 177);
            txtLog.TabIndex = 0;
            // 
            // openFileDialogCsv
            // 
            openFileDialogCsv.FileName = "openFileDialog1";
            openFileDialogCsv.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialogCsv.Title = "Select CSV file";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(884, 611);
            Controls.Add(grpLog);
            Controls.Add(pnlActions);
            Controls.Add(grpOutput);
            Controls.Add(grpSettings);
            Controls.Add(grpInputs);
            KeyPreview = true;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Freyja";
            Load += MainForm_Load;
            grpInputs.ResumeLayout(false);
            grpInputs.PerformLayout();
            tlpInputs.ResumeLayout(false);
            tlpInputs.PerformLayout();
            grpSettings.ResumeLayout(false);
            grpSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudThreshold).EndInit();
            grpOutput.ResumeLayout(false);
            grpOutput.PerformLayout();
            pnlActions.ResumeLayout(false);
            grpLog.ResumeLayout(false);
            grpLog.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpInputs;
        private FolderBrowserDialog folderBrowserDialogOutput;
        private TableLayoutPanel tlpInputs;
        private Button btnBrowseUsers;
        private Button btnBrowseItems;
        private TextBox txtUsersPath;
        private TextBox txtItemsPath;
        private GroupBox grpSettings;
        private NumericUpDown nudThreshold;
        private Label lblThreshold;
        private CheckedListBox clbPatronGroups;
        private Label lblPatronGroups;
        private GroupBox grpOutput;
        private Button btnBrowseOutput;
        private TextBox txtOutputFolder;
        private Label lblOutputFolder;
        private TextBox txtCirculationPath;
        private Button btnBrowseCirculation;
        private Label lblCirculation;
        private Label lblUsers;
        private Label lblItems;
        private Panel pnlActions;
        private Button btnExit;
        private Button btnClear;
        private Button btnRun;
        private GroupBox grpLog;
        private TextBox txtLog;
        private OpenFileDialog openFileDialogCsv;
        private ToolTip toolTipMain;
    }
}