
namespace KNote.ClientWin.Views
{
    partial class RepositoryEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RepositoryEditorForm));
            buttonCancel = new Button();
            buttonAccept = new Button();
            panelForm = new Panel();
            checkResourceContentInDB = new CheckBox();
            buttonSelectDirectoryResources = new Button();
            textResourcesContainer = new TextBox();
            textResourcesContainerUrl = new TextBox();
            textResourcesContainerRoot = new TextBox();
            labelContainerUrl = new Label();
            labelContainerRoot = new Label();
            labelContainer = new Label();
            panelMSSqlServer = new Panel();
            textSQLDataBase = new TextBox();
            label1 = new Label();
            textSQLServer = new TextBox();
            label5 = new Label();
            panelSqLite = new Panel();
            buttonSelectFile = new Button();
            buttonSelectDirectory = new Button();
            textSqLiteDataBase = new TextBox();
            labelSqLiteDataBase = new Label();
            textSqLiteDirectory = new TextBox();
            labelDirectory = new Label();
            groupRepositoryType = new GroupBox();
            radioMSSqlServer = new RadioButton();
            radioSqLite = new RadioButton();
            buttonFolderSearch = new Button();
            labelAlias = new Label();
            textAliasName = new TextBox();
            panelForm.SuspendLayout();
            panelMSSqlServer.SuspendLayout();
            panelSqLite.SuspendLayout();
            groupRepositoryType.SuspendLayout();
            SuspendLayout();
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Location = new Point(563, 563);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(64, 29);
            buttonCancel.TabIndex = 15;
            buttonCancel.Text = "&Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonAccept
            // 
            buttonAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAccept.Location = new Point(493, 563);
            buttonAccept.Name = "buttonAccept";
            buttonAccept.Size = new Size(64, 29);
            buttonAccept.TabIndex = 14;
            buttonAccept.Text = "&Accept";
            buttonAccept.UseVisualStyleBackColor = true;
            buttonAccept.Click += buttonAccept_Click;
            // 
            // panelForm
            // 
            panelForm.Controls.Add(checkResourceContentInDB);
            panelForm.Controls.Add(buttonSelectDirectoryResources);
            panelForm.Controls.Add(textResourcesContainer);
            panelForm.Controls.Add(textResourcesContainerUrl);
            panelForm.Controls.Add(textResourcesContainerRoot);
            panelForm.Controls.Add(labelContainerUrl);
            panelForm.Controls.Add(labelContainerRoot);
            panelForm.Controls.Add(labelContainer);
            panelForm.Controls.Add(panelMSSqlServer);
            panelForm.Controls.Add(panelSqLite);
            panelForm.Controls.Add(groupRepositoryType);
            panelForm.Controls.Add(buttonFolderSearch);
            panelForm.Controls.Add(labelAlias);
            panelForm.Controls.Add(textAliasName);
            panelForm.Dock = DockStyle.Top;
            panelForm.Location = new Point(0, 0);
            panelForm.Name = "panelForm";
            panelForm.Size = new Size(639, 567);
            panelForm.TabIndex = 3;
            // 
            // checkResourceContentInDB
            // 
            checkResourceContentInDB.AutoSize = true;
            checkResourceContentInDB.Location = new Point(314, 150);
            checkResourceContentInDB.Name = "checkResourceContentInDB";
            checkResourceContentInDB.Size = new Size(277, 19);
            checkResourceContentInDB.TabIndex = 4;
            checkResourceContentInDB.Text = "Save a copy of the resource content in database";
            checkResourceContentInDB.UseVisualStyleBackColor = true;
            // 
            // buttonSelectDirectoryResources
            // 
            buttonSelectDirectoryResources.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSelectDirectoryResources.Location = new Point(597, 198);
            buttonSelectDirectoryResources.Name = "buttonSelectDirectoryResources";
            buttonSelectDirectoryResources.Size = new Size(24, 23);
            buttonSelectDirectoryResources.TabIndex = 6;
            buttonSelectDirectoryResources.Text = "...";
            buttonSelectDirectoryResources.UseVisualStyleBackColor = true;
            buttonSelectDirectoryResources.Click += buttonSelectDirectoryResources_Click;
            // 
            // textResourcesContainer
            // 
            textResourcesContainer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textResourcesContainer.Location = new Point(10, 148);
            textResourcesContainer.Name = "textResourcesContainer";
            textResourcesContainer.Size = new Size(277, 23);
            textResourcesContainer.TabIndex = 3;
            // 
            // textResourcesContainerUrl
            // 
            textResourcesContainerUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textResourcesContainerUrl.Location = new Point(8, 248);
            textResourcesContainerUrl.Name = "textResourcesContainerUrl";
            textResourcesContainerUrl.Size = new Size(613, 23);
            textResourcesContainerUrl.TabIndex = 7;
            // 
            // textResourcesContainerRoot
            // 
            textResourcesContainerRoot.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textResourcesContainerRoot.Location = new Point(8, 198);
            textResourcesContainerRoot.Name = "textResourcesContainerRoot";
            textResourcesContainerRoot.Size = new Size(583, 23);
            textResourcesContainerRoot.TabIndex = 5;
            // 
            // labelContainerUrl
            // 
            labelContainerUrl.AutoSize = true;
            labelContainerUrl.Location = new Point(8, 230);
            labelContainerUrl.Name = "labelContainerUrl";
            labelContainerUrl.Size = new Size(184, 15);
            labelContainerUrl.TabIndex = 17;
            labelContainerUrl.Text = "Resources container root file URL:";
            // 
            // labelContainerRoot
            // 
            labelContainerRoot.AutoSize = true;
            labelContainerRoot.Location = new Point(8, 180);
            labelContainerRoot.Name = "labelContainerRoot";
            labelContainerRoot.Size = new Size(175, 15);
            labelContainerRoot.TabIndex = 16;
            labelContainerRoot.Text = "Resources container root folder:";
            // 
            // labelContainer
            // 
            labelContainer.AutoSize = true;
            labelContainer.Location = new Point(8, 130);
            labelContainer.Name = "labelContainer";
            labelContainer.Size = new Size(149, 15);
            labelContainer.TabIndex = 15;
            labelContainer.Text = "Resources container name:";
            // 
            // panelMSSqlServer
            // 
            panelMSSqlServer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelMSSqlServer.BorderStyle = BorderStyle.FixedSingle;
            panelMSSqlServer.Controls.Add(textSQLDataBase);
            panelMSSqlServer.Controls.Add(label1);
            panelMSSqlServer.Controls.Add(textSQLServer);
            panelMSSqlServer.Controls.Add(label5);
            panelMSSqlServer.Location = new Point(6, 404);
            panelMSSqlServer.Name = "panelMSSqlServer";
            panelMSSqlServer.Size = new Size(624, 117);
            panelMSSqlServer.TabIndex = 14;
            // 
            // textSQLDataBase
            // 
            textSQLDataBase.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textSQLDataBase.Location = new Point(1, 83);
            textSQLDataBase.Name = "textSQLDataBase";
            textSQLDataBase.Size = new Size(613, 23);
            textSQLDataBase.TabIndex = 13;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(1, 65);
            label1.Name = "label1";
            label1.Size = new Size(61, 15);
            label1.TabIndex = 11;
            label1.Text = "Data base:";
            // 
            // textSQLServer
            // 
            textSQLServer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textSQLServer.Location = new Point(1, 28);
            textSQLServer.Name = "textSQLServer";
            textSQLServer.Size = new Size(613, 23);
            textSQLServer.TabIndex = 12;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(1, 10);
            label5.Name = "label5";
            label5.Size = new Size(115, 15);
            label5.TabIndex = 8;
            label5.Text = "SQL Server\\instance:";
            // 
            // panelSqLite
            // 
            panelSqLite.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelSqLite.BorderStyle = BorderStyle.FixedSingle;
            panelSqLite.Controls.Add(buttonSelectFile);
            panelSqLite.Controls.Add(buttonSelectDirectory);
            panelSqLite.Controls.Add(textSqLiteDataBase);
            panelSqLite.Controls.Add(labelSqLiteDataBase);
            panelSqLite.Controls.Add(textSqLiteDirectory);
            panelSqLite.Controls.Add(labelDirectory);
            panelSqLite.Location = new Point(6, 277);
            panelSqLite.Name = "panelSqLite";
            panelSqLite.Size = new Size(624, 121);
            panelSqLite.TabIndex = 13;
            // 
            // buttonSelectFile
            // 
            buttonSelectFile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSelectFile.Location = new Point(590, 82);
            buttonSelectFile.Name = "buttonSelectFile";
            buttonSelectFile.Size = new Size(24, 23);
            buttonSelectFile.TabIndex = 11;
            buttonSelectFile.Text = "...";
            buttonSelectFile.UseVisualStyleBackColor = true;
            buttonSelectFile.Visible = false;
            buttonSelectFile.Click += buttonSelectFile_Click;
            // 
            // buttonSelectDirectory
            // 
            buttonSelectDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSelectDirectory.Location = new Point(590, 28);
            buttonSelectDirectory.Name = "buttonSelectDirectory";
            buttonSelectDirectory.Size = new Size(24, 23);
            buttonSelectDirectory.TabIndex = 9;
            buttonSelectDirectory.Text = "...";
            buttonSelectDirectory.UseVisualStyleBackColor = true;
            buttonSelectDirectory.Click += buttonSelectDirectory_Click;
            // 
            // textSqLiteDataBase
            // 
            textSqLiteDataBase.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textSqLiteDataBase.Location = new Point(1, 83);
            textSqLiteDataBase.Name = "textSqLiteDataBase";
            textSqLiteDataBase.Size = new Size(613, 23);
            textSqLiteDataBase.TabIndex = 10;
            // 
            // labelSqLiteDataBase
            // 
            labelSqLiteDataBase.AutoSize = true;
            labelSqLiteDataBase.Location = new Point(1, 65);
            labelSqLiteDataBase.Name = "labelSqLiteDataBase";
            labelSqLiteDataBase.Size = new Size(80, 15);
            labelSqLiteDataBase.TabIndex = 10;
            labelSqLiteDataBase.Text = "Data base file:";
            // 
            // textSqLiteDirectory
            // 
            textSqLiteDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textSqLiteDirectory.Location = new Point(1, 28);
            textSqLiteDirectory.Name = "textSqLiteDirectory";
            textSqLiteDirectory.Size = new Size(583, 23);
            textSqLiteDirectory.TabIndex = 8;
            // 
            // labelDirectory
            // 
            labelDirectory.AutoSize = true;
            labelDirectory.Location = new Point(1, 10);
            labelDirectory.Name = "labelDirectory";
            labelDirectory.Size = new Size(108, 15);
            labelDirectory.TabIndex = 8;
            labelDirectory.Text = "Database directory:";
            // 
            // groupRepositoryType
            // 
            groupRepositoryType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupRepositoryType.Controls.Add(radioMSSqlServer);
            groupRepositoryType.Controls.Add(radioSqLite);
            groupRepositoryType.Location = new Point(8, 7);
            groupRepositoryType.Name = "groupRepositoryType";
            groupRepositoryType.Size = new Size(615, 61);
            groupRepositoryType.TabIndex = 12;
            groupRepositoryType.TabStop = false;
            groupRepositoryType.Text = "Reposoty database type";
            // 
            // radioMSSqlServer
            // 
            radioMSSqlServer.AutoSize = true;
            radioMSSqlServer.Location = new Point(156, 22);
            radioMSSqlServer.Name = "radioMSSqlServer";
            radioMSSqlServer.Size = new Size(135, 19);
            radioMSSqlServer.TabIndex = 1;
            radioMSSqlServer.TabStop = true;
            radioMSSqlServer.Text = "Microsoft SQL Server";
            radioMSSqlServer.UseVisualStyleBackColor = true;
            radioMSSqlServer.CheckedChanged += radioDataBase_CheckedChanged;
            // 
            // radioSqLite
            // 
            radioSqLite.AutoSize = true;
            radioSqLite.Location = new Point(35, 22);
            radioSqLite.Name = "radioSqLite";
            radioSqLite.Size = new Size(57, 19);
            radioSqLite.TabIndex = 0;
            radioSqLite.TabStop = true;
            radioSqLite.Text = "SqLite";
            radioSqLite.UseVisualStyleBackColor = true;
            radioSqLite.CheckedChanged += radioDataBase_CheckedChanged;
            // 
            // buttonFolderSearch
            // 
            buttonFolderSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonFolderSearch.BackColor = SystemColors.Control;
            buttonFolderSearch.FlatStyle = FlatStyle.System;
            buttonFolderSearch.Location = new Point(899, 199);
            buttonFolderSearch.Margin = new Padding(4, 3, 4, 3);
            buttonFolderSearch.Name = "buttonFolderSearch";
            buttonFolderSearch.Size = new Size(27, 25);
            buttonFolderSearch.TabIndex = 11;
            buttonFolderSearch.Text = "...";
            buttonFolderSearch.UseVisualStyleBackColor = false;
            // 
            // labelAlias
            // 
            labelAlias.AutoSize = true;
            labelAlias.Location = new Point(8, 80);
            labelAlias.Name = "labelAlias";
            labelAlias.Size = new Size(68, 15);
            labelAlias.TabIndex = 6;
            labelAlias.Text = "Alias name:";
            // 
            // textAliasName
            // 
            textAliasName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textAliasName.Location = new Point(10, 98);
            textAliasName.Name = "textAliasName";
            textAliasName.Size = new Size(613, 23);
            textAliasName.TabIndex = 2;
            // 
            // RepositoryEditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(639, 604);
            Controls.Add(buttonCancel);
            Controls.Add(buttonAccept);
            Controls.Add(panelForm);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RepositoryEditorForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Repository editor";
            FormClosing += RepositoryEditorForm_FormClosing;
            Load += RepositoryEditorForm_Load;
            KeyPress += RepositoryEditorForm_KeyPress;
            KeyUp += RepositoryEditorForm_KeyUp;
            panelForm.ResumeLayout(false);
            panelForm.PerformLayout();
            panelMSSqlServer.ResumeLayout(false);
            panelMSSqlServer.PerformLayout();
            panelSqLite.ResumeLayout(false);
            panelSqLite.PerformLayout();
            groupRepositoryType.ResumeLayout(false);
            groupRepositoryType.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Button buttonFolderSearch;
        private System.Windows.Forms.Label labelAlias;
        private System.Windows.Forms.TextBox textAliasName;
        private System.Windows.Forms.GroupBox groupRepositoryType;
        private System.Windows.Forms.RadioButton radioMSSqlServer;
        private System.Windows.Forms.RadioButton radioSqLite;
        private System.Windows.Forms.Panel panelMSSqlServer;
        private System.Windows.Forms.TextBox textSQLDataBase;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textSQLServer;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panelSqLite;
        private System.Windows.Forms.TextBox textSqLiteDataBase;
        private System.Windows.Forms.Label labelSqLiteDataBase;
        private System.Windows.Forms.TextBox textSqLiteDirectory;
        private System.Windows.Forms.Label labelDirectory;
        private System.Windows.Forms.Button buttonSelectDirectory;
        private System.Windows.Forms.Button buttonSelectFile;
        private System.Windows.Forms.TextBox textResourcesContainerUrl;
        private System.Windows.Forms.TextBox textResourcesContainerRoot;
        private System.Windows.Forms.TextBox textResourcesContainer;
        private System.Windows.Forms.Label labelContainerUrl;
        private System.Windows.Forms.Label labelContainerRoot;
        private System.Windows.Forms.Label labelContainer;
        private System.Windows.Forms.Button buttonSelectDirectoryResources;
        private System.Windows.Forms.CheckBox checkResourceContentInDB;
    }
}