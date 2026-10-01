
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
            toolTipAdminTabs = new ToolTip();
            tabControlMain = new TabControl();
            tabPageGeneral = new TabPage();
            tabPageUsers = new TabPage();
            tabPageNoteTypes = new TabPage();
            tabPageTraceNoteTypes = new TabPage();
            tabPageAttributes = new TabPage();
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
            labelAlias = new Label();
            textAliasName = new TextBox();
            tabControlMain.SuspendLayout();
            tabPageGeneral.SuspendLayout();
            tabPageUsers.SuspendLayout();
            tabPageNoteTypes.SuspendLayout();
            tabPageTraceNoteTypes.SuspendLayout();
            tabPageAttributes.SuspendLayout();
            panelForm.SuspendLayout();
            panelMSSqlServer.SuspendLayout();
            panelSqLite.SuspendLayout();
            groupRepositoryType.SuspendLayout();
            SuspendLayout();
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Location = new Point(563, 597);
            buttonCancel.Margin = new Padding(3);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(64, 29);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = "&Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonAccept
            // 
            buttonAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAccept.Location = new Point(493, 597);
            buttonAccept.Margin = new Padding(3);
            buttonAccept.Name = "buttonAccept";
            buttonAccept.Size = new Size(64, 29);
            buttonAccept.TabIndex = 1;
            buttonAccept.Text = "&Accept";
            buttonAccept.UseVisualStyleBackColor = true;
            buttonAccept.Click += buttonAccept_Click;
            //
            // tabControlMain
            //
            tabControlMain.Controls.Add(tabPageGeneral);
            tabControlMain.Controls.Add(tabPageUsers);
            tabControlMain.Controls.Add(tabPageNoteTypes);
            tabControlMain.Controls.Add(tabPageTraceNoteTypes);
            tabControlMain.Controls.Add(tabPageAttributes);
            tabControlMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControlMain.Location = new Point(3, 2);
            tabControlMain.Margin = new Padding(3);
            tabControlMain.Name = "tabControlMain";
            tabControlMain.SelectedIndex = 0;
            tabControlMain.Size = new Size(634, 582);
            tabControlMain.TabIndex = 0;
            //
            // tabPageGeneral
            //
            tabPageGeneral.Controls.Add(panelForm);
            tabPageGeneral.Location = new Point(3, 20);
            tabPageGeneral.Margin = new Padding(3);
            tabPageGeneral.Name = "tabPageGeneral";
            tabPageGeneral.Padding = new Padding(3);
            tabPageGeneral.Size = new Size(634, 564);
            tabPageGeneral.TabIndex = 0;
            tabPageGeneral.Text = "General";
            tabPageGeneral.UseVisualStyleBackColor = true;
            //
            // tabPageUsers
            //
            tabPageUsers.Location = new Point(3, 20);
            tabPageUsers.Margin = new Padding(3);
            tabPageUsers.Name = "tabPageUsers";
            tabPageUsers.Padding = new Padding(3);
            tabPageUsers.Size = new Size(634, 564);
            tabPageUsers.TabIndex = 1;
            tabPageUsers.Text = "Users";
            tabPageUsers.UseVisualStyleBackColor = true;
            //
            // tabPageNoteTypes
            //
            tabPageNoteTypes.Location = new Point(3, 20);
            tabPageNoteTypes.Margin = new Padding(3);
            tabPageNoteTypes.Name = "tabPageNoteTypes";
            tabPageNoteTypes.Padding = new Padding(3);
            tabPageNoteTypes.Size = new Size(634, 564);
            tabPageNoteTypes.TabIndex = 2;
            tabPageNoteTypes.Text = "Note types";
            tabPageNoteTypes.UseVisualStyleBackColor = true;
            //
            // tabPageTraceNoteTypes
            //
            tabPageTraceNoteTypes.Location = new Point(3, 20);
            tabPageTraceNoteTypes.Margin = new Padding(3);
            tabPageTraceNoteTypes.Name = "tabPageTraceNoteTypes";
            tabPageTraceNoteTypes.Padding = new Padding(3);
            tabPageTraceNoteTypes.Size = new Size(634, 564);
            tabPageTraceNoteTypes.TabIndex = 3;
            tabPageTraceNoteTypes.Text = "Trace note types";
            tabPageTraceNoteTypes.UseVisualStyleBackColor = true;
            //
            // tabPageAttributes
            //
            tabPageAttributes.Location = new Point(3, 20);
            tabPageAttributes.Margin = new Padding(3);
            tabPageAttributes.Name = "tabPageAttributes";
            tabPageAttributes.Padding = new Padding(3);
            tabPageAttributes.Size = new Size(634, 564);
            tabPageAttributes.TabIndex = 4;
            tabPageAttributes.Text = "Attributes";
            tabPageAttributes.UseVisualStyleBackColor = true;
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
            panelForm.Controls.Add(labelAlias);
            panelForm.Controls.Add(textAliasName);
            panelForm.Dock = DockStyle.Top;
            panelForm.Location = new Point(0, 0);
            panelForm.Margin = new Padding(3);
            panelForm.Name = "panelForm";
            panelForm.Size = new Size(639, 557);
            panelForm.TabIndex = 3;
            // 
            // checkResourceContentInDB
            // 
            checkResourceContentInDB.AutoSize = true;
            checkResourceContentInDB.Location = new Point(314, 150);
            checkResourceContentInDB.Margin = new Padding(3);
            checkResourceContentInDB.Name = "checkResourceContentInDB";
            checkResourceContentInDB.Size = new Size(293, 17);
            checkResourceContentInDB.TabIndex = 5;
            checkResourceContentInDB.Text = "Save a copy of the resource content in database";
            checkResourceContentInDB.UseVisualStyleBackColor = true;
            // 
            // buttonSelectDirectoryResources
            // 
            buttonSelectDirectoryResources.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSelectDirectoryResources.Location = new Point(595, 196);
            buttonSelectDirectoryResources.Margin = new Padding(3);
            buttonSelectDirectoryResources.Name = "buttonSelectDirectoryResources";
            buttonSelectDirectoryResources.Size = new Size(24, 23);
            buttonSelectDirectoryResources.TabIndex = 8;
            buttonSelectDirectoryResources.Text = "...";
            buttonSelectDirectoryResources.UseVisualStyleBackColor = true;
            buttonSelectDirectoryResources.Click += buttonSelectDirectoryResources_Click;
            // 
            // textResourcesContainer
            // 
            textResourcesContainer.Location = new Point(10, 148);
            textResourcesContainer.Margin = new Padding(3);
            textResourcesContainer.Name = "textResourcesContainer";
            textResourcesContainer.Size = new Size(287, 19);
            textResourcesContainer.TabIndex = 4;
            // 
            // textResourcesContainerUrl
            // 
            textResourcesContainerUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textResourcesContainerUrl.Location = new Point(8, 248);
            textResourcesContainerUrl.Margin = new Padding(3);
            textResourcesContainerUrl.Name = "textResourcesContainerUrl";
            textResourcesContainerUrl.Size = new Size(612, 19);
            textResourcesContainerUrl.TabIndex = 10;
            // 
            // textResourcesContainerRoot
            // 
            textResourcesContainerRoot.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textResourcesContainerRoot.Location = new Point(8, 198);
            textResourcesContainerRoot.Margin = new Padding(3);
            textResourcesContainerRoot.Name = "textResourcesContainerRoot";
            textResourcesContainerRoot.Size = new Size(582, 19);
            textResourcesContainerRoot.TabIndex = 7;
            // 
            // labelContainerUrl
            // 
            labelContainerUrl.AutoSize = true;
            labelContainerUrl.Location = new Point(8, 230);
            labelContainerUrl.Margin = new Padding(3, 0, 3, 0);
            labelContainerUrl.Name = "labelContainerUrl";
            labelContainerUrl.Size = new Size(193, 15);
            labelContainerUrl.TabIndex = 9;
            labelContainerUrl.Text = "Resources container root file URL:";
            // 
            // labelContainerRoot
            // 
            labelContainerRoot.AutoSize = true;
            labelContainerRoot.Location = new Point(8, 180);
            labelContainerRoot.Margin = new Padding(3, 0, 3, 0);
            labelContainerRoot.Name = "labelContainerRoot";
            labelContainerRoot.Size = new Size(185, 15);
            labelContainerRoot.TabIndex = 6;
            labelContainerRoot.Text = "Resources container root folder:";
            // 
            // labelContainer
            // 
            labelContainer.AutoSize = true;
            labelContainer.Location = new Point(8, 130);
            labelContainer.Margin = new Padding(3, 0, 3, 0);
            labelContainer.Name = "labelContainer";
            labelContainer.Size = new Size(155, 15);
            labelContainer.TabIndex = 3;
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
            panelMSSqlServer.Location = new Point(5, 403);
            panelMSSqlServer.Margin = new Padding(3);
            panelMSSqlServer.Name = "panelMSSqlServer";
            panelMSSqlServer.Size = new Size(624, 116);
            panelMSSqlServer.TabIndex = 12;
            // 
            // textSQLDataBase
            // 
            textSQLDataBase.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textSQLDataBase.Location = new Point(3, 83);
            textSQLDataBase.Margin = new Padding(3);
            textSQLDataBase.Name = "textSQLDataBase";
            textSQLDataBase.Size = new Size(610, 19);
            textSQLDataBase.TabIndex = 13;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(1, 65);
            label1.Margin = new Padding(3, 0, 3, 0);
            label1.Name = "label1";
            label1.Size = new Size(66, 15);
            label1.TabIndex = 11;
            label1.Text = "Data base:";
            // 
            // textSQLServer
            // 
            textSQLServer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textSQLServer.Location = new Point(3, 28);
            textSQLServer.Margin = new Padding(3);
            textSQLServer.Name = "textSQLServer";
            textSQLServer.Size = new Size(610, 19);
            textSQLServer.TabIndex = 12;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(1, 10);
            label5.Margin = new Padding(3, 0, 3, 0);
            label5.Name = "label5";
            label5.Size = new Size(121, 15);
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
            panelSqLite.Location = new Point(5, 277);
            panelSqLite.Margin = new Padding(3);
            panelSqLite.Name = "panelSqLite";
            panelSqLite.Size = new Size(624, 120);
            panelSqLite.TabIndex = 11;
            // 
            // buttonSelectFile
            // 
            buttonSelectFile.Location = new Point(589, 80);
            buttonSelectFile.Margin = new Padding(3);
            buttonSelectFile.Name = "buttonSelectFile";
            buttonSelectFile.Size = new Size(24, 23);
            buttonSelectFile.TabIndex = 12;
            buttonSelectFile.Text = "...";
            buttonSelectFile.UseVisualStyleBackColor = true;
            buttonSelectFile.Click += buttonSelectFile_Click;
            // 
            // buttonSelectDirectory
            // 
            buttonSelectDirectory.Location = new Point(588, 26);
            buttonSelectDirectory.Margin = new Padding(3);
            buttonSelectDirectory.Name = "buttonSelectDirectory";
            buttonSelectDirectory.Size = new Size(24, 23);
            buttonSelectDirectory.TabIndex = 9;
            buttonSelectDirectory.Text = "...";
            buttonSelectDirectory.UseVisualStyleBackColor = true;
            buttonSelectDirectory.Click += buttonSelectDirectory_Click;
            // 
            // textSqLiteDataBase
            // 
            textSqLiteDataBase.Location = new Point(3, 83);
            textSqLiteDataBase.Margin = new Padding(3);
            textSqLiteDataBase.Name = "textSqLiteDataBase";
            textSqLiteDataBase.Size = new Size(580, 19);
            textSqLiteDataBase.TabIndex = 10;
            // 
            // labelSqLiteDataBase
            // 
            labelSqLiteDataBase.AutoSize = true;
            labelSqLiteDataBase.Location = new Point(1, 65);
            labelSqLiteDataBase.Margin = new Padding(3, 0, 3, 0);
            labelSqLiteDataBase.Name = "labelSqLiteDataBase";
            labelSqLiteDataBase.Size = new Size(86, 15);
            labelSqLiteDataBase.TabIndex = 10;
            labelSqLiteDataBase.Text = "Data base file:";
            // 
            // textSqLiteDirectory
            // 
            textSqLiteDirectory.Location = new Point(3, 28);
            textSqLiteDirectory.Margin = new Padding(3);
            textSqLiteDirectory.Name = "textSqLiteDirectory";
            textSqLiteDirectory.Size = new Size(580, 19);
            textSqLiteDirectory.TabIndex = 8;
            // 
            // labelDirectory
            // 
            labelDirectory.AutoSize = true;
            labelDirectory.Location = new Point(1, 10);
            labelDirectory.Margin = new Padding(3, 0, 3, 0);
            labelDirectory.Name = "labelDirectory";
            labelDirectory.Size = new Size(115, 15);
            labelDirectory.TabIndex = 8;
            labelDirectory.Text = "Database directory:";
            // 
            // groupRepositoryType
            // 
            groupRepositoryType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupRepositoryType.Controls.Add(radioMSSqlServer);
            groupRepositoryType.Controls.Add(radioSqLite);
            groupRepositoryType.Location = new Point(8, 7);
            groupRepositoryType.Margin = new Padding(3);
            groupRepositoryType.Name = "groupRepositoryType";
            groupRepositoryType.Padding = new Padding(3);
            groupRepositoryType.Size = new Size(615, 61);
            groupRepositoryType.TabIndex = 0;
            groupRepositoryType.TabStop = false;
            groupRepositoryType.Text = "Reposoty database type";
            // 
            // radioMSSqlServer
            // 
            radioMSSqlServer.AutoSize = true;
            radioMSSqlServer.Location = new Point(156, 22);
            radioMSSqlServer.Margin = new Padding(3);
            radioMSSqlServer.Name = "radioMSSqlServer";
            radioMSSqlServer.Size = new Size(143, 17);
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
            radioSqLite.Margin = new Padding(3);
            radioSqLite.Name = "radioSqLite";
            radioSqLite.Size = new Size(59, 17);
            radioSqLite.TabIndex = 0;
            radioSqLite.TabStop = true;
            radioSqLite.Text = "SqLite";
            radioSqLite.UseVisualStyleBackColor = true;
            radioSqLite.CheckedChanged += radioDataBase_CheckedChanged;
            // 
            // labelAlias
            // 
            labelAlias.AutoSize = true;
            labelAlias.Location = new Point(8, 80);
            labelAlias.Margin = new Padding(3, 0, 3, 0);
            labelAlias.Name = "labelAlias";
            labelAlias.Size = new Size(71, 15);
            labelAlias.TabIndex = 1;
            labelAlias.Text = "Alias name:";
            // 
            // textAliasName
            // 
            textAliasName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textAliasName.Location = new Point(10, 98);
            textAliasName.Margin = new Padding(3);
            textAliasName.Name = "textAliasName";
            textAliasName.Size = new Size(612, 19);
            textAliasName.TabIndex = 2;
            // 
            // RepositoryEditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(639, 634);
            Controls.Add(buttonCancel);
            Controls.Add(buttonAccept);
            Controls.Add(tabControlMain);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new Padding(3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RepositoryEditorForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Repository editor";
            Load += RepositoryEditorForm_Load;
            tabControlMain.ResumeLayout(false);
            tabPageGeneral.ResumeLayout(false);
            tabPageUsers.ResumeLayout(false);
            tabPageUsers.PerformLayout();
            tabPageNoteTypes.ResumeLayout(false);
            tabPageNoteTypes.PerformLayout();
            tabPageTraceNoteTypes.ResumeLayout(false);
            tabPageTraceNoteTypes.PerformLayout();
            tabPageAttributes.ResumeLayout(false);
            tabPageAttributes.PerformLayout();
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
        private System.Windows.Forms.ToolTip toolTipAdminTabs;
        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageGeneral;
        private System.Windows.Forms.TabPage tabPageUsers;
        private System.Windows.Forms.TabPage tabPageNoteTypes;
        private System.Windows.Forms.TabPage tabPageTraceNoteTypes;
        private System.Windows.Forms.TabPage tabPageAttributes;
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
        private System.Windows.Forms.TextBox textResourcesContainerUrl;
        private System.Windows.Forms.TextBox textResourcesContainerRoot;
        private System.Windows.Forms.TextBox textResourcesContainer;
        private System.Windows.Forms.Label labelContainerUrl;
        private System.Windows.Forms.Label labelContainerRoot;
        private System.Windows.Forms.Label labelContainer;
        private System.Windows.Forms.Button buttonSelectDirectoryResources;
        private System.Windows.Forms.CheckBox checkResourceContentInDB;
        private Button buttonSelectFile;
    }
}