
namespace KNote.ClientWin.Views
{
    partial class NotesFilterParamForm
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
            this.panelForm = new System.Windows.Forms.Panel();
            this.listViewAttributes = new System.Windows.Forms.ListView();
            this.columnAttribute = new System.Windows.Forms.ColumnHeader();
            this.columnValue = new System.Windows.Forms.ColumnHeader();
            this.buttonRemoveAttribute = new System.Windows.Forms.Button();
            this.buttonAddAttribute = new System.Windows.Forms.Button();
            this.labelAttributes = new System.Windows.Forms.Label();
            this.buttonFolderSelect = new System.Windows.Forms.Button();
            this.buttonFolderClear = new System.Windows.Forms.Button();
            this.textFolder = new System.Windows.Forms.TextBox();
            this.labelFolder = new System.Windows.Forms.Label();
            this.comboNoteType = new System.Windows.Forms.ComboBox();
            this.labelNoteType = new System.Windows.Forms.Label();
            this.textTags = new System.Windows.Forms.TextBox();
            this.labelTags = new System.Windows.Forms.Label();
            this.textDescription = new System.Windows.Forms.TextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.textTopic = new System.Windows.Forms.TextBox();
            this.labelTopic = new System.Windows.Forms.Label();
            this.comboRepositories = new System.Windows.Forms.ComboBox();
            this.labelRepository = new System.Windows.Forms.Label();
            this.labelHelp = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.buttonClean = new System.Windows.Forms.Button();
            this.buttonFilter = new System.Windows.Forms.Button();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.panelForm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            //
            // panelForm
            //
            this.panelForm.Controls.Add(this.listViewAttributes);
            this.panelForm.Controls.Add(this.buttonRemoveAttribute);
            this.panelForm.Controls.Add(this.buttonAddAttribute);
            this.panelForm.Controls.Add(this.labelAttributes);
            this.panelForm.Controls.Add(this.buttonFolderSelect);
            this.panelForm.Controls.Add(this.buttonFolderClear);
            this.panelForm.Controls.Add(this.textFolder);
            this.panelForm.Controls.Add(this.labelFolder);
            this.panelForm.Controls.Add(this.comboNoteType);
            this.panelForm.Controls.Add(this.labelNoteType);
            this.panelForm.Controls.Add(this.textTags);
            this.panelForm.Controls.Add(this.labelTags);
            this.panelForm.Controls.Add(this.textDescription);
            this.panelForm.Controls.Add(this.labelDescription);
            this.panelForm.Controls.Add(this.textTopic);
            this.panelForm.Controls.Add(this.labelTopic);
            this.panelForm.Controls.Add(this.comboRepositories);
            this.panelForm.Controls.Add(this.labelRepository);
            this.panelForm.Controls.Add(this.labelHelp);
            this.panelForm.Controls.Add(this.pictureBox1);
            this.panelForm.Controls.Add(this.buttonClean);
            this.panelForm.Controls.Add(this.buttonFilter);
            this.panelForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelForm.Location = new System.Drawing.Point(0, 0);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(437, 600);
            this.panelForm.TabIndex = 0;
            //
            // listViewAttributes
            //
            this.listViewAttributes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewAttributes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnAttribute,
            this.columnValue});
            this.listViewAttributes.FullRowSelect = true;
            this.listViewAttributes.HideSelection = false;
            this.listViewAttributes.Location = new System.Drawing.Point(7, 404);
            this.listViewAttributes.MultiSelect = false;
            this.listViewAttributes.Name = "listViewAttributes";
            this.listViewAttributes.Size = new System.Drawing.Size(424, 175);
            this.listViewAttributes.TabIndex = 21;
            this.listViewAttributes.UseCompatibleStateImageBehavior = false;
            this.listViewAttributes.View = System.Windows.Forms.View.Details;
            //
            // columnAttribute
            //
            this.columnAttribute.Text = "Attribute";
            this.columnAttribute.Width = 200;
            //
            // columnValue
            //
            this.columnValue.Text = "Value";
            this.columnValue.Width = 204;
            //
            // buttonRemoveAttribute
            //
            this.buttonRemoveAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveAttribute.Location = new System.Drawing.Point(401, 377);
            this.buttonRemoveAttribute.Name = "buttonRemoveAttribute";
            this.buttonRemoveAttribute.Size = new System.Drawing.Size(27, 23);
            this.buttonRemoveAttribute.TabIndex = 20;
            this.buttonRemoveAttribute.Text = "-";
            this.buttonRemoveAttribute.UseVisualStyleBackColor = true;
            this.buttonRemoveAttribute.Click += new System.EventHandler(this.buttonRemoveAttribute_Click);
            //
            // buttonAddAttribute
            //
            this.buttonAddAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddAttribute.Location = new System.Drawing.Point(373, 377);
            this.buttonAddAttribute.Name = "buttonAddAttribute";
            this.buttonAddAttribute.Size = new System.Drawing.Size(27, 23);
            this.buttonAddAttribute.TabIndex = 19;
            this.buttonAddAttribute.Text = "+";
            this.buttonAddAttribute.UseVisualStyleBackColor = true;
            this.buttonAddAttribute.Click += new System.EventHandler(this.buttonAddAttribute_Click);
            //
            // labelAttributes
            //
            this.labelAttributes.AutoSize = true;
            this.labelAttributes.Location = new System.Drawing.Point(7, 381);
            this.labelAttributes.Name = "labelAttributes";
            this.labelAttributes.Size = new System.Drawing.Size(67, 15);
            this.labelAttributes.TabIndex = 18;
            this.labelAttributes.Text = "Attributes:";
            //
            // buttonFolderSelect
            //
            this.buttonFolderSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFolderSelect.Location = new System.Drawing.Point(401, 346);
            this.buttonFolderSelect.Name = "buttonFolderSelect";
            this.buttonFolderSelect.Size = new System.Drawing.Size(27, 26);
            this.buttonFolderSelect.TabIndex = 17;
            this.buttonFolderSelect.Text = "...";
            this.buttonFolderSelect.UseVisualStyleBackColor = true;
            this.buttonFolderSelect.Click += new System.EventHandler(this.buttonFolderSelect_Click);
            //
            // buttonFolderClear
            //
            this.buttonFolderClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFolderClear.Location = new System.Drawing.Point(372, 346);
            this.buttonFolderClear.Name = "buttonFolderClear";
            this.buttonFolderClear.Size = new System.Drawing.Size(27, 26);
            this.buttonFolderClear.TabIndex = 16;
            this.buttonFolderClear.Text = "X";
            this.buttonFolderClear.UseVisualStyleBackColor = true;
            this.buttonFolderClear.Click += new System.EventHandler(this.buttonFolderClear_Click);
            //
            // textFolder
            //
            this.textFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textFolder.Enabled = false;
            this.textFolder.Location = new System.Drawing.Point(7, 348);
            this.textFolder.Name = "textFolder";
            this.textFolder.Size = new System.Drawing.Size(363, 23);
            this.textFolder.TabIndex = 15;
            //
            // labelFolder
            //
            this.labelFolder.AutoSize = true;
            this.labelFolder.Location = new System.Drawing.Point(7, 330);
            this.labelFolder.Name = "labelFolder";
            this.labelFolder.Size = new System.Drawing.Size(45, 15);
            this.labelFolder.TabIndex = 14;
            this.labelFolder.Text = "Folder:";
            //
            // comboNoteType
            //
            this.comboNoteType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboNoteType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboNoteType.FormattingEnabled = true;
            this.comboNoteType.Location = new System.Drawing.Point(7, 299);
            this.comboNoteType.Name = "comboNoteType";
            this.comboNoteType.Size = new System.Drawing.Size(424, 23);
            this.comboNoteType.TabIndex = 13;
            //
            // labelNoteType
            //
            this.labelNoteType.AutoSize = true;
            this.labelNoteType.Location = new System.Drawing.Point(7, 281);
            this.labelNoteType.Name = "labelNoteType";
            this.labelNoteType.Size = new System.Drawing.Size(68, 15);
            this.labelNoteType.TabIndex = 12;
            this.labelNoteType.Text = "Note type:";
            //
            // textTags
            //
            this.textTags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textTags.Location = new System.Drawing.Point(7, 250);
            this.textTags.Name = "textTags";
            this.textTags.Size = new System.Drawing.Size(424, 23);
            this.textTags.TabIndex = 11;
            //
            // labelTags
            //
            this.labelTags.AutoSize = true;
            this.labelTags.Location = new System.Drawing.Point(7, 232);
            this.labelTags.Name = "labelTags";
            this.labelTags.Size = new System.Drawing.Size(35, 15);
            this.labelTags.TabIndex = 10;
            this.labelTags.Text = "Tags:";
            //
            // textDescription
            //
            this.textDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textDescription.Location = new System.Drawing.Point(7, 201);
            this.textDescription.Name = "textDescription";
            this.textDescription.Size = new System.Drawing.Size(424, 23);
            this.textDescription.TabIndex = 9;
            //
            // labelDescription
            //
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(7, 183);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(72, 15);
            this.labelDescription.TabIndex = 8;
            this.labelDescription.Text = "Description:";
            //
            // textTopic
            //
            this.textTopic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textTopic.Location = new System.Drawing.Point(7, 152);
            this.textTopic.Name = "textTopic";
            this.textTopic.Size = new System.Drawing.Size(424, 23);
            this.textTopic.TabIndex = 7;
            //
            // labelTopic
            //
            this.labelTopic.AutoSize = true;
            this.labelTopic.Location = new System.Drawing.Point(7, 134);
            this.labelTopic.Name = "labelTopic";
            this.labelTopic.Size = new System.Drawing.Size(41, 15);
            this.labelTopic.TabIndex = 6;
            this.labelTopic.Text = "Topic:";
            //
            // comboRepositories
            //
            this.comboRepositories.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboRepositories.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboRepositories.FormattingEnabled = true;
            this.comboRepositories.Location = new System.Drawing.Point(5, 103);
            this.comboRepositories.Name = "comboRepositories";
            this.comboRepositories.Size = new System.Drawing.Size(424, 23);
            this.comboRepositories.TabIndex = 5;
            this.comboRepositories.SelectedIndexChanged += new System.EventHandler(this.comboRepositories_SelectedIndexChanged);
            //
            // labelRepository
            //
            this.labelRepository.AutoSize = true;
            this.labelRepository.Location = new System.Drawing.Point(7, 85);
            this.labelRepository.Name = "labelRepository";
            this.labelRepository.Size = new System.Drawing.Size(66, 15);
            this.labelRepository.TabIndex = 4;
            this.labelRepository.Text = "Repository:";
            //
            // labelHelp
            //
            this.labelHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelHelp.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelHelp.Location = new System.Drawing.Point(7, 49);
            this.labelHelp.Name = "labelHelp";
            this.labelHelp.Size = new System.Drawing.Size(423, 32);
            this.labelHelp.TabIndex = 3;
            this.labelHelp.Text = "Search notes matching all the criteria below (leave a field blank to ignore it)" +
    ".";
            //
            // pictureBox1
            //
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(5, 40);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(425, 1);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            //
            // buttonClean
            //
            this.buttonClean.Location = new System.Drawing.Point(7, 9);
            this.buttonClean.Name = "buttonClean";
            this.buttonClean.Size = new System.Drawing.Size(65, 27);
            this.buttonClean.TabIndex = 0;
            this.buttonClean.Text = "&Clean";
            this.buttonClean.UseVisualStyleBackColor = true;
            this.buttonClean.Click += new System.EventHandler(this.buttonClean_Click);
            //
            // buttonFilter
            //
            this.buttonFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFilter.Location = new System.Drawing.Point(301, 9);
            this.buttonFilter.Name = "buttonFilter";
            this.buttonFilter.Size = new System.Drawing.Size(130, 27);
            this.buttonFilter.TabIndex = 1;
            this.buttonFilter.Text = "&Apply filter";
            this.buttonFilter.UseVisualStyleBackColor = true;
            this.buttonFilter.Click += new System.EventHandler(this.buttonFilter_Click);
            //
            // panelBottom
            //
            this.panelBottom.Controls.Add(this.buttonCancel);
            this.panelBottom.Controls.Add(this.buttonAccept);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 600);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(437, 50);
            this.panelBottom.TabIndex = 1;
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(347, 11);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(81, 29);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            //
            // buttonAccept
            //
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(260, 11);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(81, 29);
            this.buttonAccept.TabIndex = 0;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            //
            // NotesFilterParamForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 650);
            this.Controls.Add(this.panelForm);
            this.Controls.Add(this.panelBottom);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NotesFilterParamForm";
            this.Text = "Filter parameters";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NotesFilterParamForm_FormClosing);
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Button buttonClean;
        private System.Windows.Forms.Button buttonFilter;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label labelHelp;
        private System.Windows.Forms.Label labelRepository;
        private System.Windows.Forms.ComboBox comboRepositories;
        private System.Windows.Forms.Label labelTopic;
        private System.Windows.Forms.TextBox textTopic;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.TextBox textDescription;
        private System.Windows.Forms.Label labelTags;
        private System.Windows.Forms.TextBox textTags;
        private System.Windows.Forms.Label labelNoteType;
        private System.Windows.Forms.ComboBox comboNoteType;
        private System.Windows.Forms.Label labelFolder;
        private System.Windows.Forms.TextBox textFolder;
        private System.Windows.Forms.Button buttonFolderClear;
        private System.Windows.Forms.Button buttonFolderSelect;
        private System.Windows.Forms.Label labelAttributes;
        private System.Windows.Forms.Button buttonAddAttribute;
        private System.Windows.Forms.Button buttonRemoveAttribute;
        private System.Windows.Forms.ListView listViewAttributes;
        private System.Windows.Forms.ColumnHeader columnAttribute;
        private System.Windows.Forms.ColumnHeader columnValue;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
    }
}
