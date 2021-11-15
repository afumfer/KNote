
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.panelForm = new System.Windows.Forms.Panel();
            this.buttonSelectDirectoryResources = new System.Windows.Forms.Button();
            this.textResourcesContainer = new System.Windows.Forms.TextBox();
            this.textResourcesContainerUrl = new System.Windows.Forms.TextBox();
            this.textResourcesContainerRoot = new System.Windows.Forms.TextBox();
            this.labelContainerUrl = new System.Windows.Forms.Label();
            this.labelContainerRoot = new System.Windows.Forms.Label();
            this.labelContainer = new System.Windows.Forms.Label();
            this.panelMSSqlServer = new System.Windows.Forms.Panel();
            this.textSQLDataBase = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textSQLServer = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panelSqLite = new System.Windows.Forms.Panel();
            this.buttonSelectFile = new System.Windows.Forms.Button();
            this.buttonSelectDirectory = new System.Windows.Forms.Button();
            this.textSqLiteDataBase = new System.Windows.Forms.TextBox();
            this.labelSqLiteDataBase = new System.Windows.Forms.Label();
            this.textSqLiteDirectory = new System.Windows.Forms.TextBox();
            this.labelDirectory = new System.Windows.Forms.Label();
            this.groupRepositoryType = new System.Windows.Forms.GroupBox();
            this.radioMSSqlServer = new System.Windows.Forms.RadioButton();
            this.radioSqLite = new System.Windows.Forms.RadioButton();
            this.buttonFolderSearch = new System.Windows.Forms.Button();
            this.labelAlias = new System.Windows.Forms.Label();
            this.textAliasName = new System.Windows.Forms.TextBox();
            this.checkResourceContentInDB = new System.Windows.Forms.CheckBox();
            this.panelForm.SuspendLayout();
            this.panelMSSqlServer.SuspendLayout();
            this.panelSqLite.SuspendLayout();
            this.groupRepositoryType.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(563, 563);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(64, 29);
            this.buttonCancel.TabIndex = 15;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonAccept
            // 
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(493, 563);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(64, 29);
            this.buttonAccept.TabIndex = 14;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // panelForm
            // 
            this.panelForm.Controls.Add(this.checkResourceContentInDB);
            this.panelForm.Controls.Add(this.buttonSelectDirectoryResources);
            this.panelForm.Controls.Add(this.textResourcesContainer);
            this.panelForm.Controls.Add(this.textResourcesContainerUrl);
            this.panelForm.Controls.Add(this.textResourcesContainerRoot);
            this.panelForm.Controls.Add(this.labelContainerUrl);
            this.panelForm.Controls.Add(this.labelContainerRoot);
            this.panelForm.Controls.Add(this.labelContainer);
            this.panelForm.Controls.Add(this.panelMSSqlServer);
            this.panelForm.Controls.Add(this.panelSqLite);
            this.panelForm.Controls.Add(this.groupRepositoryType);
            this.panelForm.Controls.Add(this.buttonFolderSearch);
            this.panelForm.Controls.Add(this.labelAlias);
            this.panelForm.Controls.Add(this.textAliasName);
            this.panelForm.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelForm.Location = new System.Drawing.Point(0, 0);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(639, 567);
            this.panelForm.TabIndex = 3;
            // 
            // buttonSelectDirectoryResources
            // 
            this.buttonSelectDirectoryResources.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectDirectoryResources.Location = new System.Drawing.Point(597, 198);
            this.buttonSelectDirectoryResources.Name = "buttonSelectDirectoryResources";
            this.buttonSelectDirectoryResources.Size = new System.Drawing.Size(24, 23);
            this.buttonSelectDirectoryResources.TabIndex = 6;
            this.buttonSelectDirectoryResources.Text = "...";
            this.buttonSelectDirectoryResources.UseVisualStyleBackColor = true;
            this.buttonSelectDirectoryResources.Click += new System.EventHandler(this.buttonSelectDirectoryResources_Click);
            // 
            // textResourcesContainer
            // 
            this.textResourcesContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textResourcesContainer.Location = new System.Drawing.Point(10, 148);
            this.textResourcesContainer.Name = "textResourcesContainer";
            this.textResourcesContainer.Size = new System.Drawing.Size(277, 23);
            this.textResourcesContainer.TabIndex = 3;
            // 
            // textResourcesContainerUrl
            // 
            this.textResourcesContainerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textResourcesContainerUrl.Location = new System.Drawing.Point(8, 248);
            this.textResourcesContainerUrl.Name = "textResourcesContainerUrl";
            this.textResourcesContainerUrl.Size = new System.Drawing.Size(613, 23);
            this.textResourcesContainerUrl.TabIndex = 7;
            // 
            // textResourcesContainerRoot
            // 
            this.textResourcesContainerRoot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textResourcesContainerRoot.Location = new System.Drawing.Point(8, 198);
            this.textResourcesContainerRoot.Name = "textResourcesContainerRoot";
            this.textResourcesContainerRoot.Size = new System.Drawing.Size(583, 23);
            this.textResourcesContainerRoot.TabIndex = 5;
            // 
            // labelContainerUrl
            // 
            this.labelContainerUrl.AutoSize = true;
            this.labelContainerUrl.Location = new System.Drawing.Point(8, 230);
            this.labelContainerUrl.Name = "labelContainerUrl";
            this.labelContainerUrl.Size = new System.Drawing.Size(140, 15);
            this.labelContainerUrl.TabIndex = 17;
            this.labelContainerUrl.Text = "Resources container URL:";
            // 
            // labelContainerRoot
            // 
            this.labelContainerRoot.AutoSize = true;
            this.labelContainerRoot.Location = new System.Drawing.Point(8, 180);
            this.labelContainerRoot.Name = "labelContainerRoot";
            this.labelContainerRoot.Size = new System.Drawing.Size(175, 15);
            this.labelContainerRoot.TabIndex = 16;
            this.labelContainerRoot.Text = "Resources container root folder:";
            // 
            // labelContainer
            // 
            this.labelContainer.AutoSize = true;
            this.labelContainer.Location = new System.Drawing.Point(8, 130);
            this.labelContainer.Name = "labelContainer";
            this.labelContainer.Size = new System.Drawing.Size(149, 15);
            this.labelContainer.TabIndex = 15;
            this.labelContainer.Text = "Resources container name:";
            // 
            // panelMSSqlServer
            // 
            this.panelMSSqlServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMSSqlServer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMSSqlServer.Controls.Add(this.textSQLDataBase);
            this.panelMSSqlServer.Controls.Add(this.label1);
            this.panelMSSqlServer.Controls.Add(this.textSQLServer);
            this.panelMSSqlServer.Controls.Add(this.label5);
            this.panelMSSqlServer.Location = new System.Drawing.Point(6, 404);
            this.panelMSSqlServer.Name = "panelMSSqlServer";
            this.panelMSSqlServer.Size = new System.Drawing.Size(624, 117);
            this.panelMSSqlServer.TabIndex = 14;
            // 
            // textSQLDataBase
            // 
            this.textSQLDataBase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textSQLDataBase.Location = new System.Drawing.Point(1, 83);
            this.textSQLDataBase.Name = "textSQLDataBase";
            this.textSQLDataBase.Size = new System.Drawing.Size(613, 23);
            this.textSQLDataBase.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 15);
            this.label1.TabIndex = 11;
            this.label1.Text = "Data base:";
            // 
            // textSQLServer
            // 
            this.textSQLServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textSQLServer.Location = new System.Drawing.Point(1, 28);
            this.textSQLServer.Name = "textSQLServer";
            this.textSQLServer.Size = new System.Drawing.Size(613, 23);
            this.textSQLServer.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(1, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(115, 15);
            this.label5.TabIndex = 8;
            this.label5.Text = "SQL Server\\instance:";
            // 
            // panelSqLite
            // 
            this.panelSqLite.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelSqLite.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSqLite.Controls.Add(this.buttonSelectFile);
            this.panelSqLite.Controls.Add(this.buttonSelectDirectory);
            this.panelSqLite.Controls.Add(this.textSqLiteDataBase);
            this.panelSqLite.Controls.Add(this.labelSqLiteDataBase);
            this.panelSqLite.Controls.Add(this.textSqLiteDirectory);
            this.panelSqLite.Controls.Add(this.labelDirectory);
            this.panelSqLite.Location = new System.Drawing.Point(6, 277);
            this.panelSqLite.Name = "panelSqLite";
            this.panelSqLite.Size = new System.Drawing.Size(624, 121);
            this.panelSqLite.TabIndex = 13;
            // 
            // buttonSelectFile
            // 
            this.buttonSelectFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectFile.Location = new System.Drawing.Point(590, 82);
            this.buttonSelectFile.Name = "buttonSelectFile";
            this.buttonSelectFile.Size = new System.Drawing.Size(24, 23);
            this.buttonSelectFile.TabIndex = 11;
            this.buttonSelectFile.Text = "...";
            this.buttonSelectFile.UseVisualStyleBackColor = true;
            this.buttonSelectFile.Visible = false;
            this.buttonSelectFile.Click += new System.EventHandler(this.buttonSelectFile_Click);
            // 
            // buttonSelectDirectory
            // 
            this.buttonSelectDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectDirectory.Location = new System.Drawing.Point(590, 28);
            this.buttonSelectDirectory.Name = "buttonSelectDirectory";
            this.buttonSelectDirectory.Size = new System.Drawing.Size(24, 23);
            this.buttonSelectDirectory.TabIndex = 9;
            this.buttonSelectDirectory.Text = "...";
            this.buttonSelectDirectory.UseVisualStyleBackColor = true;
            this.buttonSelectDirectory.Click += new System.EventHandler(this.buttonSelectDirectory_Click);
            // 
            // textSqLiteDataBase
            // 
            this.textSqLiteDataBase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textSqLiteDataBase.Location = new System.Drawing.Point(1, 83);
            this.textSqLiteDataBase.Name = "textSqLiteDataBase";
            this.textSqLiteDataBase.Size = new System.Drawing.Size(613, 23);
            this.textSqLiteDataBase.TabIndex = 10;
            // 
            // labelSqLiteDataBase
            // 
            this.labelSqLiteDataBase.AutoSize = true;
            this.labelSqLiteDataBase.Location = new System.Drawing.Point(1, 65);
            this.labelSqLiteDataBase.Name = "labelSqLiteDataBase";
            this.labelSqLiteDataBase.Size = new System.Drawing.Size(80, 15);
            this.labelSqLiteDataBase.TabIndex = 10;
            this.labelSqLiteDataBase.Text = "Data base file:";
            // 
            // textSqLiteDirectory
            // 
            this.textSqLiteDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textSqLiteDirectory.Location = new System.Drawing.Point(1, 28);
            this.textSqLiteDirectory.Name = "textSqLiteDirectory";
            this.textSqLiteDirectory.Size = new System.Drawing.Size(583, 23);
            this.textSqLiteDirectory.TabIndex = 8;
            // 
            // labelDirectory
            // 
            this.labelDirectory.AutoSize = true;
            this.labelDirectory.Location = new System.Drawing.Point(1, 10);
            this.labelDirectory.Name = "labelDirectory";
            this.labelDirectory.Size = new System.Drawing.Size(108, 15);
            this.labelDirectory.TabIndex = 8;
            this.labelDirectory.Text = "Database directory:";
            // 
            // groupRepositoryType
            // 
            this.groupRepositoryType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupRepositoryType.Controls.Add(this.radioMSSqlServer);
            this.groupRepositoryType.Controls.Add(this.radioSqLite);
            this.groupRepositoryType.Location = new System.Drawing.Point(8, 7);
            this.groupRepositoryType.Name = "groupRepositoryType";
            this.groupRepositoryType.Size = new System.Drawing.Size(615, 61);
            this.groupRepositoryType.TabIndex = 12;
            this.groupRepositoryType.TabStop = false;
            this.groupRepositoryType.Text = "Reposoty database type";
            // 
            // radioMSSqlServer
            // 
            this.radioMSSqlServer.AutoSize = true;
            this.radioMSSqlServer.Location = new System.Drawing.Point(156, 22);
            this.radioMSSqlServer.Name = "radioMSSqlServer";
            this.radioMSSqlServer.Size = new System.Drawing.Size(135, 19);
            this.radioMSSqlServer.TabIndex = 1;
            this.radioMSSqlServer.TabStop = true;
            this.radioMSSqlServer.Text = "Microsoft SQL Server";
            this.radioMSSqlServer.UseVisualStyleBackColor = true;
            this.radioMSSqlServer.CheckedChanged += new System.EventHandler(this.radioDataBase_CheckedChanged);
            // 
            // radioSqLite
            // 
            this.radioSqLite.AutoSize = true;
            this.radioSqLite.Location = new System.Drawing.Point(35, 22);
            this.radioSqLite.Name = "radioSqLite";
            this.radioSqLite.Size = new System.Drawing.Size(57, 19);
            this.radioSqLite.TabIndex = 0;
            this.radioSqLite.TabStop = true;
            this.radioSqLite.Text = "SqLite";
            this.radioSqLite.UseVisualStyleBackColor = true;
            this.radioSqLite.CheckedChanged += new System.EventHandler(this.radioDataBase_CheckedChanged);
            // 
            // buttonFolderSearch
            // 
            this.buttonFolderSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFolderSearch.BackColor = System.Drawing.SystemColors.Control;
            this.buttonFolderSearch.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonFolderSearch.Location = new System.Drawing.Point(899, 199);
            this.buttonFolderSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonFolderSearch.Name = "buttonFolderSearch";
            this.buttonFolderSearch.Size = new System.Drawing.Size(27, 25);
            this.buttonFolderSearch.TabIndex = 11;
            this.buttonFolderSearch.Text = "...";
            this.buttonFolderSearch.UseVisualStyleBackColor = false;
            // 
            // labelAlias
            // 
            this.labelAlias.AutoSize = true;
            this.labelAlias.Location = new System.Drawing.Point(8, 80);
            this.labelAlias.Name = "labelAlias";
            this.labelAlias.Size = new System.Drawing.Size(68, 15);
            this.labelAlias.TabIndex = 6;
            this.labelAlias.Text = "Alias name:";
            // 
            // textAliasName
            // 
            this.textAliasName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textAliasName.Location = new System.Drawing.Point(10, 98);
            this.textAliasName.Name = "textAliasName";
            this.textAliasName.Size = new System.Drawing.Size(613, 23);
            this.textAliasName.TabIndex = 2;
            // 
            // checkResourceContentInDB
            // 
            this.checkResourceContentInDB.AutoSize = true;
            this.checkResourceContentInDB.Location = new System.Drawing.Point(314, 150);
            this.checkResourceContentInDB.Name = "checkResourceContentInDB";
            this.checkResourceContentInDB.Size = new System.Drawing.Size(277, 19);
            this.checkResourceContentInDB.TabIndex = 4;
            this.checkResourceContentInDB.Text = "Save a copy of the resource content in database";
            this.checkResourceContentInDB.UseVisualStyleBackColor = true;
            // 
            // RepositoryEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 604);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.panelForm);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RepositoryEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Repository editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RepositoryEditorForm_FormClosing);
            this.Load += new System.EventHandler(this.RepositoryEditorForm_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RepositoryEditorForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RepositoryEditorForm_KeyUp);
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.panelMSSqlServer.ResumeLayout(false);
            this.panelMSSqlServer.PerformLayout();
            this.panelSqLite.ResumeLayout(false);
            this.panelSqLite.PerformLayout();
            this.groupRepositoryType.ResumeLayout(false);
            this.groupRepositoryType.PerformLayout();
            this.ResumeLayout(false);

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