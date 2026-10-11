namespace KNote.ClientWin.Views
{
    partial class KntScriptConsoleForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KntScriptConsoleForm));
            toolStripConsole = new ToolStrip();
            buttonNew = new ToolStripButton();
            buttonOpen = new ToolStripButton();
            buttonSave = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            buttonInsertCode = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            labelLanguage = new ToolStripLabel();
            comboLanguage = new ToolStripComboBox();
            buttonRun = new ToolStripDropDownButton();
            buttonRunIntegrated = new ToolStripMenuItem();
            buttonRunStdOut = new ToolStripMenuItem();
            statusStripKntConsole = new StatusStrip();
            statusAction = new ToolStripStatusLabel();
            statusFileName = new ToolStripStatusLabel();
            panel1 = new Panel();
            splitContainer1 = new SplitContainer();
            textSourceCode = new TextBox();
            panelInput = new TableLayoutPanel();
            textInput = new TextBox();
            buttonCloseInput = new Button();
            labelInput = new Label();
            openFileDialogScript = new OpenFileDialog();
            saveFileDialogScript = new SaveFileDialog();
            toolStripConsole.SuspendLayout();
            statusStripKntConsole.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panelInput.SuspendLayout();
            SuspendLayout();
            // 
            // toolStripConsole
            // 
            toolStripConsole.Items.AddRange(new ToolStripItem[] { buttonNew, buttonOpen, buttonSave, toolStripSeparator1, buttonInsertCode, toolStripSeparator2, labelLanguage, comboLanguage, buttonRun });
            toolStripConsole.Location = new Point(0, 0);
            toolStripConsole.Name = "toolStripConsole";
            toolStripConsole.Size = new Size(1111, 25);
            toolStripConsole.TabIndex = 9;
            toolStripConsole.Text = "KntSctipt console";
            // 
            // buttonNew
            // 
            buttonNew.DisplayStyle = ToolStripItemDisplayStyle.Image;
            buttonNew.Name = "buttonNew";
            buttonNew.Size = new Size(23, 22);
            buttonNew.Text = "New";
            buttonNew.Click += buttonNew_Click;
            // 
            // buttonOpen
            // 
            buttonOpen.DisplayStyle = ToolStripItemDisplayStyle.Image;
            buttonOpen.Name = "buttonOpen";
            buttonOpen.Size = new Size(23, 22);
            buttonOpen.Text = "Open";
            buttonOpen.Click += buttonOpen_Click;
            // 
            // buttonSave
            // 
            buttonSave.DisplayStyle = ToolStripItemDisplayStyle.Image;
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(23, 22);
            buttonSave.Text = "Save";
            buttonSave.Click += buttonSave_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // buttonInsertCode
            // 
            buttonInsertCode.DisplayStyle = ToolStripItemDisplayStyle.Image;
            buttonInsertCode.Name = "buttonInsertCode";
            buttonInsertCode.Size = new Size(23, 22);
            buttonInsertCode.Text = "Insert code snippet ...";
            buttonInsertCode.Click += buttonInsertCode_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // labelLanguage
            // 
            labelLanguage.Name = "labelLanguage";
            labelLanguage.Size = new Size(62, 22);
            labelLanguage.Text = "Language:";
            // 
            // comboLanguage
            // 
            comboLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            comboLanguage.Name = "comboLanguage";
            comboLanguage.Size = new Size(150, 25);
            comboLanguage.SelectedIndexChanged += comboLanguage_SelectedIndexChanged;
            // 
            // buttonRun
            // 
            buttonRun.DropDownItems.AddRange(new ToolStripItem[] { buttonRunIntegrated, buttonRunStdOut });
            buttonRun.Name = "buttonRun";
            buttonRun.Size = new Size(57, 22);
            buttonRun.Text = "Run";
            // 
            // buttonRunIntegrated
            // 
            buttonRunIntegrated.Name = "buttonRunIntegrated";
            buttonRunIntegrated.ShortcutKeyDisplayString = "F5";
            buttonRunIntegrated.Size = new Size(320, 22);
            buttonRunIntegrated.Text = "Run in integrated output console";
            buttonRunIntegrated.Click += buttonRunIntegrated_Click;
            // 
            // buttonRunStdOut
            // 
            buttonRunStdOut.Name = "buttonRunStdOut";
            buttonRunStdOut.ShortcutKeyDisplayString = "Shift+F5";
            buttonRunStdOut.Size = new Size(320, 22);
            buttonRunStdOut.Text = "Run in standard output (stdout)";
            buttonRunStdOut.Click += buttonRunStdOut_Click;
            //
            // statusStripKntConsole
            // 
            statusStripKntConsole.Items.AddRange(new ToolStripItem[] { statusAction, statusFileName });
            statusStripKntConsole.Location = new Point(0, 647);
            statusStripKntConsole.Name = "statusStripKntConsole";
            statusStripKntConsole.Padding = new Padding(1, 0, 16, 0);
            statusStripKntConsole.Size = new Size(1111, 22);
            statusStripKntConsole.TabIndex = 10;
            statusStripKntConsole.Text = "statusStrip1";
            // 
            // statusAction
            // 
            statusAction.Name = "statusAction";
            statusAction.Size = new Size(0, 17);
            // 
            // statusFileName
            // 
            statusFileName.AutoSize = false;
            statusFileName.Name = "statusFileName";
            statusFileName.Size = new Size(1094, 17);
            statusFileName.Spring = true;
            statusFileName.Text = "KntScript";
            statusFileName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            panel1.Controls.Add(splitContainer1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 25);
            panel1.Margin = new Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(1111, 622);
            panel1.TabIndex = 11;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            //
            splitContainer1.Panel1.Controls.Add(textSourceCode);
            //
            // splitContainer1.Panel2
            //
            splitContainer1.Panel2.Controls.Add(panelInput);
            splitContainer1.Size = new Size(1111, 622);
            splitContainer1.SplitterDistance = 569;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 1;
            // 
            // textSourceCode
            // 
            textSourceCode.AcceptsTab = true;
            textSourceCode.Dock = DockStyle.Fill;
            textSourceCode.Font = new Font("Courier New", 11.25F);
            textSourceCode.Location = new Point(0, 0);
            textSourceCode.Margin = new Padding(6);
            textSourceCode.Multiline = true;
            textSourceCode.Name = "textSourceCode";
            textSourceCode.ScrollBars = ScrollBars.Both;
            textSourceCode.Size = new Size(569, 622);
            textSourceCode.TabIndex = 3;
            textSourceCode.WordWrap = false;
            textSourceCode.TextChanged += textSourceCode_TextChanged;
            //
            // panelInput
            //
            panelInput.AutoSize = true;
            panelInput.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelInput.ColumnCount = 3;
            panelInput.ColumnStyles.Add(new ColumnStyle());
            panelInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            panelInput.ColumnStyles.Add(new ColumnStyle());
            panelInput.Controls.Add(labelInput, 0, 0);
            panelInput.Controls.Add(textInput, 1, 0);
            panelInput.Controls.Add(buttonCloseInput, 2, 0);
            panelInput.Dock = DockStyle.Bottom;
            panelInput.Location = new Point(0, 586);
            panelInput.Name = "panelInput";
            panelInput.Padding = new Padding(6, 5, 6, 5);
            panelInput.RowCount = 1;
            panelInput.RowStyles.Add(new RowStyle());
            panelInput.Size = new Size(537, 36);
            panelInput.TabIndex = 0;
            //
            // labelInput
            //
            labelInput.Anchor = AnchorStyles.Left;
            labelInput.AutoSize = true;
            labelInput.Location = new Point(6, 15);
            labelInput.Margin = new Padding(0, 0, 6, 0);
            labelInput.Name = "labelInput";
            labelInput.Size = new Size(38, 15);
            labelInput.TabIndex = 0;
            labelInput.Text = "Input:";
            //
            // buttonCloseInput
            //
            buttonCloseInput.Anchor = AnchorStyles.Right;
            buttonCloseInput.Enabled = false;
            buttonCloseInput.Location = new Point(434, 5);
            buttonCloseInput.Margin = new Padding(0);
            buttonCloseInput.Name = "buttonCloseInput";
            buttonCloseInput.Size = new Size(97, 26);
            buttonCloseInput.TabIndex = 2;
            buttonCloseInput.Text = "Close stdin";
            buttonCloseInput.UseVisualStyleBackColor = true;
            buttonCloseInput.Click += buttonCloseInput_Click;
            //
            // textInput
            //
            textInput.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            textInput.Enabled = false;
            textInput.Location = new Point(50, 7);
            textInput.Margin = new Padding(0, 0, 8, 0);
            textInput.Name = "textInput";
            textInput.Size = new Size(376, 23);
            textInput.TabIndex = 1;
            textInput.KeyDown += textInput_KeyDown;
            //
            // openFileDialogScript
            // 
            openFileDialogScript.FileName = "KntScript";
            // 
            // KntScriptConsoleForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1111, 669);
            MinimumSize = new Size(720, 420);
            Controls.Add(panel1);
            Controls.Add(statusStripKntConsole);
            Controls.Add(toolStripConsole);
            KeyPreview = true;
            Margin = new Padding(4, 3, 4, 3);
            Name = "KntScriptConsoleForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "KntScript - Console";
            Load += KntScriptForm_Load;
            KeyUp += KntScriptForm_KeyUp;
            toolStripConsole.ResumeLayout(false);
            toolStripConsole.PerformLayout();
            statusStripKntConsole.ResumeLayout(false);
            statusStripKntConsole.PerformLayout();
            panel1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panelInput.ResumeLayout(false);
            panelInput.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripConsole;
        private System.Windows.Forms.StatusStrip statusStripKntConsole;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox textSourceCode;
        private System.Windows.Forms.TableLayoutPanel panelInput;
        private System.Windows.Forms.TextBox textInput;
        private System.Windows.Forms.Button buttonCloseInput;
        private System.Windows.Forms.Label labelInput;
        private System.Windows.Forms.ToolStripButton buttonNew;
        private System.Windows.Forms.ToolStripButton buttonOpen;
        private System.Windows.Forms.ToolStripButton buttonSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripStatusLabel statusFileName;
        private System.Windows.Forms.OpenFileDialog openFileDialogScript;
        private System.Windows.Forms.SaveFileDialog saveFileDialogScript;
        private ToolStripStatusLabel statusAction;
        private ToolStripButton buttonInsertCode;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripLabel labelLanguage;
        private ToolStripComboBox comboLanguage;
        private ToolStripDropDownButton buttonRun;
        private ToolStripMenuItem buttonRunIntegrated;
        private ToolStripMenuItem buttonRunStdOut;
    }
}
