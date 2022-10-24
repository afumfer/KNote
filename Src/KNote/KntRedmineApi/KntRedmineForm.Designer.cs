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
            this.tabRedmineUtils = new System.Windows.Forms.TabControl();
            this.tabImport = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.buttonIssuesImportFile = new System.Windows.Forms.Button();
            this.textIssuesImportFile = new System.Windows.Forms.TextBox();
            this.textFolderNumForImportIssues = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.listInfoRedmine = new System.Windows.Forms.ListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textIssuesId = new System.Windows.Forms.TextBox();
            this.textApiKey = new System.Windows.Forms.TextBox();
            this.textHost = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonImportRedmineIssues = new System.Windows.Forms.Button();
            this.tabPredict = new System.Windows.Forms.TabPage();
            this.label12 = new System.Windows.Forms.Label();
            this.textPredictCategory = new System.Windows.Forms.TextBox();
            this.buttonFindIssue = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonPredictPH = new System.Windows.Forms.Button();
            this.textPredictionPH = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonPredictGestion = new System.Windows.Forms.Button();
            this.textPredictionGestion = new System.Windows.Forms.TextBox();
            this.textPredictDescription = new System.Windows.Forms.TextBox();
            this.textPredictSubject = new System.Windows.Forms.TextBox();
            this.textPredictFindIssue = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.tabRedmineUtils.SuspendLayout();
            this.tabImport.SuspendLayout();
            this.tabPredict.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabRedmineUtils
            // 
            this.tabRedmineUtils.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabRedmineUtils.Controls.Add(this.tabImport);
            this.tabRedmineUtils.Controls.Add(this.tabPredict);
            this.tabRedmineUtils.Location = new System.Drawing.Point(12, 12);
            this.tabRedmineUtils.Name = "tabRedmineUtils";
            this.tabRedmineUtils.SelectedIndex = 0;
            this.tabRedmineUtils.Size = new System.Drawing.Size(676, 582);
            this.tabRedmineUtils.TabIndex = 0;
            // 
            // tabImport
            // 
            this.tabImport.Controls.Add(this.label8);
            this.tabImport.Controls.Add(this.buttonIssuesImportFile);
            this.tabImport.Controls.Add(this.textIssuesImportFile);
            this.tabImport.Controls.Add(this.textFolderNumForImportIssues);
            this.tabImport.Controls.Add(this.label7);
            this.tabImport.Controls.Add(this.label6);
            this.tabImport.Controls.Add(this.listInfoRedmine);
            this.tabImport.Controls.Add(this.label5);
            this.tabImport.Controls.Add(this.textIssuesId);
            this.tabImport.Controls.Add(this.textApiKey);
            this.tabImport.Controls.Add(this.textHost);
            this.tabImport.Controls.Add(this.label4);
            this.tabImport.Controls.Add(this.label3);
            this.tabImport.Controls.Add(this.buttonImportRedmineIssues);
            this.tabImport.Location = new System.Drawing.Point(4, 24);
            this.tabImport.Name = "tabImport";
            this.tabImport.Padding = new System.Windows.Forms.Padding(3);
            this.tabImport.Size = new System.Drawing.Size(668, 554);
            this.tabImport.TabIndex = 0;
            this.tabImport.Text = "Import";
            this.tabImport.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(9, 109);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(109, 15);
            this.label8.TabIndex = 41;
            this.label8.Text = "Issues # import file:";
            // 
            // buttonIssuesImportFile
            // 
            this.buttonIssuesImportFile.Location = new System.Drawing.Point(451, 127);
            this.buttonIssuesImportFile.Name = "buttonIssuesImportFile";
            this.buttonIssuesImportFile.Size = new System.Drawing.Size(24, 24);
            this.buttonIssuesImportFile.TabIndex = 40;
            this.buttonIssuesImportFile.Text = "...";
            this.buttonIssuesImportFile.UseVisualStyleBackColor = true;
            this.buttonIssuesImportFile.Click += new System.EventHandler(this.buttonIssuesImportFile_Click);
            // 
            // textIssuesImportFile
            // 
            this.textIssuesImportFile.Location = new System.Drawing.Point(9, 127);
            this.textIssuesImportFile.Name = "textIssuesImportFile";
            this.textIssuesImportFile.Size = new System.Drawing.Size(436, 23);
            this.textIssuesImportFile.TabIndex = 39;
            // 
            // textFolderNumForImportIssues
            // 
            this.textFolderNumForImportIssues.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textFolderNumForImportIssues.Location = new System.Drawing.Point(492, 127);
            this.textFolderNumForImportIssues.Name = "textFolderNumForImportIssues";
            this.textFolderNumForImportIssues.Size = new System.Drawing.Size(166, 23);
            this.textFolderNumForImportIssues.TabIndex = 38;
            this.textFolderNumForImportIssues.Text = "1";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(492, 109);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(170, 15);
            this.label7.TabIndex = 37;
            this.label7.Text = "Root folder # for import issues:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(288, 177);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(28, 15);
            this.label6.TabIndex = 36;
            this.label6.Text = "Info";
            // 
            // listInfoRedmine
            // 
            this.listInfoRedmine.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listInfoRedmine.FormattingEnabled = true;
            this.listInfoRedmine.ItemHeight = 15;
            this.listInfoRedmine.Location = new System.Drawing.Point(288, 195);
            this.listInfoRedmine.Name = "listInfoRedmine";
            this.listInfoRedmine.Size = new System.Drawing.Size(371, 349);
            this.listInfoRedmine.TabIndex = 35;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 179);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 15);
            this.label5.TabIndex = 34;
            this.label5.Text = "HU IDs";
            // 
            // textIssuesId
            // 
            this.textIssuesId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textIssuesId.Location = new System.Drawing.Point(9, 197);
            this.textIssuesId.Multiline = true;
            this.textIssuesId.Name = "textIssuesId";
            this.textIssuesId.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textIssuesId.Size = new System.Drawing.Size(273, 350);
            this.textIssuesId.TabIndex = 33;
            // 
            // textApiKey
            // 
            this.textApiKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textApiKey.Location = new System.Drawing.Point(9, 74);
            this.textApiKey.Name = "textApiKey";
            this.textApiKey.Size = new System.Drawing.Size(487, 23);
            this.textApiKey.TabIndex = 32;
            // 
            // textHost
            // 
            this.textHost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textHost.Location = new System.Drawing.Point(9, 24);
            this.textHost.Name = "textHost";
            this.textHost.Size = new System.Drawing.Size(487, 23);
            this.textHost.TabIndex = 31;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 15);
            this.label4.TabIndex = 30;
            this.label4.Text = "ApiKey:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 15);
            this.label3.TabIndex = 29;
            this.label3.Text = "Host:";
            // 
            // buttonImportRedmineIssues
            // 
            this.buttonImportRedmineIssues.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonImportRedmineIssues.Location = new System.Drawing.Point(515, 20);
            this.buttonImportRedmineIssues.Name = "buttonImportRedmineIssues";
            this.buttonImportRedmineIssues.Size = new System.Drawing.Size(144, 27);
            this.buttonImportRedmineIssues.TabIndex = 28;
            this.buttonImportRedmineIssues.Text = "Import RedMine Issues";
            this.buttonImportRedmineIssues.UseVisualStyleBackColor = true;
            this.buttonImportRedmineIssues.Click += new System.EventHandler(this.buttonImportRedmineIssues_Click);
            // 
            // tabPredict
            // 
            this.tabPredict.Controls.Add(this.label12);
            this.tabPredict.Controls.Add(this.textPredictCategory);
            this.tabPredict.Controls.Add(this.buttonFindIssue);
            this.tabPredict.Controls.Add(this.groupBox2);
            this.tabPredict.Controls.Add(this.groupBox1);
            this.tabPredict.Controls.Add(this.textPredictDescription);
            this.tabPredict.Controls.Add(this.textPredictSubject);
            this.tabPredict.Controls.Add(this.textPredictFindIssue);
            this.tabPredict.Controls.Add(this.label11);
            this.tabPredict.Controls.Add(this.label10);
            this.tabPredict.Controls.Add(this.label9);
            this.tabPredict.Location = new System.Drawing.Point(4, 24);
            this.tabPredict.Name = "tabPredict";
            this.tabPredict.Padding = new System.Windows.Forms.Padding(3);
            this.tabPredict.Size = new System.Drawing.Size(668, 554);
            this.tabPredict.TabIndex = 1;
            this.tabPredict.Text = "Predict";
            this.tabPredict.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(15, 307);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(58, 15);
            this.label12.TabIndex = 32;
            this.label12.Text = "Category:";
            // 
            // textPredictCategory
            // 
            this.textPredictCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textPredictCategory.Location = new System.Drawing.Point(94, 307);
            this.textPredictCategory.Name = "textPredictCategory";
            this.textPredictCategory.Size = new System.Drawing.Size(566, 23);
            this.textPredictCategory.TabIndex = 31;
            // 
            // buttonFindIssue
            // 
            this.buttonFindIssue.Location = new System.Drawing.Point(187, 18);
            this.buttonFindIssue.Name = "buttonFindIssue";
            this.buttonFindIssue.Size = new System.Drawing.Size(119, 23);
            this.buttonFindIssue.TabIndex = 30;
            this.buttonFindIssue.Text = "Find Issue";
            this.buttonFindIssue.UseVisualStyleBackColor = true;
            this.buttonFindIssue.Click += new System.EventHandler(this.buttonFindIssue_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.buttonPredictPH);
            this.groupBox2.Controls.Add(this.textPredictionPH);
            this.groupBox2.Location = new System.Drawing.Point(9, 439);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(651, 98);
            this.groupBox2.TabIndex = 29;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Predict -> PH";
            // 
            // buttonPredictPH
            // 
            this.buttonPredictPH.Location = new System.Drawing.Point(6, 23);
            this.buttonPredictPH.Name = "buttonPredictPH";
            this.buttonPredictPH.Size = new System.Drawing.Size(122, 22);
            this.buttonPredictPH.TabIndex = 2;
            this.buttonPredictPH.Text = "Predict";
            this.buttonPredictPH.UseVisualStyleBackColor = true;
            this.buttonPredictPH.Click += new System.EventHandler(this.buttonPredictPH_Click);
            // 
            // textPredictionPH
            // 
            this.textPredictionPH.Location = new System.Drawing.Point(134, 22);
            this.textPredictionPH.Name = "textPredictionPH";
            this.textPredictionPH.Size = new System.Drawing.Size(163, 23);
            this.textPredictionPH.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.buttonPredictGestion);
            this.groupBox1.Controls.Add(this.textPredictionGestion);
            this.groupBox1.Location = new System.Drawing.Point(9, 335);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(651, 98);
            this.groupBox1.TabIndex = 28;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Predict -> Gestión";
            // 
            // buttonPredictGestion
            // 
            this.buttonPredictGestion.Location = new System.Drawing.Point(6, 21);
            this.buttonPredictGestion.Name = "buttonPredictGestion";
            this.buttonPredictGestion.Size = new System.Drawing.Size(122, 22);
            this.buttonPredictGestion.TabIndex = 1;
            this.buttonPredictGestion.Text = "Predict";
            this.buttonPredictGestion.UseVisualStyleBackColor = true;
            this.buttonPredictGestion.Click += new System.EventHandler(this.buttonPredictGestion_Click);
            // 
            // textPredictionGestion
            // 
            this.textPredictionGestion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textPredictionGestion.Location = new System.Drawing.Point(134, 22);
            this.textPredictionGestion.Name = "textPredictionGestion";
            this.textPredictionGestion.Size = new System.Drawing.Size(962, 23);
            this.textPredictionGestion.TabIndex = 0;
            // 
            // textPredictDescription
            // 
            this.textPredictDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textPredictDescription.Location = new System.Drawing.Point(94, 74);
            this.textPredictDescription.Multiline = true;
            this.textPredictDescription.Name = "textPredictDescription";
            this.textPredictDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textPredictDescription.Size = new System.Drawing.Size(566, 227);
            this.textPredictDescription.TabIndex = 27;
            // 
            // textPredictSubject
            // 
            this.textPredictSubject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textPredictSubject.Location = new System.Drawing.Point(94, 45);
            this.textPredictSubject.Name = "textPredictSubject";
            this.textPredictSubject.Size = new System.Drawing.Size(566, 23);
            this.textPredictSubject.TabIndex = 26;
            // 
            // textPredictFindIssue
            // 
            this.textPredictFindIssue.Location = new System.Drawing.Point(94, 18);
            this.textPredictFindIssue.Name = "textPredictFindIssue";
            this.textPredictFindIssue.Size = new System.Drawing.Size(81, 23);
            this.textPredictFindIssue.TabIndex = 25;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(11, 74);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(70, 15);
            this.label11.TabIndex = 24;
            this.label11.Text = "Description:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(11, 48);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(49, 15);
            this.label10.TabIndex = 23;
            this.label10.Text = "Subject:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(11, 21);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(46, 15);
            this.label9.TabIndex = 22;
            this.label9.Text = "Issue #:";
            // 
            // KntRedmineForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(694, 605);
            this.Controls.Add(this.tabRedmineUtils);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "KntRedmineForm";
            this.Text = "KaNote Redmine utils";
            this.Load += new System.EventHandler(this.KntRedmineForm_Load);
            this.tabRedmineUtils.ResumeLayout(false);
            this.tabImport.ResumeLayout(false);
            this.tabImport.PerformLayout();
            this.tabPredict.ResumeLayout(false);
            this.tabPredict.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

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
        private TextBox textApiKey;
        private TextBox textHost;
        private Label label4;
        private Label label3;
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
    }
}