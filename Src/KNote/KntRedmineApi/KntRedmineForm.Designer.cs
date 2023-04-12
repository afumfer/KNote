namespace KntRedmineApi
{
    partial class KntRedmineForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KntRedmineForm));
            tabRedmineUtils = new TabControl();
            tabImport = new TabPage();
            label8 = new Label();
            buttonIssuesImportFile = new Button();
            textIssuesImportFile = new TextBox();
            textFolderNumForImportIssues = new TextBox();
            label7 = new Label();
            label6 = new Label();
            listInfoRedmine = new ListBox();
            label5 = new Label();
            textIssuesId = new TextBox();
            buttonImportRedmineIssues = new Button();
            tabPredict = new TabPage();
            label12 = new Label();
            textPredictCategory = new TextBox();
            buttonFindIssue = new Button();
            groupBox2 = new GroupBox();
            buttonPredictPH = new Button();
            textPredictionPH = new TextBox();
            groupBox1 = new GroupBox();
            buttonPredictGestion = new Button();
            textPredictionGestion = new TextBox();
            textPredictDescription = new TextBox();
            textPredictSubject = new TextBox();
            textPredictFindIssue = new TextBox();
            label11 = new Label();
            label10 = new Label();
            label9 = new Label();
            tabOptions = new TabPage();
            buttonSaveParameters = new Button();
            textKNoteImportUser = new TextBox();
            label2 = new Label();
            textToolsPath = new TextBox();
            label1 = new Label();
            textApiKey = new TextBox();
            textHost = new TextBox();
            label4 = new Label();
            label3 = new Label();
            openFileDialog = new OpenFileDialog();
            tabRedmineUtils.SuspendLayout();
            tabImport.SuspendLayout();
            tabPredict.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            tabOptions.SuspendLayout();
            SuspendLayout();
            // 
            // tabRedmineUtils
            // 
            tabRedmineUtils.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabRedmineUtils.Controls.Add(tabImport);
            tabRedmineUtils.Controls.Add(tabPredict);
            tabRedmineUtils.Controls.Add(tabOptions);
            tabRedmineUtils.Location = new Point(6, 6);
            tabRedmineUtils.Name = "tabRedmineUtils";
            tabRedmineUtils.SelectedIndex = 0;
            tabRedmineUtils.Size = new Size(684, 594);
            tabRedmineUtils.TabIndex = 0;
            // 
            // tabImport
            // 
            tabImport.Controls.Add(label8);
            tabImport.Controls.Add(buttonIssuesImportFile);
            tabImport.Controls.Add(textIssuesImportFile);
            tabImport.Controls.Add(textFolderNumForImportIssues);
            tabImport.Controls.Add(label7);
            tabImport.Controls.Add(label6);
            tabImport.Controls.Add(listInfoRedmine);
            tabImport.Controls.Add(label5);
            tabImport.Controls.Add(textIssuesId);
            tabImport.Controls.Add(buttonImportRedmineIssues);
            tabImport.Location = new Point(4, 24);
            tabImport.Name = "tabImport";
            tabImport.Padding = new Padding(3);
            tabImport.Size = new Size(676, 566);
            tabImport.TabIndex = 0;
            tabImport.Text = "Import";
            tabImport.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(9, 6);
            label8.Name = "label8";
            label8.Size = new Size(109, 15);
            label8.TabIndex = 41;
            label8.Text = "Issues # import file:";
            // 
            // buttonIssuesImportFile
            // 
            buttonIssuesImportFile.Location = new Point(451, 24);
            buttonIssuesImportFile.Name = "buttonIssuesImportFile";
            buttonIssuesImportFile.Size = new Size(24, 24);
            buttonIssuesImportFile.TabIndex = 40;
            buttonIssuesImportFile.Text = "...";
            buttonIssuesImportFile.UseVisualStyleBackColor = true;
            buttonIssuesImportFile.Click += buttonIssuesImportFile_Click;
            // 
            // textIssuesImportFile
            // 
            textIssuesImportFile.Location = new Point(9, 24);
            textIssuesImportFile.Name = "textIssuesImportFile";
            textIssuesImportFile.Size = new Size(436, 23);
            textIssuesImportFile.TabIndex = 39;
            // 
            // textFolderNumForImportIssues
            // 
            textFolderNumForImportIssues.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textFolderNumForImportIssues.Location = new Point(9, 76);
            textFolderNumForImportIssues.Name = "textFolderNumForImportIssues";
            textFolderNumForImportIssues.Size = new Size(174, 23);
            textFolderNumForImportIssues.TabIndex = 38;
            textFolderNumForImportIssues.Text = "1";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(9, 58);
            label7.Name = "label7";
            label7.Size = new Size(170, 15);
            label7.TabIndex = 37;
            label7.Text = "Root folder # for import issues:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(189, 116);
            label6.Name = "label6";
            label6.Size = new Size(51, 15);
            label6.TabIndex = 36;
            label6.Text = "Info log:";
            // 
            // listInfoRedmine
            // 
            listInfoRedmine.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listInfoRedmine.FormattingEnabled = true;
            listInfoRedmine.ItemHeight = 15;
            listInfoRedmine.Location = new Point(189, 135);
            listInfoRedmine.Name = "listInfoRedmine";
            listInfoRedmine.Size = new Size(478, 424);
            listInfoRedmine.TabIndex = 35;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(9, 116);
            label5.Name = "label5";
            label5.Size = new Size(103, 15);
            label5.TabIndex = 34;
            label5.Text = "HU IDs for import:";
            // 
            // textIssuesId
            // 
            textIssuesId.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            textIssuesId.Location = new Point(9, 134);
            textIssuesId.Multiline = true;
            textIssuesId.Name = "textIssuesId";
            textIssuesId.ScrollBars = ScrollBars.Vertical;
            textIssuesId.Size = new Size(174, 425);
            textIssuesId.TabIndex = 33;
            // 
            // buttonImportRedmineIssues
            // 
            buttonImportRedmineIssues.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonImportRedmineIssues.Location = new Point(499, 24);
            buttonImportRedmineIssues.Name = "buttonImportRedmineIssues";
            buttonImportRedmineIssues.Size = new Size(168, 27);
            buttonImportRedmineIssues.TabIndex = 28;
            buttonImportRedmineIssues.Text = "Import RedMine Issues";
            buttonImportRedmineIssues.UseVisualStyleBackColor = true;
            buttonImportRedmineIssues.Click += buttonImportRedmineIssues_Click;
            // 
            // tabPredict
            // 
            tabPredict.Controls.Add(label12);
            tabPredict.Controls.Add(textPredictCategory);
            tabPredict.Controls.Add(buttonFindIssue);
            tabPredict.Controls.Add(groupBox2);
            tabPredict.Controls.Add(groupBox1);
            tabPredict.Controls.Add(textPredictDescription);
            tabPredict.Controls.Add(textPredictSubject);
            tabPredict.Controls.Add(textPredictFindIssue);
            tabPredict.Controls.Add(label11);
            tabPredict.Controls.Add(label10);
            tabPredict.Controls.Add(label9);
            tabPredict.Location = new Point(4, 24);
            tabPredict.Name = "tabPredict";
            tabPredict.Padding = new Padding(3);
            tabPredict.Size = new Size(676, 566);
            tabPredict.TabIndex = 1;
            tabPredict.Text = "Predict";
            tabPredict.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            label12.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label12.AutoSize = true;
            label12.Location = new Point(15, 307);
            label12.Name = "label12";
            label12.Size = new Size(58, 15);
            label12.TabIndex = 32;
            label12.Text = "Category:";
            // 
            // textPredictCategory
            // 
            textPredictCategory.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textPredictCategory.Location = new Point(94, 307);
            textPredictCategory.Name = "textPredictCategory";
            textPredictCategory.Size = new Size(566, 23);
            textPredictCategory.TabIndex = 31;
            // 
            // buttonFindIssue
            // 
            buttonFindIssue.Location = new Point(187, 18);
            buttonFindIssue.Name = "buttonFindIssue";
            buttonFindIssue.Size = new Size(119, 23);
            buttonFindIssue.TabIndex = 30;
            buttonFindIssue.Text = "Find Issue";
            buttonFindIssue.UseVisualStyleBackColor = true;
            buttonFindIssue.Click += buttonFindIssue_Click;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.Controls.Add(buttonPredictPH);
            groupBox2.Controls.Add(textPredictionPH);
            groupBox2.Location = new Point(9, 439);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(651, 98);
            groupBox2.TabIndex = 29;
            groupBox2.TabStop = false;
            groupBox2.Text = "Predict -> PH";
            // 
            // buttonPredictPH
            // 
            buttonPredictPH.Location = new Point(6, 23);
            buttonPredictPH.Name = "buttonPredictPH";
            buttonPredictPH.Size = new Size(122, 22);
            buttonPredictPH.TabIndex = 2;
            buttonPredictPH.Text = "Predict";
            buttonPredictPH.UseVisualStyleBackColor = true;
            buttonPredictPH.Click += buttonPredictPH_Click;
            // 
            // textPredictionPH
            // 
            textPredictionPH.Location = new Point(134, 22);
            textPredictionPH.Name = "textPredictionPH";
            textPredictionPH.Size = new Size(163, 23);
            textPredictionPH.TabIndex = 0;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(buttonPredictGestion);
            groupBox1.Controls.Add(textPredictionGestion);
            groupBox1.Location = new Point(9, 335);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(651, 98);
            groupBox1.TabIndex = 28;
            groupBox1.TabStop = false;
            groupBox1.Text = "Predict -> Gestión";
            // 
            // buttonPredictGestion
            // 
            buttonPredictGestion.Location = new Point(6, 21);
            buttonPredictGestion.Name = "buttonPredictGestion";
            buttonPredictGestion.Size = new Size(122, 22);
            buttonPredictGestion.TabIndex = 1;
            buttonPredictGestion.Text = "Predict";
            buttonPredictGestion.UseVisualStyleBackColor = true;
            buttonPredictGestion.Click += buttonPredictGestion_Click;
            // 
            // textPredictionGestion
            // 
            textPredictionGestion.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textPredictionGestion.Location = new Point(134, 22);
            textPredictionGestion.Name = "textPredictionGestion";
            textPredictionGestion.Size = new Size(511, 23);
            textPredictionGestion.TabIndex = 0;
            // 
            // textPredictDescription
            // 
            textPredictDescription.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textPredictDescription.Location = new Point(94, 74);
            textPredictDescription.Multiline = true;
            textPredictDescription.Name = "textPredictDescription";
            textPredictDescription.ScrollBars = ScrollBars.Vertical;
            textPredictDescription.Size = new Size(566, 227);
            textPredictDescription.TabIndex = 27;
            // 
            // textPredictSubject
            // 
            textPredictSubject.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textPredictSubject.Location = new Point(94, 45);
            textPredictSubject.Name = "textPredictSubject";
            textPredictSubject.Size = new Size(566, 23);
            textPredictSubject.TabIndex = 26;
            // 
            // textPredictFindIssue
            // 
            textPredictFindIssue.Location = new Point(94, 18);
            textPredictFindIssue.Name = "textPredictFindIssue";
            textPredictFindIssue.Size = new Size(81, 23);
            textPredictFindIssue.TabIndex = 25;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(11, 74);
            label11.Name = "label11";
            label11.Size = new Size(70, 15);
            label11.TabIndex = 24;
            label11.Text = "Description:";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(11, 48);
            label10.Name = "label10";
            label10.Size = new Size(49, 15);
            label10.TabIndex = 23;
            label10.Text = "Subject:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(11, 21);
            label9.Name = "label9";
            label9.Size = new Size(46, 15);
            label9.TabIndex = 22;
            label9.Text = "Issue #:";
            // 
            // tabOptions
            // 
            tabOptions.Controls.Add(buttonSaveParameters);
            tabOptions.Controls.Add(textKNoteImportUser);
            tabOptions.Controls.Add(label2);
            tabOptions.Controls.Add(textToolsPath);
            tabOptions.Controls.Add(label1);
            tabOptions.Controls.Add(textApiKey);
            tabOptions.Controls.Add(textHost);
            tabOptions.Controls.Add(label4);
            tabOptions.Controls.Add(label3);
            tabOptions.Location = new Point(4, 24);
            tabOptions.Name = "tabOptions";
            tabOptions.Size = new Size(676, 566);
            tabOptions.TabIndex = 2;
            tabOptions.Text = "Configuration options";
            tabOptions.UseVisualStyleBackColor = true;
            // 
            // buttonSaveParameters
            // 
            buttonSaveParameters.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonSaveParameters.Location = new Point(557, 531);
            buttonSaveParameters.Name = "buttonSaveParameters";
            buttonSaveParameters.Size = new Size(103, 23);
            buttonSaveParameters.TabIndex = 41;
            buttonSaveParameters.Text = "Save parameters";
            buttonSaveParameters.UseVisualStyleBackColor = true;
            buttonSaveParameters.Click += buttonSaveParameters_Click;
            // 
            // textKNoteImportUser
            // 
            textKNoteImportUser.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textKNoteImportUser.Location = new Point(15, 185);
            textKNoteImportUser.Name = "textKNoteImportUser";
            textKNoteImportUser.Size = new Size(127, 23);
            textKNoteImportUser.TabIndex = 40;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(15, 167);
            label2.Name = "label2";
            label2.Size = new Size(107, 15);
            label2.TabIndex = 39;
            label2.Text = "KNote import user:";
            // 
            // textToolsPath
            // 
            textToolsPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textToolsPath.Location = new Point(15, 132);
            textToolsPath.Name = "textToolsPath";
            textToolsPath.Size = new Size(645, 23);
            textToolsPath.TabIndex = 38;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(13, 114);
            label1.Name = "label1";
            label1.Size = new Size(64, 15);
            label1.TabIndex = 37;
            label1.Text = "Tools path:";
            // 
            // textApiKey
            // 
            textApiKey.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textApiKey.Location = new Point(15, 80);
            textApiKey.Name = "textApiKey";
            textApiKey.Size = new Size(645, 23);
            textApiKey.TabIndex = 36;
            // 
            // textHost
            // 
            textHost.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textHost.Location = new Point(15, 30);
            textHost.Name = "textHost";
            textHost.Size = new Size(645, 23);
            textHost.TabIndex = 35;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(15, 62);
            label4.Name = "label4";
            label4.Size = new Size(97, 15);
            label4.TabIndex = 34;
            label4.Text = "Redmine ApiKey:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(13, 13);
            label3.Name = "label3";
            label3.Size = new Size(106, 15);
            label3.TabIndex = 33;
            label3.Text = "Redmine API Host:";
            // 
            // KntRedmineForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(694, 605);
            Controls.Add(tabRedmineUtils);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "KntRedmineForm";
            Text = "KNote Redmine utils";
            FormClosing += KntRedmineForm_FormClosing;
            Load += KntRedmineForm_Load;
            tabRedmineUtils.ResumeLayout(false);
            tabImport.ResumeLayout(false);
            tabImport.PerformLayout();
            tabPredict.ResumeLayout(false);
            tabPredict.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tabOptions.ResumeLayout(false);
            tabOptions.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabRedmineUtils;
        private TabPage tabImport;
        private TabPage tabPredict;
        private Label label8;
        private Button buttonIssuesImportFile;
        private TextBox textIssuesImportFile;
        private TextBox textFolderNumForImportIssues;
        private Label label7;
        private Label label6;
        private ListBox listInfoRedmine;
        private Label label5;
        private TextBox textIssuesId;
        private Button buttonImportRedmineIssues;
        private Label label12;
        private TextBox textPredictCategory;
        private Button buttonFindIssue;
        private GroupBox groupBox2;
        private Button buttonPredictPH;
        private TextBox textPredictionPH;
        private GroupBox groupBox1;
        private Button buttonPredictGestion;
        private TextBox textPredictionGestion;
        private TextBox textPredictDescription;
        private TextBox textPredictSubject;
        private TextBox textPredictFindIssue;
        private Label label11;
        private Label label10;
        private Label label9;
        private OpenFileDialog openFileDialog;
        private TabPage tabOptions;
        private TextBox textToolsPath;
        private Label label1;
        private TextBox textApiKey;
        private TextBox textHost;
        private Label label4;
        private Label label3;
        private TextBox textKNoteImportUser;
        private Label label2;
        private Button buttonSaveParameters;
    }
}