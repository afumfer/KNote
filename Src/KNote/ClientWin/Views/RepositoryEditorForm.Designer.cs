
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
            buttonCancel.Location = new Point(804, 938);
            buttonCancel.Margin = new Padding(4, 5, 4, 5);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(91, 48);
            buttonCancel.TabIndex = 15;
            buttonCancel.Text = "&Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonAccept
            // 
            buttonAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAccept.Location = new Point(704, 938);
            buttonAccept.Margin = new Padding(4, 5, 4, 5);
            buttonAccept.Name = "buttonAccept";
            buttonAccept.Size = new Size(91, 48);
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
            panelForm.Margin = new Padding(4, 5, 4, 5);
            panelForm.Name = "panelForm";
            panelForm.Size = new Size(913, 928);
            panelForm.TabIndex = 3;
            // 
            // checkResourceContentInDB
            // 
            checkResourceContentInDB.AutoSize = true;
            checkResourceContentInDB.Location = new Point(449, 250);
            checkResourceContentInDB.Margin = new Padding(4, 5, 4, 5);
            checkResourceContentInDB.Name = "checkResourceContentInDB";
            checkResourceContentInDB.Size = new Size(418, 29);
            checkResourceContentInDB.TabIndex = 4;
            checkResourceContentInDB.Text = "Save a copy of the resource content in database";
            checkResourceContentInDB.UseVisualStyleBackColor = true;
            // 
            // buttonSelectDirectoryResources
            // 
            buttonSelectDirectoryResources.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSelectDirectoryResources.Location = new Point(853, 330);
            buttonSelectDirectoryResources.Margin = new Padding(4, 5, 4, 5);
            buttonSelectDirectoryResources.Name = "buttonSelectDirectoryResources";
            buttonSelectDirectoryResources.Size = new Size(34, 38);
            buttonSelectDirectoryResources.TabIndex = 6;
            buttonSelectDirectoryResources.Text = "...";
            buttonSelectDirectoryResources.UseVisualStyleBackColor = true;
            buttonSelectDirectoryResources.Click += buttonSelectDirectoryResources_Click;
            // 
            // textResourcesContainer
            // 
            textResourcesContainer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textResourcesContainer.Location = new Point(14, 247);
            textResourcesContainer.Margin = new Padding(4, 5, 4, 5);
            textResourcesContainer.Name = "textResourcesContainer";
            textResourcesContainer.Size = new Size(394, 31);
            textResourcesContainer.TabIndex = 3;
            // 
            // textResourcesContainerUrl
            // 
            textResourcesContainerUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textResourcesContainerUrl.Location = new Point(11, 413);
            textResourcesContainerUrl.Margin = new Padding(4, 5, 4, 5);
            textResourcesContainerUrl.Name = "textResourcesContainerUrl";
            textResourcesContainerUrl.Size = new Size(874, 31);
            textResourcesContainerUrl.TabIndex = 7;
            // 
            // textResourcesContainerRoot
            // 
            textResourcesContainerRoot.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textResourcesContainerRoot.Location = new Point(11, 330);
            textResourcesContainerRoot.Margin = new Padding(4, 5, 4, 5);
            textResourcesContainerRoot.Name = "textResourcesContainerRoot";
            textResourcesContainerRoot.Size = new Size(831, 31);
            textResourcesContainerRoot.TabIndex = 5;
            // 
            // labelContainerUrl
            // 
            labelContainerUrl.AutoSize = true;
            labelContainerUrl.Location = new Point(11, 383);
            labelContainerUrl.Margin = new Padding(4, 0, 4, 0);
            labelContainerUrl.Name = "labelContainerUrl";
            labelContainerUrl.Size = new Size(276, 25);
            labelContainerUrl.TabIndex = 17;
            labelContainerUrl.Text = "Resources container root file URL:";
            // 
            // labelContainerRoot
            // 
            labelContainerRoot.AutoSize = true;
            labelContainerRoot.Location = new Point(11, 300);
            labelContainerRoot.Margin = new Padding(4, 0, 4, 0);
            labelContainerRoot.Name = "labelContainerRoot";
            labelContainerRoot.Size = new Size(264, 25);
            labelContainerRoot.TabIndex = 16;
            labelContainerRoot.Text = "Resources container root folder:";
            // 
            // labelContainer
            // 
            labelContainer.AutoSize = true;
            labelContainer.Location = new Point(11, 217);
            labelContainer.Margin = new Padding(4, 0, 4, 0);
            labelContainer.Name = "labelContainer";
            labelContainer.Size = new Size(222, 25);
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
            panelMSSqlServer.Location = new Point(9, 673);
            panelMSSqlServer.Margin = new Padding(4, 5, 4, 5);
            panelMSSqlServer.Name = "panelMSSqlServer";
            panelMSSqlServer.Size = new Size(891, 194);
            panelMSSqlServer.TabIndex = 14;
            // 
            // textSQLDataBase
            // 
            textSQLDataBase.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textSQLDataBase.Location = new Point(1, 138);
            textSQLDataBase.Margin = new Padding(4, 5, 4, 5);
            textSQLDataBase.Name = "textSQLDataBase";
            textSQLDataBase.Size = new Size(874, 31);
            textSQLDataBase.TabIndex = 13;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(1, 108);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(95, 25);
            label1.TabIndex = 11;
            label1.Text = "Data base:";
            // 
            // textSQLServer
            // 
            textSQLServer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textSQLServer.Location = new Point(1, 47);
            textSQLServer.Margin = new Padding(4, 5, 4, 5);
            textSQLServer.Name = "textSQLServer";
            textSQLServer.Size = new Size(874, 31);
            textSQLServer.TabIndex = 12;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(1, 17);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(173, 25);
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
            panelSqLite.Location = new Point(9, 462);
            panelSqLite.Margin = new Padding(4, 5, 4, 5);
            panelSqLite.Name = "panelSqLite";
            panelSqLite.Size = new Size(891, 200);
            panelSqLite.TabIndex = 13;
            // 
            // buttonSelectFile
            // 
            buttonSelectFile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSelectFile.Location = new Point(843, 137);
            buttonSelectFile.Margin = new Padding(4, 5, 4, 5);
            buttonSelectFile.Name = "buttonSelectFile";
            buttonSelectFile.Size = new Size(34, 38);
            buttonSelectFile.TabIndex = 11;
            buttonSelectFile.Text = "...";
            buttonSelectFile.UseVisualStyleBackColor = true;
            buttonSelectFile.Visible = false;
            buttonSelectFile.Click += buttonSelectFile_Click;
            // 
            // buttonSelectDirectory
            // 
            buttonSelectDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSelectDirectory.Location = new Point(843, 47);
            buttonSelectDirectory.Margin = new Padding(4, 5, 4, 5);
            buttonSelectDirectory.Name = "buttonSelectDirectory";
            buttonSelectDirectory.Size = new Size(34, 38);
            buttonSelectDirectory.TabIndex = 9;
            buttonSelectDirectory.Text = "...";
            buttonSelectDirectory.UseVisualStyleBackColor = true;
            buttonSelectDirectory.Click += buttonSelectDirectory_Click;
            // 
            // textSqLiteDataBase
            // 
            textSqLiteDataBase.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textSqLiteDataBase.Location = new Point(1, 138);
            textSqLiteDataBase.Margin = new Padding(4, 5, 4, 5);
            textSqLiteDataBase.Name = "textSqLiteDataBase";
            textSqLiteDataBase.Size = new Size(874, 31);
            textSqLiteDataBase.TabIndex = 10;
            // 
            // labelSqLiteDataBase
            // 
            labelSqLiteDataBase.AutoSize = true;
            labelSqLiteDataBase.Location = new Point(1, 108);
            labelSqLiteDataBase.Margin = new Padding(4, 0, 4, 0);
            labelSqLiteDataBase.Name = "labelSqLiteDataBase";
            labelSqLiteDataBase.Size = new Size(123, 25);
            labelSqLiteDataBase.TabIndex = 10;
            labelSqLiteDataBase.Text = "Data base file:";
            // 
            // textSqLiteDirectory
            // 
            textSqLiteDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textSqLiteDirectory.Location = new Point(1, 47);
            textSqLiteDirectory.Margin = new Padding(4, 5, 4, 5);
            textSqLiteDirectory.Name = "textSqLiteDirectory";
            textSqLiteDirectory.Size = new Size(831, 31);
            textSqLiteDirectory.TabIndex = 8;
            // 
            // labelDirectory
            // 
            labelDirectory.AutoSize = true;
            labelDirectory.Location = new Point(1, 17);
            labelDirectory.Margin = new Padding(4, 0, 4, 0);
            labelDirectory.Name = "labelDirectory";
            labelDirectory.Size = new Size(165, 25);
            labelDirectory.TabIndex = 8;
            labelDirectory.Text = "Database directory:";
            // 
            // groupRepositoryType
            // 
            groupRepositoryType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupRepositoryType.Controls.Add(radioMSSqlServer);
            groupRepositoryType.Controls.Add(radioSqLite);
            groupRepositoryType.Location = new Point(11, 12);
            groupRepositoryType.Margin = new Padding(4, 5, 4, 5);
            groupRepositoryType.Name = "groupRepositoryType";
            groupRepositoryType.Padding = new Padding(4, 5, 4, 5);
            groupRepositoryType.Size = new Size(879, 102);
            groupRepositoryType.TabIndex = 12;
            groupRepositoryType.TabStop = false;
            groupRepositoryType.Text = "Reposoty database type";
            // 
            // radioMSSqlServer
            // 
            radioMSSqlServer.AutoSize = true;
            radioMSSqlServer.Location = new Point(223, 37);
            radioMSSqlServer.Margin = new Padding(4, 5, 4, 5);
            radioMSSqlServer.Name = "radioMSSqlServer";
            radioMSSqlServer.Size = new Size(204, 29);
            radioMSSqlServer.TabIndex = 1;
            radioMSSqlServer.TabStop = true;
            radioMSSqlServer.Text = "Microsoft SQL Server";
            radioMSSqlServer.UseVisualStyleBackColor = true;
            radioMSSqlServer.CheckedChanged += radioDataBase_CheckedChanged;
            // 
            // radioSqLite
            // 
            radioSqLite.AutoSize = true;
            radioSqLite.Location = new Point(50, 37);
            radioSqLite.Margin = new Padding(4, 5, 4, 5);
            radioSqLite.Name = "radioSqLite";
            radioSqLite.Size = new Size(85, 29);
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
            buttonFolderSearch.Location = new Point(1284, 332);
            buttonFolderSearch.Margin = new Padding(6, 5, 6, 5);
            buttonFolderSearch.Name = "buttonFolderSearch";
            buttonFolderSearch.Size = new Size(39, 42);
            buttonFolderSearch.TabIndex = 11;
            buttonFolderSearch.Text = "...";
            buttonFolderSearch.UseVisualStyleBackColor = false;
            // 
            // labelAlias
            // 
            labelAlias.AutoSize = true;
            labelAlias.Location = new Point(11, 133);
            labelAlias.Margin = new Padding(4, 0, 4, 0);
            labelAlias.Name = "labelAlias";
            labelAlias.Size = new Size(102, 25);
            labelAlias.TabIndex = 6;
            labelAlias.Text = "Alias name:";
            // 
            // textAliasName
            // 
            textAliasName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textAliasName.Location = new Point(14, 163);
            textAliasName.Margin = new Padding(4, 5, 4, 5);
            textAliasName.Name = "textAliasName";
            textAliasName.Size = new Size(874, 31);
            textAliasName.TabIndex = 2;
            // 
            // RepositoryEditorForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(913, 1007);
            Controls.Add(buttonCancel);
            Controls.Add(buttonAccept);
            Controls.Add(panelForm);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new Padding(4, 5, 4, 5);
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