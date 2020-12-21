﻿namespace KNote.ClientWin.Views
{
    partial class NoteEditorForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NoteEditorForm));
            this.toolBarNoteEditor = new System.Windows.Forms.ToolStrip();
            this.buttonSave = new System.Windows.Forms.ToolStripButton();
            this.buttonDelete = new System.Windows.Forms.ToolStripButton();
            this.buttonUndo = new System.Windows.Forms.ToolStripButton();
            this.separador1 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonPostIt = new System.Windows.Forms.ToolStripButton();
            this.separador2 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonCheck = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonPrint = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonTools = new System.Windows.Forms.ToolStripDropDownButton();
            this.buttonInsertTemplate = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonExecuteAntScript = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonWordWrap = new System.Windows.Forms.ToolStripMenuItem();
            this.imageListTabNoteData = new System.Windows.Forms.ImageList(this.components);
            this.panelForm = new System.Windows.Forms.Panel();
            this.tabNoteData = new System.Windows.Forms.TabControl();
            this.tabBasicData = new System.Windows.Forms.TabPage();
            this.panelDescription = new System.Windows.Forms.Panel();
            this.htmlDescription = new Pavonis.Html.Editor.HtmlEditorControl();
            this.textDescription = new System.Windows.Forms.TextBox();
            this.toolDescription = new System.Windows.Forms.ToolStrip();
            this.toolDescriptionHtmlTitles = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolDescriptionHtmlTitle1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionHtmlTitle2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionHtmlTitle3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionHtmlTitle4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionMarkdown = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolDescriptionMarkdownH1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionMarkdownH2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionMarkdownH3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionMarkdownH4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionMarkdownS1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolDescriptionMarkdownBold = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionMarkdownStrikethrough = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionMarkdownItalic = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionMarkdownS2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolDescriptionMarkdownList = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionMarkdownListOrdered = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionMarkdownLine = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionMarkdownLink = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionMarkdownImage = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionMarkdownTable = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionMarkdownCode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionUploads = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolDescriptionUploadFromFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolDescriptionUploadFromClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.labelLoadingHtml = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.buttonEditMarkdown = new System.Windows.Forms.Button();
            this.buttonViewHtml = new System.Windows.Forms.Button();
            this.textPriority = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textTags = new System.Windows.Forms.TextBox();
            this.textFolder = new System.Windows.Forms.TextBox();
            this.textTopic = new System.Windows.Forms.TextBox();
            this.buttonFolderSearch = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textNoteNumber = new System.Windows.Forms.TextBox();
            this.textFolderNumber = new System.Windows.Forms.TextBox();
            this.tabAlarms = new System.Windows.Forms.TabPage();
            this.listViewAlarms = new System.Windows.Forms.ListView();
            this.buttonEditAlarm = new System.Windows.Forms.Button();
            this.buttonDeleteAlarm = new System.Windows.Forms.Button();
            this.buttonAddAlarm = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.tabAttributes = new System.Windows.Forms.TabPage();
            this.listViewAttributes = new System.Windows.Forms.ListView();
            this.textNoteType = new System.Windows.Forms.TextBox();
            this.buttonNoteType = new System.Windows.Forms.Button();
            this.label15 = new System.Windows.Forms.Label();
            this.buttonAttributeEdit = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.tabResources = new System.Windows.Forms.TabPage();
            this.labelPreview = new System.Windows.Forms.Label();
            this.listViewResources = new System.Windows.Forms.ListView();
            this.picResource = new System.Windows.Forms.PictureBox();
            this.buttonResourceEdit = new System.Windows.Forms.Button();
            this.buttonResourceDelete = new System.Windows.Forms.Button();
            this.buttonResourceAdd = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.tabTasks = new System.Windows.Forms.TabPage();
            this.listViewTasks = new System.Windows.Forms.ListView();
            this.buttonTaskEdit = new System.Windows.Forms.Button();
            this.buttonTaskDelete = new System.Windows.Forms.Button();
            this.buttonTaskAdd = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.tabCode = new System.Windows.Forms.TabPage();
            this.textScriptCode = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tabTraceNotes = new System.Windows.Forms.TabPage();
            this.listView2 = new System.Windows.Forms.ListView();
            this.listView1 = new System.Windows.Forms.ListView();
            this.textTraceNodeType = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.button16 = new System.Windows.Forms.Button();
            this.button17 = new System.Windows.Forms.Button();
            this.button18 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            this.button14 = new System.Windows.Forms.Button();
            this.button15 = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.toolBarNoteEditor.SuspendLayout();
            this.panelForm.SuspendLayout();
            this.tabNoteData.SuspendLayout();
            this.tabBasicData.SuspendLayout();
            this.panelDescription.SuspendLayout();
            this.toolDescription.SuspendLayout();
            this.tabAlarms.SuspendLayout();
            this.tabAttributes.SuspendLayout();
            this.tabResources.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picResource)).BeginInit();
            this.tabTasks.SuspendLayout();
            this.tabCode.SuspendLayout();
            this.tabTraceNotes.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolBarNoteEditor
            // 
            this.toolBarNoteEditor.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.toolBarNoteEditor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonSave,
            this.buttonDelete,
            this.buttonUndo,
            this.separador1,
            this.buttonPostIt,
            this.separador2,
            this.buttonCheck,
            this.toolStripSeparator1,
            this.buttonPrint,
            this.toolStripSeparator2,
            this.buttonTools});
            this.toolBarNoteEditor.Location = new System.Drawing.Point(0, 0);
            this.toolBarNoteEditor.Name = "toolBarNoteEditor";
            this.toolBarNoteEditor.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolBarNoteEditor.Size = new System.Drawing.Size(808, 25);
            this.toolBarNoteEditor.TabIndex = 6;
            this.toolBarNoteEditor.Text = "Toolbar KeyNotes";
            // 
            // buttonSave
            // 
            this.buttonSave.Image = ((System.Drawing.Image)(resources.GetObject("buttonSave.Image")));
            this.buttonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(57, 22);
            this.buttonSave.Text = "Save  ";
            this.buttonSave.Click += new System.EventHandler(this.buttonToolBar_Click);
            // 
            // buttonDelete
            // 
            this.buttonDelete.Image = ((System.Drawing.Image)(resources.GetObject("buttonDelete.Image")));
            this.buttonDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(66, 22);
            this.buttonDelete.Text = "Delete  ";
            this.buttonDelete.Click += new System.EventHandler(this.buttonToolBar_Click);
            // 
            // buttonUndo
            // 
            this.buttonUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonUndo.Enabled = false;
            this.buttonUndo.Image = ((System.Drawing.Image)(resources.GetObject("buttonUndo.Image")));
            this.buttonUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonUndo.Name = "buttonUndo";
            this.buttonUndo.Size = new System.Drawing.Size(23, 22);
            this.buttonUndo.Text = "Undo  ";
            this.buttonUndo.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.buttonUndo.ToolTipText = "Undo changes";
            this.buttonUndo.Click += new System.EventHandler(this.buttonToolBar_Click);
            // 
            // separador1
            // 
            this.separador1.Name = "separador1";
            this.separador1.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonPostIt
            // 
            this.buttonPostIt.Image = ((System.Drawing.Image)(resources.GetObject("buttonPostIt.Image")));
            this.buttonPostIt.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonPostIt.Name = "buttonPostIt";
            this.buttonPostIt.Size = new System.Drawing.Size(104, 22);
            this.buttonPostIt.Text = "View as Post-It";
            // 
            // separador2
            // 
            this.separador2.Name = "separador2";
            this.separador2.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonCheck
            // 
            this.buttonCheck.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonCheck.Image = ((System.Drawing.Image)(resources.GetObject("buttonCheck.Image")));
            this.buttonCheck.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonCheck.Name = "buttonCheck";
            this.buttonCheck.Size = new System.Drawing.Size(23, 22);
            this.buttonCheck.Text = "Check";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonPrint
            // 
            this.buttonPrint.Image = ((System.Drawing.Image)(resources.GetObject("buttonPrint.Image")));
            this.buttonPrint.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonPrint.Name = "buttonPrint";
            this.buttonPrint.Size = new System.Drawing.Size(58, 22);
            this.buttonPrint.Text = "Print  ";
            this.buttonPrint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonTools
            // 
            this.buttonTools.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonInsertTemplate,
            this.toolStripMenuItem3,
            this.buttonExecuteAntScript,
            this.toolStripMenuItem2,
            this.buttonWordWrap});
            this.buttonTools.Image = ((System.Drawing.Image)(resources.GetObject("buttonTools.Image")));
            this.buttonTools.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonTools.Name = "buttonTools";
            this.buttonTools.Size = new System.Drawing.Size(29, 22);
            this.buttonTools.Text = "toolStripDropDownTools";
            // 
            // buttonInsertTemplate
            // 
            this.buttonInsertTemplate.Name = "buttonInsertTemplate";
            this.buttonInsertTemplate.ShortcutKeys = System.Windows.Forms.Keys.F9;
            this.buttonInsertTemplate.Size = new System.Drawing.Size(217, 22);
            this.buttonInsertTemplate.Text = "Insertar template text ...";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(214, 6);
            // 
            // buttonExecuteAntScript
            // 
            this.buttonExecuteAntScript.Image = ((System.Drawing.Image)(resources.GetObject("buttonExecuteAntScript.Image")));
            this.buttonExecuteAntScript.Name = "buttonExecuteAntScript";
            this.buttonExecuteAntScript.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.buttonExecuteAntScript.Size = new System.Drawing.Size(217, 22);
            this.buttonExecuteAntScript.Text = "Execute AntScript code";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(214, 6);
            // 
            // buttonWordWrap
            // 
            this.buttonWordWrap.Checked = true;
            this.buttonWordWrap.CheckState = System.Windows.Forms.CheckState.Checked;
            this.buttonWordWrap.Name = "buttonWordWrap";
            this.buttonWordWrap.Size = new System.Drawing.Size(217, 22);
            this.buttonWordWrap.Text = "Word wrap";
            // 
            // imageListTabNoteData
            // 
            this.imageListTabNoteData.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageListTabNoteData.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListTabNoteData.ImageStream")));
            this.imageListTabNoteData.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListTabNoteData.Images.SetKeyName(0, "alarm_16.png");
            this.imageListTabNoteData.Images.SetKeyName(1, "fileTestLight_16.png");
            this.imageListTabNoteData.Images.SetKeyName(2, "bookmarkLight_24.png");
            this.imageListTabNoteData.Images.SetKeyName(3, "books_16.png");
            this.imageListTabNoteData.Images.SetKeyName(4, "code_16.png");
            this.imageListTabNoteData.Images.SetKeyName(5, "libraryBooks_16.png");
            this.imageListTabNoteData.Images.SetKeyName(6, "tasks_16.png");
            this.imageListTabNoteData.Images.SetKeyName(7, "checkbox_16.png");
            this.imageListTabNoteData.Images.SetKeyName(8, "codgs_16.png");
            this.imageListTabNoteData.Images.SetKeyName(9, "code_16.png");
            this.imageListTabNoteData.Images.SetKeyName(10, "upload_16.png");
            // 
            // panelForm
            // 
            this.panelForm.Controls.Add(this.tabNoteData);
            this.panelForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelForm.Location = new System.Drawing.Point(0, 25);
            this.panelForm.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(808, 612);
            this.panelForm.TabIndex = 39;
            // 
            // tabNoteData
            // 
            this.tabNoteData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabNoteData.Controls.Add(this.tabBasicData);
            this.tabNoteData.Controls.Add(this.tabAlarms);
            this.tabNoteData.Controls.Add(this.tabAttributes);
            this.tabNoteData.Controls.Add(this.tabResources);
            this.tabNoteData.Controls.Add(this.tabTasks);
            this.tabNoteData.Controls.Add(this.tabCode);
            this.tabNoteData.Controls.Add(this.tabTraceNotes);
            this.tabNoteData.ImageList = this.imageListTabNoteData;
            this.tabNoteData.Location = new System.Drawing.Point(4, 4);
            this.tabNoteData.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabNoteData.Name = "tabNoteData";
            this.tabNoteData.Padding = new System.Drawing.Point(4, 6);
            this.tabNoteData.SelectedIndex = 0;
            this.tabNoteData.Size = new System.Drawing.Size(802, 603);
            this.tabNoteData.TabIndex = 8;
            // 
            // tabBasicData
            // 
            this.tabBasicData.Controls.Add(this.panelDescription);
            this.tabBasicData.Controls.Add(this.labelLoadingHtml);
            this.tabBasicData.Controls.Add(this.label7);
            this.tabBasicData.Controls.Add(this.buttonEditMarkdown);
            this.tabBasicData.Controls.Add(this.buttonViewHtml);
            this.tabBasicData.Controls.Add(this.textPriority);
            this.tabBasicData.Controls.Add(this.label6);
            this.tabBasicData.Controls.Add(this.textTags);
            this.tabBasicData.Controls.Add(this.textFolder);
            this.tabBasicData.Controls.Add(this.textTopic);
            this.tabBasicData.Controls.Add(this.buttonFolderSearch);
            this.tabBasicData.Controls.Add(this.label3);
            this.tabBasicData.Controls.Add(this.label2);
            this.tabBasicData.Controls.Add(this.label1);
            this.tabBasicData.Controls.Add(this.textNoteNumber);
            this.tabBasicData.Controls.Add(this.textFolderNumber);
            this.tabBasicData.ImageIndex = 1;
            this.tabBasicData.Location = new System.Drawing.Point(4, 30);
            this.tabBasicData.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabBasicData.Name = "tabBasicData";
            this.tabBasicData.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabBasicData.Size = new System.Drawing.Size(794, 569);
            this.tabBasicData.TabIndex = 0;
            this.tabBasicData.Text = "Basic data  ";
            this.tabBasicData.UseVisualStyleBackColor = true;
            // 
            // panelDescription
            // 
            this.panelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelDescription.Controls.Add(this.htmlDescription);
            this.panelDescription.Controls.Add(this.textDescription);
            this.panelDescription.Controls.Add(this.toolDescription);
            this.panelDescription.Location = new System.Drawing.Point(14, 142);
            this.panelDescription.Name = "panelDescription";
            this.panelDescription.Size = new System.Drawing.Size(621, 376);
            this.panelDescription.TabIndex = 55;
            // 
            // htmlDescription
            // 
            this.htmlDescription.InnerText = null;
            this.htmlDescription.Location = new System.Drawing.Point(40, 116);
            this.htmlDescription.Name = "htmlDescription";
            this.htmlDescription.Size = new System.Drawing.Size(277, 125);
            this.htmlDescription.TabIndex = 8;
            // 
            // textDescription
            // 
            this.textDescription.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.textDescription.Location = new System.Drawing.Point(40, 14);
            this.textDescription.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textDescription.Multiline = true;
            this.textDescription.Name = "textDescription";
            this.textDescription.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textDescription.Size = new System.Drawing.Size(277, 78);
            this.textDescription.TabIndex = 7;
            // 
            // toolDescription
            // 
            this.toolDescription.Dock = System.Windows.Forms.DockStyle.Left;
            this.toolDescription.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolDescriptionHtmlTitles,
            this.toolDescriptionMarkdown,
            this.toolDescriptionUploads});
            this.toolDescription.Location = new System.Drawing.Point(0, 0);
            this.toolDescription.Name = "toolDescription";
            this.toolDescription.Size = new System.Drawing.Size(30, 376);
            this.toolDescription.TabIndex = 0;
            this.toolDescription.Text = "toolStrip1";
            // 
            // toolDescriptionHtmlTitles
            // 
            this.toolDescriptionHtmlTitles.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolDescriptionHtmlTitles.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolDescriptionHtmlTitle1,
            this.toolDescriptionHtmlTitle2,
            this.toolDescriptionHtmlTitle3,
            this.toolDescriptionHtmlTitle4});
            this.toolDescriptionHtmlTitles.Image = ((System.Drawing.Image)(resources.GetObject("toolDescriptionHtmlTitles.Image")));
            this.toolDescriptionHtmlTitles.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolDescriptionHtmlTitles.Name = "toolDescriptionHtmlTitles";
            this.toolDescriptionHtmlTitles.Size = new System.Drawing.Size(27, 20);
            this.toolDescriptionHtmlTitles.Text = "H";
            // 
            // toolDescriptionHtmlTitle1
            // 
            this.toolDescriptionHtmlTitle1.Name = "toolDescriptionHtmlTitle1";
            this.toolDescriptionHtmlTitle1.Size = new System.Drawing.Size(105, 22);
            this.toolDescriptionHtmlTitle1.Text = "Title 1";
            this.toolDescriptionHtmlTitle1.Click += new System.EventHandler(this.toolDescriptionHtmlTitle_Click);
            // 
            // toolDescriptionHtmlTitle2
            // 
            this.toolDescriptionHtmlTitle2.Name = "toolDescriptionHtmlTitle2";
            this.toolDescriptionHtmlTitle2.Size = new System.Drawing.Size(105, 22);
            this.toolDescriptionHtmlTitle2.Text = "Title 2";
            this.toolDescriptionHtmlTitle2.Click += new System.EventHandler(this.toolDescriptionHtmlTitle_Click);
            // 
            // toolDescriptionHtmlTitle3
            // 
            this.toolDescriptionHtmlTitle3.Name = "toolDescriptionHtmlTitle3";
            this.toolDescriptionHtmlTitle3.Size = new System.Drawing.Size(105, 22);
            this.toolDescriptionHtmlTitle3.Text = "Title 3";
            this.toolDescriptionHtmlTitle3.Click += new System.EventHandler(this.toolDescriptionHtmlTitle_Click);
            // 
            // toolDescriptionHtmlTitle4
            // 
            this.toolDescriptionHtmlTitle4.Name = "toolDescriptionHtmlTitle4";
            this.toolDescriptionHtmlTitle4.Size = new System.Drawing.Size(105, 22);
            this.toolDescriptionHtmlTitle4.Text = "Title 4";
            this.toolDescriptionHtmlTitle4.Click += new System.EventHandler(this.toolDescriptionHtmlTitle_Click);
            // 
            // toolDescriptionMarkdown
            // 
            this.toolDescriptionMarkdown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolDescriptionMarkdown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolDescriptionMarkdownH1,
            this.toolDescriptionMarkdownH2,
            this.toolDescriptionMarkdownH3,
            this.toolDescriptionMarkdownH4,
            this.toolDescriptionMarkdownS1,
            this.toolDescriptionMarkdownBold,
            this.toolDescriptionMarkdownStrikethrough,
            this.toolDescriptionMarkdownItalic,
            this.toolDescriptionMarkdownS2,
            this.toolDescriptionMarkdownList,
            this.toolDescriptionMarkdownListOrdered,
            this.toolDescriptionMarkdownLine,
            this.toolDescriptionMarkdownLink,
            this.toolDescriptionMarkdownImage,
            this.toolDescriptionMarkdownTable,
            this.toolDescriptionMarkdownCode});
            this.toolDescriptionMarkdown.Image = ((System.Drawing.Image)(resources.GetObject("toolDescriptionMarkdown.Image")));
            this.toolDescriptionMarkdown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolDescriptionMarkdown.Name = "toolDescriptionMarkdown";
            this.toolDescriptionMarkdown.Size = new System.Drawing.Size(27, 20);
            this.toolDescriptionMarkdown.Text = "Markdown";
            // 
            // toolDescriptionMarkdownH1
            // 
            this.toolDescriptionMarkdownH1.Name = "toolDescriptionMarkdownH1";
            this.toolDescriptionMarkdownH1.Size = new System.Drawing.Size(146, 22);
            this.toolDescriptionMarkdownH1.Text = "Title 1";
            this.toolDescriptionMarkdownH1.Click += new System.EventHandler(this.toolDescriptionMarkdown_Click);
            // 
            // toolDescriptionMarkdownH2
            // 
            this.toolDescriptionMarkdownH2.Name = "toolDescriptionMarkdownH2";
            this.toolDescriptionMarkdownH2.Size = new System.Drawing.Size(146, 22);
            this.toolDescriptionMarkdownH2.Text = "Title 2";
            this.toolDescriptionMarkdownH2.Click += new System.EventHandler(this.toolDescriptionMarkdown_Click);
            // 
            // toolDescriptionMarkdownH3
            // 
            this.toolDescriptionMarkdownH3.Name = "toolDescriptionMarkdownH3";
            this.toolDescriptionMarkdownH3.Size = new System.Drawing.Size(146, 22);
            this.toolDescriptionMarkdownH3.Text = "Title 3";
            this.toolDescriptionMarkdownH3.Click += new System.EventHandler(this.toolDescriptionMarkdown_Click);
            // 
            // toolDescriptionMarkdownH4
            // 
            this.toolDescriptionMarkdownH4.Name = "toolDescriptionMarkdownH4";
            this.toolDescriptionMarkdownH4.Size = new System.Drawing.Size(146, 22);
            this.toolDescriptionMarkdownH4.Text = "Title 4";
            this.toolDescriptionMarkdownH4.Click += new System.EventHandler(this.toolDescriptionMarkdown_Click);
            // 
            // toolDescriptionMarkdownS1
            // 
            this.toolDescriptionMarkdownS1.Name = "toolDescriptionMarkdownS1";
            this.toolDescriptionMarkdownS1.Size = new System.Drawing.Size(143, 6);
            // 
            // toolDescriptionMarkdownBold
            // 
            this.toolDescriptionMarkdownBold.Name = "toolDescriptionMarkdownBold";
            this.toolDescriptionMarkdownBold.Size = new System.Drawing.Size(146, 22);
            this.toolDescriptionMarkdownBold.Text = "Bold";
            this.toolDescriptionMarkdownBold.Click += new System.EventHandler(this.toolDescriptionMarkdown_Click);
            // 
            // toolDescriptionMarkdownStrikethrough
            // 
            this.toolDescriptionMarkdownStrikethrough.Name = "toolDescriptionMarkdownStrikethrough";
            this.toolDescriptionMarkdownStrikethrough.Size = new System.Drawing.Size(146, 22);
            this.toolDescriptionMarkdownStrikethrough.Text = "Strikethrough";
            this.toolDescriptionMarkdownStrikethrough.Click += new System.EventHandler(this.toolDescriptionMarkdown_Click);
            // 
            // toolDescriptionMarkdownItalic
            // 
            this.toolDescriptionMarkdownItalic.Name = "toolDescriptionMarkdownItalic";
            this.toolDescriptionMarkdownItalic.Size = new System.Drawing.Size(146, 22);
            this.toolDescriptionMarkdownItalic.Text = "Italic";
            this.toolDescriptionMarkdownItalic.Click += new System.EventHandler(this.toolDescriptionMarkdown_Click);
            // 
            // toolDescriptionMarkdownS2
            // 
            this.toolDescriptionMarkdownS2.Name = "toolDescriptionMarkdownS2";
            this.toolDescriptionMarkdownS2.Size = new System.Drawing.Size(143, 6);
            // 
            // toolDescriptionMarkdownList
            // 
            this.toolDescriptionMarkdownList.Name = "toolDescriptionMarkdownList";
            this.toolDescriptionMarkdownList.Size = new System.Drawing.Size(146, 22);
            this.toolDescriptionMarkdownList.Text = "List";
            this.toolDescriptionMarkdownList.Click += new System.EventHandler(this.toolDescriptionMarkdown_Click);
            // 
            // toolDescriptionMarkdownListOrdered
            // 
            this.toolDescriptionMarkdownListOrdered.Name = "toolDescriptionMarkdownListOrdered";
            this.toolDescriptionMarkdownListOrdered.Size = new System.Drawing.Size(146, 22);
            this.toolDescriptionMarkdownListOrdered.Text = "ListOrdered";
            this.toolDescriptionMarkdownListOrdered.Click += new System.EventHandler(this.toolDescriptionMarkdown_Click);
            // 
            // toolDescriptionMarkdownLine
            // 
            this.toolDescriptionMarkdownLine.Name = "toolDescriptionMarkdownLine";
            this.toolDescriptionMarkdownLine.Size = new System.Drawing.Size(146, 22);
            this.toolDescriptionMarkdownLine.Text = "Line";
            this.toolDescriptionMarkdownLine.Click += new System.EventHandler(this.toolDescriptionMarkdown_Click);
            // 
            // toolDescriptionMarkdownLink
            // 
            this.toolDescriptionMarkdownLink.Name = "toolDescriptionMarkdownLink";
            this.toolDescriptionMarkdownLink.Size = new System.Drawing.Size(146, 22);
            this.toolDescriptionMarkdownLink.Text = "Link";
            this.toolDescriptionMarkdownLink.Click += new System.EventHandler(this.toolDescriptionMarkdown_Click);
            // 
            // toolDescriptionMarkdownImage
            // 
            this.toolDescriptionMarkdownImage.Name = "toolDescriptionMarkdownImage";
            this.toolDescriptionMarkdownImage.Size = new System.Drawing.Size(146, 22);
            this.toolDescriptionMarkdownImage.Text = "Image";
            this.toolDescriptionMarkdownImage.Click += new System.EventHandler(this.toolDescriptionMarkdown_Click);
            // 
            // toolDescriptionMarkdownTable
            // 
            this.toolDescriptionMarkdownTable.Name = "toolDescriptionMarkdownTable";
            this.toolDescriptionMarkdownTable.Size = new System.Drawing.Size(146, 22);
            this.toolDescriptionMarkdownTable.Text = "Table";
            this.toolDescriptionMarkdownTable.Click += new System.EventHandler(this.toolDescriptionMarkdown_Click);
            // 
            // toolDescriptionMarkdownCode
            // 
            this.toolDescriptionMarkdownCode.Name = "toolDescriptionMarkdownCode";
            this.toolDescriptionMarkdownCode.Size = new System.Drawing.Size(146, 22);
            this.toolDescriptionMarkdownCode.Text = "Code";
            this.toolDescriptionMarkdownCode.Click += new System.EventHandler(this.toolDescriptionMarkdown_Click);
            // 
            // toolDescriptionUploads
            // 
            this.toolDescriptionUploads.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolDescriptionUploads.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolDescriptionUploadFromFile,
            this.toolDescriptionUploadFromClipboard});
            this.toolDescriptionUploads.Image = ((System.Drawing.Image)(resources.GetObject("toolDescriptionUploads.Image")));
            this.toolDescriptionUploads.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolDescriptionUploads.Name = "toolDescriptionUploads";
            this.toolDescriptionUploads.Size = new System.Drawing.Size(27, 20);
            this.toolDescriptionUploads.Text = "I";
            // 
            // toolDescriptionUploadFromFile
            // 
            this.toolDescriptionUploadFromFile.Name = "toolDescriptionUploadFromFile";
            this.toolDescriptionUploadFromFile.Size = new System.Drawing.Size(221, 22);
            this.toolDescriptionUploadFromFile.Text = "Insert image from file";
            // 
            // toolDescriptionUploadFromClipboard
            // 
            this.toolDescriptionUploadFromClipboard.Name = "toolDescriptionUploadFromClipboard";
            this.toolDescriptionUploadFromClipboard.Size = new System.Drawing.Size(221, 22);
            this.toolDescriptionUploadFromClipboard.Text = "Insert image form clipboard";
            // 
            // labelLoadingHtml
            // 
            this.labelLoadingHtml.AutoSize = true;
            this.labelLoadingHtml.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelLoadingHtml.Location = new System.Drawing.Point(101, 108);
            this.labelLoadingHtml.Name = "labelLoadingHtml";
            this.labelLoadingHtml.Size = new System.Drawing.Size(138, 15);
            this.labelLoadingHtml.TabIndex = 54;
            this.labelLoadingHtml.Text = "Loading html content ...";
            this.labelLoadingHtml.Visible = false;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(9, 68);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(103, 18);
            this.label7.TabIndex = 50;
            this.label7.Text = "Priority:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonEditMarkdown
            // 
            this.buttonEditMarkdown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEditMarkdown.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonEditMarkdown.ImageList = this.imageListTabNoteData;
            this.buttonEditMarkdown.Location = new System.Drawing.Point(658, 105);
            this.buttonEditMarkdown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonEditMarkdown.Name = "buttonEditMarkdown";
            this.buttonEditMarkdown.Size = new System.Drawing.Size(79, 24);
            this.buttonEditMarkdown.TabIndex = 5;
            this.buttonEditMarkdown.Text = "Markdown";
            this.buttonEditMarkdown.UseVisualStyleBackColor = true;
            this.buttonEditMarkdown.Click += new System.EventHandler(this.buttonEditMarkdown_Click);
            // 
            // buttonViewHtml
            // 
            this.buttonViewHtml.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonViewHtml.Location = new System.Drawing.Point(740, 105);
            this.buttonViewHtml.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonViewHtml.Name = "buttonViewHtml";
            this.buttonViewHtml.Size = new System.Drawing.Size(46, 24);
            this.buttonViewHtml.TabIndex = 6;
            this.buttonViewHtml.Text = "Html";
            this.buttonViewHtml.UseVisualStyleBackColor = true;
            this.buttonViewHtml.Click += new System.EventHandler(this.buttonViewHtml_Click);
            // 
            // textPriority
            // 
            this.textPriority.Location = new System.Drawing.Point(117, 68);
            this.textPriority.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textPriority.Name = "textPriority";
            this.textPriority.Size = new System.Drawing.Size(121, 23);
            this.textPriority.TabIndex = 3;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(9, 108);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(103, 18);
            this.label6.TabIndex = 44;
            this.label6.Text = "Description:";
            // 
            // textTags
            // 
            this.textTags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textTags.Location = new System.Drawing.Point(313, 68);
            this.textTags.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textTags.MaxLength = 255;
            this.textTags.Name = "textTags";
            this.textTags.Size = new System.Drawing.Size(474, 23);
            this.textTags.TabIndex = 4;
            // 
            // textFolder
            // 
            this.textFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textFolder.Enabled = false;
            this.textFolder.Location = new System.Drawing.Point(117, 38);
            this.textFolder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textFolder.MaxLength = 255;
            this.textFolder.Name = "textFolder";
            this.textFolder.Size = new System.Drawing.Size(589, 23);
            this.textFolder.TabIndex = 1;
            // 
            // textTopic
            // 
            this.textTopic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textTopic.Location = new System.Drawing.Point(117, 8);
            this.textTopic.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textTopic.MaxLength = 255;
            this.textTopic.Name = "textTopic";
            this.textTopic.Size = new System.Drawing.Size(589, 23);
            this.textTopic.TabIndex = 0;
            // 
            // buttonFolderSearch
            // 
            this.buttonFolderSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFolderSearch.BackColor = System.Drawing.SystemColors.Control;
            this.buttonFolderSearch.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonFolderSearch.Location = new System.Drawing.Point(760, 38);
            this.buttonFolderSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonFolderSearch.Name = "buttonFolderSearch";
            this.buttonFolderSearch.Size = new System.Drawing.Size(27, 23);
            this.buttonFolderSearch.TabIndex = 2;
            this.buttonFolderSearch.Text = "...";
            this.buttonFolderSearch.UseVisualStyleBackColor = false;
            this.buttonFolderSearch.Click += new System.EventHandler(this.buttonFolderSearch_Click);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(265, 68);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 18);
            this.label3.TabIndex = 36;
            this.label3.Text = "Tags:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(9, 38);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 18);
            this.label2.TabIndex = 34;
            this.label2.Text = "Folder:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(9, 8);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 18);
            this.label1.TabIndex = 33;
            this.label1.Text = "Topic:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textNoteNumber
            // 
            this.textNoteNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textNoteNumber.BackColor = System.Drawing.SystemColors.Control;
            this.textNoteNumber.Enabled = false;
            this.textNoteNumber.Location = new System.Drawing.Point(702, 8);
            this.textNoteNumber.Name = "textNoteNumber";
            this.textNoteNumber.Size = new System.Drawing.Size(84, 23);
            this.textNoteNumber.TabIndex = 51;
            this.textNoteNumber.TabStop = false;
            this.textNoteNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textFolderNumber
            // 
            this.textFolderNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textFolderNumber.BackColor = System.Drawing.SystemColors.Control;
            this.textFolderNumber.Enabled = false;
            this.textFolderNumber.Location = new System.Drawing.Point(702, 38);
            this.textFolderNumber.Name = "textFolderNumber";
            this.textFolderNumber.Size = new System.Drawing.Size(54, 23);
            this.textFolderNumber.TabIndex = 52;
            this.textFolderNumber.TabStop = false;
            this.textFolderNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // tabAlarms
            // 
            this.tabAlarms.Controls.Add(this.listViewAlarms);
            this.tabAlarms.Controls.Add(this.buttonEditAlarm);
            this.tabAlarms.Controls.Add(this.buttonDeleteAlarm);
            this.tabAlarms.Controls.Add(this.buttonAddAlarm);
            this.tabAlarms.Controls.Add(this.label4);
            this.tabAlarms.ImageIndex = 0;
            this.tabAlarms.Location = new System.Drawing.Point(4, 30);
            this.tabAlarms.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabAlarms.Name = "tabAlarms";
            this.tabAlarms.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabAlarms.Size = new System.Drawing.Size(794, 569);
            this.tabAlarms.TabIndex = 1;
            this.tabAlarms.Text = "Alarms  ";
            this.tabAlarms.UseVisualStyleBackColor = true;
            // 
            // listViewAlarms
            // 
            this.listViewAlarms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewAlarms.HideSelection = false;
            this.listViewAlarms.Location = new System.Drawing.Point(6, 34);
            this.listViewAlarms.MultiSelect = false;
            this.listViewAlarms.Name = "listViewAlarms";
            this.listViewAlarms.Size = new System.Drawing.Size(781, 532);
            this.listViewAlarms.TabIndex = 11;
            this.listViewAlarms.UseCompatibleStateImageBehavior = false;
            this.listViewAlarms.DoubleClick += new System.EventHandler(this.listViewAlarms_DoubleClick);
            this.listViewAlarms.Resize += new System.EventHandler(this.listView_Resize);
            // 
            // buttonEditAlarm
            // 
            this.buttonEditAlarm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEditAlarm.Location = new System.Drawing.Point(760, 10);
            this.buttonEditAlarm.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonEditAlarm.Name = "buttonEditAlarm";
            this.buttonEditAlarm.Size = new System.Drawing.Size(27, 23);
            this.buttonEditAlarm.TabIndex = 11;
            this.buttonEditAlarm.Text = "...";
            this.buttonEditAlarm.UseVisualStyleBackColor = true;
            this.buttonEditAlarm.Click += new System.EventHandler(this.buttonEditAlarm_Click);
            // 
            // buttonDeleteAlarm
            // 
            this.buttonDeleteAlarm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDeleteAlarm.Location = new System.Drawing.Point(725, 10);
            this.buttonDeleteAlarm.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonDeleteAlarm.Name = "buttonDeleteAlarm";
            this.buttonDeleteAlarm.Size = new System.Drawing.Size(27, 23);
            this.buttonDeleteAlarm.TabIndex = 10;
            this.buttonDeleteAlarm.Text = "-";
            this.buttonDeleteAlarm.UseVisualStyleBackColor = true;
            this.buttonDeleteAlarm.Click += new System.EventHandler(this.buttonDeleteAlarm_Click);
            // 
            // buttonAddAlarm
            // 
            this.buttonAddAlarm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddAlarm.Location = new System.Drawing.Point(690, 10);
            this.buttonAddAlarm.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonAddAlarm.Name = "buttonAddAlarm";
            this.buttonAddAlarm.Size = new System.Drawing.Size(27, 23);
            this.buttonAddAlarm.TabIndex = 9;
            this.buttonAddAlarm.Text = "+";
            this.buttonAddAlarm.UseVisualStyleBackColor = true;
            this.buttonAddAlarm.Click += new System.EventHandler(this.buttonAddAlarm_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 16);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 15);
            this.label4.TabIndex = 0;
            this.label4.Text = "Alarms:";
            // 
            // tabAttributes
            // 
            this.tabAttributes.Controls.Add(this.listViewAttributes);
            this.tabAttributes.Controls.Add(this.textNoteType);
            this.tabAttributes.Controls.Add(this.buttonNoteType);
            this.tabAttributes.Controls.Add(this.label15);
            this.tabAttributes.Controls.Add(this.buttonAttributeEdit);
            this.tabAttributes.Controls.Add(this.label10);
            this.tabAttributes.ImageIndex = 5;
            this.tabAttributes.Location = new System.Drawing.Point(4, 30);
            this.tabAttributes.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabAttributes.Name = "tabAttributes";
            this.tabAttributes.Size = new System.Drawing.Size(794, 569);
            this.tabAttributes.TabIndex = 3;
            this.tabAttributes.Text = "Attributes  ";
            this.tabAttributes.UseVisualStyleBackColor = true;
            // 
            // listViewAttributes
            // 
            this.listViewAttributes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewAttributes.HideSelection = false;
            this.listViewAttributes.Location = new System.Drawing.Point(10, 79);
            this.listViewAttributes.Name = "listViewAttributes";
            this.listViewAttributes.Size = new System.Drawing.Size(774, 478);
            this.listViewAttributes.TabIndex = 46;
            this.listViewAttributes.UseCompatibleStateImageBehavior = false;
            this.listViewAttributes.Resize += new System.EventHandler(this.listView_Resize);
            // 
            // textNoteType
            // 
            this.textNoteType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textNoteType.Enabled = false;
            this.textNoteType.Location = new System.Drawing.Point(103, 16);
            this.textNoteType.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textNoteType.MaxLength = 255;
            this.textNoteType.Name = "textNoteType";
            this.textNoteType.ShortcutsEnabled = false;
            this.textNoteType.Size = new System.Drawing.Size(650, 23);
            this.textNoteType.TabIndex = 44;
            // 
            // buttonNoteType
            // 
            this.buttonNoteType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNoteType.BackColor = System.Drawing.SystemColors.Control;
            this.buttonNoteType.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonNoteType.Location = new System.Drawing.Point(758, 17);
            this.buttonNoteType.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonNoteType.Name = "buttonNoteType";
            this.buttonNoteType.Size = new System.Drawing.Size(27, 23);
            this.buttonNoteType.TabIndex = 45;
            this.buttonNoteType.Text = "...";
            this.buttonNoteType.UseVisualStyleBackColor = false;
            // 
            // label15
            // 
            this.label15.Location = new System.Drawing.Point(10, 16);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(85, 18);
            this.label15.TabIndex = 43;
            this.label15.Text = "Node type:";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonAttributeEdit
            // 
            this.buttonAttributeEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAttributeEdit.Location = new System.Drawing.Point(758, 54);
            this.buttonAttributeEdit.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonAttributeEdit.Name = "buttonAttributeEdit";
            this.buttonAttributeEdit.Size = new System.Drawing.Size(27, 23);
            this.buttonAttributeEdit.TabIndex = 12;
            this.buttonAttributeEdit.Text = "...";
            this.buttonAttributeEdit.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(10, 61);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(62, 15);
            this.label10.TabIndex = 3;
            this.label10.Text = "Attributes:";
            // 
            // tabResources
            // 
            this.tabResources.Controls.Add(this.labelPreview);
            this.tabResources.Controls.Add(this.listViewResources);
            this.tabResources.Controls.Add(this.picResource);
            this.tabResources.Controls.Add(this.buttonResourceEdit);
            this.tabResources.Controls.Add(this.buttonResourceDelete);
            this.tabResources.Controls.Add(this.buttonResourceAdd);
            this.tabResources.Controls.Add(this.label12);
            this.tabResources.ImageIndex = 3;
            this.tabResources.Location = new System.Drawing.Point(4, 30);
            this.tabResources.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabResources.Name = "tabResources";
            this.tabResources.Size = new System.Drawing.Size(794, 569);
            this.tabResources.TabIndex = 4;
            this.tabResources.Text = "Resources  ";
            this.tabResources.UseVisualStyleBackColor = true;
            // 
            // labelPreview
            // 
            this.labelPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPreview.AutoSize = true;
            this.labelPreview.Location = new System.Drawing.Point(689, 18);
            this.labelPreview.Name = "labelPreview";
            this.labelPreview.Size = new System.Drawing.Size(99, 15);
            this.labelPreview.TabIndex = 15;
            this.labelPreview.Text = "Preview resource:";
            // 
            // listViewResources
            // 
            this.listViewResources.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewResources.HideSelection = false;
            this.listViewResources.Location = new System.Drawing.Point(5, 36);
            this.listViewResources.Name = "listViewResources";
            this.listViewResources.Size = new System.Drawing.Size(381, 529);
            this.listViewResources.TabIndex = 14;
            this.listViewResources.UseCompatibleStateImageBehavior = false;
            this.listViewResources.SelectedIndexChanged += new System.EventHandler(this.listViewResources_SelectedIndexChanged);
            this.listViewResources.Resize += new System.EventHandler(this.listView_Resize);
            // 
            // picResource
            // 
            this.picResource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picResource.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picResource.BackgroundImage")));
            this.picResource.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picResource.Location = new System.Drawing.Point(396, 36);
            this.picResource.Name = "picResource";
            this.picResource.Size = new System.Drawing.Size(392, 530);
            this.picResource.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picResource.TabIndex = 13;
            this.picResource.TabStop = false;
            // 
            // buttonResourceEdit
            // 
            this.buttonResourceEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonResourceEdit.Location = new System.Drawing.Point(360, 10);
            this.buttonResourceEdit.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonResourceEdit.Name = "buttonResourceEdit";
            this.buttonResourceEdit.Size = new System.Drawing.Size(27, 23);
            this.buttonResourceEdit.TabIndex = 12;
            this.buttonResourceEdit.Text = "...";
            this.buttonResourceEdit.UseVisualStyleBackColor = true;
            // 
            // buttonResourceDelete
            // 
            this.buttonResourceDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonResourceDelete.Location = new System.Drawing.Point(330, 10);
            this.buttonResourceDelete.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonResourceDelete.Name = "buttonResourceDelete";
            this.buttonResourceDelete.Size = new System.Drawing.Size(27, 23);
            this.buttonResourceDelete.TabIndex = 11;
            this.buttonResourceDelete.Text = "-";
            this.buttonResourceDelete.UseVisualStyleBackColor = true;
            // 
            // buttonResourceAdd
            // 
            this.buttonResourceAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonResourceAdd.Location = new System.Drawing.Point(300, 10);
            this.buttonResourceAdd.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonResourceAdd.Name = "buttonResourceAdd";
            this.buttonResourceAdd.Size = new System.Drawing.Size(27, 23);
            this.buttonResourceAdd.TabIndex = 10;
            this.buttonResourceAdd.Text = "+";
            this.buttonResourceAdd.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 14);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(63, 15);
            this.label12.TabIndex = 5;
            this.label12.Text = "Resources:";
            // 
            // tabTasks
            // 
            this.tabTasks.Controls.Add(this.listViewTasks);
            this.tabTasks.Controls.Add(this.buttonTaskEdit);
            this.tabTasks.Controls.Add(this.buttonTaskDelete);
            this.tabTasks.Controls.Add(this.buttonTaskAdd);
            this.tabTasks.Controls.Add(this.label11);
            this.tabTasks.ImageIndex = 6;
            this.tabTasks.Location = new System.Drawing.Point(4, 30);
            this.tabTasks.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabTasks.Name = "tabTasks";
            this.tabTasks.Size = new System.Drawing.Size(794, 569);
            this.tabTasks.TabIndex = 2;
            this.tabTasks.Text = "Tasks  ";
            this.tabTasks.UseVisualStyleBackColor = true;
            // 
            // listViewTasks
            // 
            this.listViewTasks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewTasks.HideSelection = false;
            this.listViewTasks.Location = new System.Drawing.Point(5, 36);
            this.listViewTasks.Name = "listViewTasks";
            this.listViewTasks.Size = new System.Drawing.Size(782, 530);
            this.listViewTasks.TabIndex = 13;
            this.listViewTasks.UseCompatibleStateImageBehavior = false;
            this.listViewTasks.Resize += new System.EventHandler(this.listView_Resize);
            // 
            // buttonTaskEdit
            // 
            this.buttonTaskEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTaskEdit.Location = new System.Drawing.Point(760, 10);
            this.buttonTaskEdit.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonTaskEdit.Name = "buttonTaskEdit";
            this.buttonTaskEdit.Size = new System.Drawing.Size(27, 23);
            this.buttonTaskEdit.TabIndex = 12;
            this.buttonTaskEdit.Text = "...";
            this.buttonTaskEdit.UseVisualStyleBackColor = true;
            // 
            // buttonTaskDelete
            // 
            this.buttonTaskDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTaskDelete.Location = new System.Drawing.Point(725, 10);
            this.buttonTaskDelete.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonTaskDelete.Name = "buttonTaskDelete";
            this.buttonTaskDelete.Size = new System.Drawing.Size(27, 23);
            this.buttonTaskDelete.TabIndex = 11;
            this.buttonTaskDelete.Text = "-";
            this.buttonTaskDelete.UseVisualStyleBackColor = true;
            // 
            // buttonTaskAdd
            // 
            this.buttonTaskAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTaskAdd.Location = new System.Drawing.Point(690, 10);
            this.buttonTaskAdd.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonTaskAdd.Name = "buttonTaskAdd";
            this.buttonTaskAdd.Size = new System.Drawing.Size(27, 23);
            this.buttonTaskAdd.TabIndex = 10;
            this.buttonTaskAdd.Text = "+";
            this.buttonTaskAdd.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 14);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(37, 15);
            this.label11.TabIndex = 5;
            this.label11.Text = "Tasks:";
            // 
            // tabCode
            // 
            this.tabCode.Controls.Add(this.textScriptCode);
            this.tabCode.Controls.Add(this.label9);
            this.tabCode.ImageIndex = 8;
            this.tabCode.Location = new System.Drawing.Point(4, 30);
            this.tabCode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabCode.Name = "tabCode";
            this.tabCode.Size = new System.Drawing.Size(794, 569);
            this.tabCode.TabIndex = 6;
            this.tabCode.Text = "Code   ";
            this.tabCode.UseVisualStyleBackColor = true;
            // 
            // textScriptCode
            // 
            this.textScriptCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textScriptCode.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.textScriptCode.Location = new System.Drawing.Point(7, 34);
            this.textScriptCode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textScriptCode.Multiline = true;
            this.textScriptCode.Name = "textScriptCode";
            this.textScriptCode.Size = new System.Drawing.Size(780, 532);
            this.textScriptCode.TabIndex = 6;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 16);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(69, 15);
            this.label9.TabIndex = 4;
            this.label9.Text = "Script code:";
            // 
            // tabTraceNotes
            // 
            this.tabTraceNotes.Controls.Add(this.listView2);
            this.tabTraceNotes.Controls.Add(this.listView1);
            this.tabTraceNotes.Controls.Add(this.textTraceNodeType);
            this.tabTraceNotes.Controls.Add(this.button3);
            this.tabTraceNotes.Controls.Add(this.label5);
            this.tabTraceNotes.Controls.Add(this.button16);
            this.tabTraceNotes.Controls.Add(this.button17);
            this.tabTraceNotes.Controls.Add(this.button18);
            this.tabTraceNotes.Controls.Add(this.button13);
            this.tabTraceNotes.Controls.Add(this.button14);
            this.tabTraceNotes.Controls.Add(this.button15);
            this.tabTraceNotes.Controls.Add(this.label13);
            this.tabTraceNotes.Controls.Add(this.label14);
            this.tabTraceNotes.ImageIndex = 2;
            this.tabTraceNotes.Location = new System.Drawing.Point(4, 30);
            this.tabTraceNotes.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabTraceNotes.Name = "tabTraceNotes";
            this.tabTraceNotes.Size = new System.Drawing.Size(794, 569);
            this.tabTraceNotes.TabIndex = 5;
            this.tabTraceNotes.Text = "Trace notes  ";
            this.tabTraceNotes.UseVisualStyleBackColor = true;
            // 
            // listView2
            // 
            this.listView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView2.HideSelection = false;
            this.listView2.Location = new System.Drawing.Point(395, 89);
            this.listView2.Name = "listView2";
            this.listView2.Size = new System.Drawing.Size(393, 477);
            this.listView2.TabIndex = 44;
            this.listView2.UseCompatibleStateImageBehavior = false;
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(11, 89);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(378, 477);
            this.listView1.TabIndex = 43;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // textTraceNodeType
            // 
            this.textTraceNodeType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textTraceNodeType.Enabled = false;
            this.textTraceNodeType.Location = new System.Drawing.Point(135, 22);
            this.textTraceNodeType.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textTraceNodeType.MaxLength = 255;
            this.textTraceNodeType.Name = "textTraceNodeType";
            this.textTraceNodeType.Size = new System.Drawing.Size(617, 23);
            this.textTraceNodeType.TabIndex = 41;
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.BackColor = System.Drawing.SystemColors.Control;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button3.Location = new System.Drawing.Point(760, 22);
            this.button3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(24, 23);
            this.button3.TabIndex = 42;
            this.button3.Text = "...";
            this.button3.UseVisualStyleBackColor = false;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(9, 22);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(131, 18);
            this.label5.TabIndex = 40;
            this.label5.Text = "Trace node types:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button16
            // 
            this.button16.Location = new System.Drawing.Point(566, 60);
            this.button16.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button16.Name = "button16";
            this.button16.Size = new System.Drawing.Size(27, 23);
            this.button16.TabIndex = 15;
            this.button16.Text = "...";
            this.button16.UseVisualStyleBackColor = true;
            // 
            // button17
            // 
            this.button17.Location = new System.Drawing.Point(534, 60);
            this.button17.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button17.Name = "button17";
            this.button17.Size = new System.Drawing.Size(27, 23);
            this.button17.TabIndex = 14;
            this.button17.Text = "-";
            this.button17.UseVisualStyleBackColor = true;
            // 
            // button18
            // 
            this.button18.Location = new System.Drawing.Point(498, 60);
            this.button18.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button18.Name = "button18";
            this.button18.Size = new System.Drawing.Size(27, 23);
            this.button18.TabIndex = 13;
            this.button18.Text = "+";
            this.button18.UseVisualStyleBackColor = true;
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(196, 60);
            this.button13.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(27, 23);
            this.button13.TabIndex = 12;
            this.button13.Text = "...";
            this.button13.UseVisualStyleBackColor = true;
            // 
            // button14
            // 
            this.button14.Location = new System.Drawing.Point(162, 60);
            this.button14.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(27, 23);
            this.button14.TabIndex = 11;
            this.button14.Text = "-";
            this.button14.UseVisualStyleBackColor = true;
            // 
            // button15
            // 
            this.button15.Location = new System.Drawing.Point(128, 60);
            this.button15.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(27, 23);
            this.button15.TabIndex = 10;
            this.button15.Text = "+";
            this.button15.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(397, 64);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(81, 15);
            this.label13.TabIndex = 5;
            this.label13.Text = "Trace node to:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(13, 64);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(99, 15);
            this.label14.TabIndex = 4;
            this.label14.Text = "Trace node from :";
            // 
            // NoteEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(808, 637);
            this.Controls.Add(this.panelForm);
            this.Controls.Add(this.toolBarNoteEditor);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "NoteEditorForm";
            this.Text = "Note Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NoteEditorForm_FormClosing);
            this.Load += new System.EventHandler(this.NoteEditorForm_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NoteEditorForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.NoteEditorForm_KeyUp);
            this.toolBarNoteEditor.ResumeLayout(false);
            this.toolBarNoteEditor.PerformLayout();
            this.panelForm.ResumeLayout(false);
            this.tabNoteData.ResumeLayout(false);
            this.tabBasicData.ResumeLayout(false);
            this.tabBasicData.PerformLayout();
            this.panelDescription.ResumeLayout(false);
            this.panelDescription.PerformLayout();
            this.toolDescription.ResumeLayout(false);
            this.toolDescription.PerformLayout();
            this.tabAlarms.ResumeLayout(false);
            this.tabAlarms.PerformLayout();
            this.tabAttributes.ResumeLayout(false);
            this.tabAttributes.PerformLayout();
            this.tabResources.ResumeLayout(false);
            this.tabResources.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picResource)).EndInit();
            this.tabTasks.ResumeLayout(false);
            this.tabTasks.PerformLayout();
            this.tabCode.ResumeLayout(false);
            this.tabCode.PerformLayout();
            this.tabTraceNotes.ResumeLayout(false);
            this.tabTraceNotes.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolBarNoteEditor;
        private System.Windows.Forms.ToolStripButton buttonSave;
        private System.Windows.Forms.ToolStripButton buttonDelete;
        private System.Windows.Forms.ToolStripButton buttonUndo;
        private System.Windows.Forms.ToolStripSeparator separador1;
        private System.Windows.Forms.ToolStripButton buttonPostIt;
        private System.Windows.Forms.ToolStripSeparator separador2;
        private System.Windows.Forms.ToolStripButton buttonCheck;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton buttonPrint;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton buttonTools;
        private System.Windows.Forms.ToolStripMenuItem buttonInsertTemplate;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem buttonExecuteAntScript;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem buttonWordWrap;
        private System.Windows.Forms.ImageList imageListTabNoteData;
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.TabControl tabNoteData;
        private System.Windows.Forms.TabPage tabBasicData;
        private System.Windows.Forms.Label label6;
        internal System.Windows.Forms.TextBox textTags;
        internal System.Windows.Forms.TextBox textFolder;
        internal System.Windows.Forms.TextBox textTopic;
        private System.Windows.Forms.Button buttonFolderSearch;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabAlarms;
        private System.Windows.Forms.TabPage tabTasks;
        private System.Windows.Forms.TabPage tabAttributes;
        private System.Windows.Forms.TabPage tabResources;
        private System.Windows.Forms.TabPage tabTraceNotes;
        private System.Windows.Forms.TabPage tabCode;
        private System.Windows.Forms.Button buttonEditAlarm;
        private System.Windows.Forms.Button buttonDeleteAlarm;
        private System.Windows.Forms.Button buttonAddAlarm;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonAttributeEdit;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button buttonTaskEdit;
        private System.Windows.Forms.Button buttonTaskDelete;
        private System.Windows.Forms.Button buttonTaskAdd;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button buttonResourceEdit;
        private System.Windows.Forms.Button buttonResourceDelete;
        private System.Windows.Forms.Button buttonResourceAdd;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button button16;
        private System.Windows.Forms.Button button17;
        private System.Windows.Forms.Button button18;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.Button button15;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox textScriptCode;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button buttonEditMarkdown;
        private System.Windows.Forms.Button buttonViewHtml;
        private System.Windows.Forms.TextBox textPriority;
        private System.Windows.Forms.Label label7;
        internal System.Windows.Forms.TextBox textNoteType;
        private System.Windows.Forms.Button buttonNoteType;
        private System.Windows.Forms.Label label15;
        internal System.Windows.Forms.TextBox textTraceNodeType;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.PictureBox picResource;
        private System.Windows.Forms.TextBox textFolderNumber;
        private System.Windows.Forms.TextBox textNoteNumber;
        private System.Windows.Forms.ListView listViewAttributes;
        private System.Windows.Forms.Label labelPreview;
        private System.Windows.Forms.ListView listViewResources;
        private System.Windows.Forms.ListView listViewTasks;
        private System.Windows.Forms.ListView listViewAlarms;
        private System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label labelLoadingHtml;
        private System.Windows.Forms.Panel panelDescription;
        private Pavonis.Html.Editor.HtmlEditorControl htmlDescription;
        private System.Windows.Forms.TextBox textDescription;
        private System.Windows.Forms.ToolStrip toolDescription;
        private System.Windows.Forms.ToolStripDropDownButton toolDescriptionHtmlTitles;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionHtmlTitle1;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionHtmlTitle2;
        private System.Windows.Forms.ToolStripDropDownButton toolDescriptionUploads;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionUploadFromFile;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionUploadFromClipboard;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionHtmlTitle3;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionHtmlTitle4;
        private System.Windows.Forms.ToolStripDropDownButton toolDescriptionMarkdown;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionMarkdownH1;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionMarkdownH2;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionMarkdownH3;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionMarkdownH4;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionMarkdownBold;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionMarkdownStrikethrough;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionMarkdownItalic;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionMarkdownList;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionMarkdownListOrdered;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionMarkdownLine;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionMarkdownLink;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionMarkdownImage;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionMarkdownTable;
        private System.Windows.Forms.ToolStripSeparator toolDescriptionMarkdownS1;
        private System.Windows.Forms.ToolStripSeparator toolDescriptionMarkdownS2;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionMarkdownCode;
    }
}