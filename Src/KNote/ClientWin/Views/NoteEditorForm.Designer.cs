namespace KNote.ClientWin.Views
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NoteEditorForm));
            toolBarNoteEditor = new ToolStrip();
            buttonSave = new ToolStripButton();
            buttonDelete = new ToolStripButton();
            buttonUndo = new ToolStripButton();
            toolStripS1 = new ToolStripSeparator();
            buttonPostIt = new ToolStripButton();
            toolStripS2 = new ToolStripSeparator();
            buttonCheck = new ToolStripButton();
            toolStripS3 = new ToolStripSeparator();
            buttonPrint = new ToolStripButton();
            toolStripS4 = new ToolStripSeparator();
            buttonTools = new ToolStripDropDownButton();
            buttonInsertTemplate = new ToolStripMenuItem();
            toolStripToolS1 = new ToolStripSeparator();
            buttonExecuteKntScript = new ToolStripMenuItem();
            imageListTabNoteData = new ImageList(components);
            panelForm = new Panel();
            tabNoteData = new TabControl();
            tabBasicData = new TabPage();
            buttonNavigate = new Button();
            textStatus = new TextBox();
            label8 = new Label();
            panelDescription = new Panel();
            webView2 = new KntWebView.KWebView();
            htmlDescription = new Pavonis.Html.Editor.HtmlEditorControl();
            textDescription = new TextBox();
            toolDescription = new ToolStrip();
            toolDescriptionHtml = new ToolStripDropDownButton();
            toolDescriptionHtmlTitle1 = new ToolStripMenuItem();
            toolDescriptionHtmlTitle2 = new ToolStripMenuItem();
            toolDescriptionHtmlTitle3 = new ToolStripMenuItem();
            toolDescriptionHtmlTitle4 = new ToolStripMenuItem();
            toolDescriptionHtmlS3 = new ToolStripSeparator();
            toolDescriptionHtmlEdit = new ToolStripMenuItem();
            toolDescriptionMarkdown = new ToolStripDropDownButton();
            toolDescriptionMarkdownH1 = new ToolStripMenuItem();
            toolDescriptionMarkdownH2 = new ToolStripMenuItem();
            toolDescriptionMarkdownH3 = new ToolStripMenuItem();
            toolDescriptionMarkdownH4 = new ToolStripMenuItem();
            toolDescriptionMarkdownS1 = new ToolStripSeparator();
            toolDescriptionMarkdownBold = new ToolStripMenuItem();
            toolDescriptionMarkdownStrikethrough = new ToolStripMenuItem();
            toolDescriptionMarkdownItalic = new ToolStripMenuItem();
            toolDescriptionMarkdownS2 = new ToolStripSeparator();
            toolDescriptionMarkdownList = new ToolStripMenuItem();
            toolDescriptionMarkdownListOrdered = new ToolStripMenuItem();
            toolDescriptionMarkdownLine = new ToolStripMenuItem();
            toolDescriptionMarkdownLink = new ToolStripMenuItem();
            toolDescriptionMarkdownImage = new ToolStripMenuItem();
            toolDescriptionMarkdownTable = new ToolStripMenuItem();
            toolDescriptionMarkdownCode = new ToolStripMenuItem();
            toolDescriptionS3 = new ToolStripSeparator();
            toolDescriptionUploadResource = new ToolStripButton();
            toolDescriptionUploadResourceFromClipboard = new ToolStripButton();
            labelLoadingHtml = new Label();
            label7 = new Label();
            buttonEditMarkdown = new Button();
            buttonViewHtml = new Button();
            textPriority = new TextBox();
            label6 = new Label();
            textTags = new TextBox();
            textFolder = new TextBox();
            textTopic = new TextBox();
            buttonFolderSearch = new Button();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            textNoteNumber = new TextBox();
            textFolderNumber = new TextBox();
            tabAttributes = new TabPage();
            buttonDeleteType = new Button();
            listViewAttributes = new ListView();
            textNoteType = new TextBox();
            buttonNoteType = new Button();
            label15 = new Label();
            buttonAttributeEdit = new Button();
            label10 = new Label();
            tabResources = new TabPage();
            labelPreview = new Label();
            textDescriptionResource = new TextBox();
            webViewResource = new KntWebView.KWebView();
            buttonSaveResource = new Button();
            buttonInsertLink = new Button();
            panelPreview = new Panel();
            linkViewFile = new LinkLabel();
            listViewResources = new ListView();
            buttonResourceEdit = new Button();
            buttonResourceDelete = new Button();
            buttonResourceAdd = new Button();
            label12 = new Label();
            tabTasks = new TabPage();
            textTaskTags = new TextBox();
            label17 = new Label();
            textTaskDescription = new TextBox();
            label16 = new Label();
            listViewTasks = new ListView();
            buttonTaskEdit = new Button();
            buttonTaskDelete = new Button();
            buttonTaskAdd = new Button();
            label11 = new Label();
            tabAlarms = new TabPage();
            listViewAlarms = new ListView();
            buttonEditAlarm = new Button();
            buttonDeleteAlarm = new Button();
            buttonAddAlarm = new Button();
            label4 = new Label();
            tabCode = new TabPage();
            textScriptCode = new TextBox();
            label9 = new Label();
            tabTraceNotes = new TabPage();
            listViewTraceNoteTo = new ListView();
            listViewTraceNoteFrom = new ListView();
            textTraceNodeType = new TextBox();
            buttonTraceNodeTypes = new Button();
            label5 = new Label();
            buttonTraceToEdit = new Button();
            buttonTraceToRemove = new Button();
            buttonTraceToAdd = new Button();
            buttonTraceFromEdit = new Button();
            buttonTraceFromRemove = new Button();
            buttonTraceFromAdd = new Button();
            label13 = new Label();
            label14 = new Label();
            toolTipHelps = new ToolTip(components);
            saveFileDialog = new SaveFileDialog();
            toolBarNoteEditor.SuspendLayout();
            panelForm.SuspendLayout();
            tabNoteData.SuspendLayout();
            tabBasicData.SuspendLayout();
            panelDescription.SuspendLayout();
            toolDescription.SuspendLayout();
            tabAttributes.SuspendLayout();
            tabResources.SuspendLayout();
            panelPreview.SuspendLayout();
            tabTasks.SuspendLayout();
            tabAlarms.SuspendLayout();
            tabCode.SuspendLayout();
            tabTraceNotes.SuspendLayout();
            SuspendLayout();
            // 
            // toolBarNoteEditor
            // 
            toolBarNoteEditor.BackColor = SystemColors.ControlLightLight;
            toolBarNoteEditor.ImageScalingSize = new Size(20, 20);
            toolBarNoteEditor.Items.AddRange(new ToolStripItem[] { buttonSave, buttonDelete, buttonUndo, toolStripS1, buttonPostIt, toolStripS2, buttonCheck, toolStripS3, buttonPrint, toolStripS4, buttonTools });
            toolBarNoteEditor.Location = new Point(0, 0);
            toolBarNoteEditor.Name = "toolBarNoteEditor";
            toolBarNoteEditor.RenderMode = ToolStripRenderMode.Professional;
            toolBarNoteEditor.Size = new Size(808, 25);
            toolBarNoteEditor.TabIndex = 6;
            toolBarNoteEditor.Text = "Toolbar KeyNotes";
            // 
            // buttonSave
            // 
            buttonSave.Image = (Image)resources.GetObject("buttonSave.Image");
            buttonSave.ImageScaling = ToolStripItemImageScaling.None;
            buttonSave.ImageTransparentColor = Color.Magenta;
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(57, 22);
            buttonSave.Text = "Save  ";
            buttonSave.Click += buttonToolBar_Click;
            // 
            // buttonDelete
            // 
            buttonDelete.Image = (Image)resources.GetObject("buttonDelete.Image");
            buttonDelete.ImageScaling = ToolStripItemImageScaling.None;
            buttonDelete.ImageTransparentColor = Color.Magenta;
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new Size(66, 22);
            buttonDelete.Text = "Delete  ";
            buttonDelete.Click += buttonToolBar_Click;
            // 
            // buttonUndo
            // 
            buttonUndo.DisplayStyle = ToolStripItemDisplayStyle.Image;
            buttonUndo.Enabled = false;
            buttonUndo.Image = (Image)resources.GetObject("buttonUndo.Image");
            buttonUndo.ImageScaling = ToolStripItemImageScaling.None;
            buttonUndo.ImageTransparentColor = Color.Magenta;
            buttonUndo.Name = "buttonUndo";
            buttonUndo.Size = new Size(23, 22);
            buttonUndo.Text = "Undo  ";
            buttonUndo.TextImageRelation = TextImageRelation.TextBeforeImage;
            buttonUndo.ToolTipText = "Undo changes";
            buttonUndo.Click += buttonToolBar_Click;
            // 
            // toolStripS1
            // 
            toolStripS1.Name = "toolStripS1";
            toolStripS1.Size = new Size(6, 25);
            // 
            // buttonPostIt
            // 
            buttonPostIt.Image = (Image)resources.GetObject("buttonPostIt.Image");
            buttonPostIt.ImageScaling = ToolStripItemImageScaling.None;
            buttonPostIt.ImageTransparentColor = Color.Magenta;
            buttonPostIt.Name = "buttonPostIt";
            buttonPostIt.Size = new Size(104, 22);
            buttonPostIt.Text = "View as Post-It";
            buttonPostIt.Click += buttonToolBar_Click;
            // 
            // toolStripS2
            // 
            toolStripS2.Name = "toolStripS2";
            toolStripS2.Size = new Size(6, 25);
            // 
            // buttonCheck
            // 
            buttonCheck.DisplayStyle = ToolStripItemDisplayStyle.Image;
            buttonCheck.Image = (Image)resources.GetObject("buttonCheck.Image");
            buttonCheck.ImageScaling = ToolStripItemImageScaling.None;
            buttonCheck.ImageTransparentColor = Color.Magenta;
            buttonCheck.Name = "buttonCheck";
            buttonCheck.Size = new Size(23, 22);
            buttonCheck.Text = "Check";
            // 
            // toolStripS3
            // 
            toolStripS3.Name = "toolStripS3";
            toolStripS3.Size = new Size(6, 25);
            // 
            // buttonPrint
            // 
            buttonPrint.Image = (Image)resources.GetObject("buttonPrint.Image");
            buttonPrint.ImageScaling = ToolStripItemImageScaling.None;
            buttonPrint.ImageTransparentColor = Color.Magenta;
            buttonPrint.Name = "buttonPrint";
            buttonPrint.Size = new Size(58, 22);
            buttonPrint.Text = "Print  ";
            buttonPrint.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // toolStripS4
            // 
            toolStripS4.Name = "toolStripS4";
            toolStripS4.Size = new Size(6, 25);
            // 
            // buttonTools
            // 
            buttonTools.DisplayStyle = ToolStripItemDisplayStyle.Image;
            buttonTools.DropDownItems.AddRange(new ToolStripItem[] { buttonInsertTemplate, toolStripToolS1, buttonExecuteKntScript });
            buttonTools.Image = (Image)resources.GetObject("buttonTools.Image");
            buttonTools.ImageScaling = ToolStripItemImageScaling.None;
            buttonTools.ImageTransparentColor = Color.Magenta;
            buttonTools.Name = "buttonTools";
            buttonTools.Size = new Size(29, 22);
            buttonTools.Text = "toolStripDropDownTools";
            // 
            // buttonInsertTemplate
            // 
            buttonInsertTemplate.Name = "buttonInsertTemplate";
            buttonInsertTemplate.ShortcutKeys = Keys.F9;
            buttonInsertTemplate.Size = new Size(217, 22);
            buttonInsertTemplate.Text = "Insertar template text ...";
            // 
            // toolStripToolS1
            // 
            toolStripToolS1.Name = "toolStripToolS1";
            toolStripToolS1.Size = new Size(214, 6);
            // 
            // buttonExecuteKntScript
            // 
            buttonExecuteKntScript.Image = (Image)resources.GetObject("buttonExecuteKntScript.Image");
            buttonExecuteKntScript.Name = "buttonExecuteKntScript";
            buttonExecuteKntScript.ShortcutKeys = Keys.F5;
            buttonExecuteKntScript.Size = new Size(217, 22);
            buttonExecuteKntScript.Text = "Execute AntScript code";
            buttonExecuteKntScript.Click += buttonToolBar_Click;
            // 
            // imageListTabNoteData
            // 
            imageListTabNoteData.ColorDepth = ColorDepth.Depth8Bit;
            imageListTabNoteData.ImageStream = (ImageListStreamer)resources.GetObject("imageListTabNoteData.ImageStream");
            imageListTabNoteData.TransparentColor = Color.Transparent;
            imageListTabNoteData.Images.SetKeyName(0, "alarm_16.png");
            imageListTabNoteData.Images.SetKeyName(1, "fileTestLight_16.png");
            imageListTabNoteData.Images.SetKeyName(2, "bookmarkLight_24.png");
            imageListTabNoteData.Images.SetKeyName(3, "books_16.png");
            imageListTabNoteData.Images.SetKeyName(4, "code_16.png");
            imageListTabNoteData.Images.SetKeyName(5, "libraryBooks_16.png");
            imageListTabNoteData.Images.SetKeyName(6, "tasks_16.png");
            imageListTabNoteData.Images.SetKeyName(7, "checkbox_16.png");
            imageListTabNoteData.Images.SetKeyName(8, "codgs_16.png");
            imageListTabNoteData.Images.SetKeyName(9, "code_16.png");
            imageListTabNoteData.Images.SetKeyName(10, "upload_16.png");
            // 
            // panelForm
            // 
            panelForm.Controls.Add(tabNoteData);
            panelForm.Dock = DockStyle.Fill;
            panelForm.Location = new Point(0, 25);
            panelForm.Margin = new Padding(4, 3, 4, 3);
            panelForm.Name = "panelForm";
            panelForm.Size = new Size(808, 612);
            panelForm.TabIndex = 39;
            // 
            // tabNoteData
            // 
            tabNoteData.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabNoteData.Controls.Add(tabBasicData);
            tabNoteData.Controls.Add(tabAttributes);
            tabNoteData.Controls.Add(tabResources);
            tabNoteData.Controls.Add(tabTasks);
            tabNoteData.Controls.Add(tabAlarms);
            tabNoteData.Controls.Add(tabCode);
            tabNoteData.Controls.Add(tabTraceNotes);
            tabNoteData.ImageList = imageListTabNoteData;
            tabNoteData.Location = new Point(4, 4);
            tabNoteData.Margin = new Padding(4, 3, 4, 3);
            tabNoteData.Name = "tabNoteData";
            tabNoteData.Padding = new Point(4, 6);
            tabNoteData.SelectedIndex = 0;
            tabNoteData.Size = new Size(802, 603);
            tabNoteData.TabIndex = 8;
            // 
            // tabBasicData
            // 
            tabBasicData.Controls.Add(buttonNavigate);
            tabBasicData.Controls.Add(textStatus);
            tabBasicData.Controls.Add(label8);
            tabBasicData.Controls.Add(panelDescription);
            tabBasicData.Controls.Add(labelLoadingHtml);
            tabBasicData.Controls.Add(label7);
            tabBasicData.Controls.Add(buttonEditMarkdown);
            tabBasicData.Controls.Add(buttonViewHtml);
            tabBasicData.Controls.Add(textPriority);
            tabBasicData.Controls.Add(label6);
            tabBasicData.Controls.Add(textTags);
            tabBasicData.Controls.Add(textFolder);
            tabBasicData.Controls.Add(textTopic);
            tabBasicData.Controls.Add(buttonFolderSearch);
            tabBasicData.Controls.Add(label3);
            tabBasicData.Controls.Add(label2);
            tabBasicData.Controls.Add(label1);
            tabBasicData.Controls.Add(textNoteNumber);
            tabBasicData.Controls.Add(textFolderNumber);
            tabBasicData.ImageIndex = 1;
            tabBasicData.Location = new Point(4, 30);
            tabBasicData.Margin = new Padding(4, 3, 4, 3);
            tabBasicData.Name = "tabBasicData";
            tabBasicData.Padding = new Padding(4, 3, 4, 3);
            tabBasicData.Size = new Size(794, 569);
            tabBasicData.TabIndex = 0;
            tabBasicData.Text = "Basic data  ";
            tabBasicData.UseVisualStyleBackColor = true;
            // 
            // buttonNavigate
            // 
            buttonNavigate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonNavigate.Location = new Point(706, 105);
            buttonNavigate.Margin = new Padding(4, 3, 4, 3);
            buttonNavigate.Name = "buttonNavigate";
            buttonNavigate.Size = new Size(80, 24);
            buttonNavigate.TabIndex = 57;
            buttonNavigate.Text = "Navigate";
            buttonNavigate.UseVisualStyleBackColor = true;
            buttonNavigate.Click += buttonNavigate_Click;
            // 
            // textStatus
            // 
            textStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            textStatus.BackColor = SystemColors.Control;
            textStatus.Enabled = false;
            textStatus.Location = new Point(593, 68);
            textStatus.Margin = new Padding(4, 3, 4, 3);
            textStatus.Name = "textStatus";
            textStatus.Size = new Size(193, 23);
            textStatus.TabIndex = 4;
            // 
            // label8
            // 
            label8.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label8.Location = new Point(544, 68);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(45, 18);
            label8.TabIndex = 56;
            label8.Text = "Status:";
            label8.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelDescription
            // 
            panelDescription.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelDescription.Controls.Add(webView2);
            panelDescription.Controls.Add(htmlDescription);
            panelDescription.Controls.Add(textDescription);
            panelDescription.Controls.Add(toolDescription);
            panelDescription.Location = new Point(4, 132);
            panelDescription.Name = "panelDescription";
            panelDescription.Size = new Size(782, 434);
            panelDescription.TabIndex = 55;
            panelDescription.Visible = false;
            // 
            // webView2
            // 
            webView2.EnableUrlBox = true;
            webView2.ForceHttps = false;
            webView2.IsInitialized = false;
            webView2.Location = new Point(40, 127);
            webView2.Margin = new Padding(3, 4, 3, 4);
            webView2.Name = "webView2";
            webView2.ShowNavigationTools = false;
            webView2.ShowStatusInfo = false;
            webView2.Size = new Size(277, 166);
            webView2.TabIndex = 11;
            webView2.TextUrl = "";
            webView2.Visible = false;
            // 
            // htmlDescription
            // 
            htmlDescription.InnerText = null;
            htmlDescription.Location = new Point(324, 14);
            htmlDescription.Name = "htmlDescription";
            htmlDescription.Size = new Size(277, 88);
            htmlDescription.TabIndex = 8;
            // 
            // textDescription
            // 
            textDescription.Font = new Font("Courier New", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            textDescription.Location = new Point(40, 14);
            textDescription.Margin = new Padding(4, 3, 4, 3);
            textDescription.MaxLength = 0;
            textDescription.Multiline = true;
            textDescription.Name = "textDescription";
            textDescription.ScrollBars = ScrollBars.Both;
            textDescription.Size = new Size(277, 88);
            textDescription.TabIndex = 7;
            // 
            // toolDescription
            // 
            toolDescription.Dock = DockStyle.Left;
            toolDescription.ImageScalingSize = new Size(20, 20);
            toolDescription.Items.AddRange(new ToolStripItem[] { toolDescriptionHtml, toolDescriptionMarkdown, toolDescriptionS3, toolDescriptionUploadResource, toolDescriptionUploadResourceFromClipboard });
            toolDescription.Location = new Point(0, 0);
            toolDescription.Name = "toolDescription";
            toolDescription.Size = new Size(34, 434);
            toolDescription.TabIndex = 0;
            toolDescription.Text = "Tool description editor";
            // 
            // toolDescriptionHtml
            // 
            toolDescriptionHtml.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolDescriptionHtml.DropDownItems.AddRange(new ToolStripItem[] { toolDescriptionHtmlTitle1, toolDescriptionHtmlTitle2, toolDescriptionHtmlTitle3, toolDescriptionHtmlTitle4, toolDescriptionHtmlS3, toolDescriptionHtmlEdit });
            toolDescriptionHtml.Image = (Image)resources.GetObject("toolDescriptionHtml.Image");
            toolDescriptionHtml.ImageTransparentColor = Color.Magenta;
            toolDescriptionHtml.Name = "toolDescriptionHtml";
            toolDescriptionHtml.Size = new Size(31, 24);
            toolDescriptionHtml.Text = "H";
            // 
            // toolDescriptionHtmlTitle1
            // 
            toolDescriptionHtmlTitle1.Name = "toolDescriptionHtmlTitle1";
            toolDescriptionHtmlTitle1.Size = new Size(163, 22);
            toolDescriptionHtmlTitle1.Text = "Title 1";
            toolDescriptionHtmlTitle1.Click += toolDescriptionHtml_Click;
            // 
            // toolDescriptionHtmlTitle2
            // 
            toolDescriptionHtmlTitle2.Name = "toolDescriptionHtmlTitle2";
            toolDescriptionHtmlTitle2.Size = new Size(163, 22);
            toolDescriptionHtmlTitle2.Text = "Title 2";
            toolDescriptionHtmlTitle2.Click += toolDescriptionHtml_Click;
            // 
            // toolDescriptionHtmlTitle3
            // 
            toolDescriptionHtmlTitle3.Name = "toolDescriptionHtmlTitle3";
            toolDescriptionHtmlTitle3.Size = new Size(163, 22);
            toolDescriptionHtmlTitle3.Text = "Title 3";
            toolDescriptionHtmlTitle3.Click += toolDescriptionHtml_Click;
            // 
            // toolDescriptionHtmlTitle4
            // 
            toolDescriptionHtmlTitle4.Name = "toolDescriptionHtmlTitle4";
            toolDescriptionHtmlTitle4.Size = new Size(163, 22);
            toolDescriptionHtmlTitle4.Text = "Title 4";
            toolDescriptionHtmlTitle4.Click += toolDescriptionHtml_Click;
            // 
            // toolDescriptionHtmlS3
            // 
            toolDescriptionHtmlS3.Name = "toolDescriptionHtmlS3";
            toolDescriptionHtmlS3.Size = new Size(160, 6);
            // 
            // toolDescriptionHtmlEdit
            // 
            toolDescriptionHtmlEdit.Name = "toolDescriptionHtmlEdit";
            toolDescriptionHtmlEdit.Size = new Size(163, 22);
            toolDescriptionHtmlEdit.Text = "Edit html code ...";
            toolDescriptionHtmlEdit.Click += toolDescriptionHtml_Click;
            // 
            // toolDescriptionMarkdown
            // 
            toolDescriptionMarkdown.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolDescriptionMarkdown.DropDownItems.AddRange(new ToolStripItem[] { toolDescriptionMarkdownH1, toolDescriptionMarkdownH2, toolDescriptionMarkdownH3, toolDescriptionMarkdownH4, toolDescriptionMarkdownS1, toolDescriptionMarkdownBold, toolDescriptionMarkdownStrikethrough, toolDescriptionMarkdownItalic, toolDescriptionMarkdownS2, toolDescriptionMarkdownList, toolDescriptionMarkdownListOrdered, toolDescriptionMarkdownLine, toolDescriptionMarkdownLink, toolDescriptionMarkdownImage, toolDescriptionMarkdownTable, toolDescriptionMarkdownCode });
            toolDescriptionMarkdown.Image = (Image)resources.GetObject("toolDescriptionMarkdown.Image");
            toolDescriptionMarkdown.ImageTransparentColor = Color.Magenta;
            toolDescriptionMarkdown.Name = "toolDescriptionMarkdown";
            toolDescriptionMarkdown.Size = new Size(31, 24);
            toolDescriptionMarkdown.Text = "Markdown";
            // 
            // toolDescriptionMarkdownH1
            // 
            toolDescriptionMarkdownH1.Name = "toolDescriptionMarkdownH1";
            toolDescriptionMarkdownH1.Size = new Size(146, 22);
            toolDescriptionMarkdownH1.Text = "Title 1";
            toolDescriptionMarkdownH1.Click += toolDescriptionMarkdown_Click;
            // 
            // toolDescriptionMarkdownH2
            // 
            toolDescriptionMarkdownH2.Name = "toolDescriptionMarkdownH2";
            toolDescriptionMarkdownH2.Size = new Size(146, 22);
            toolDescriptionMarkdownH2.Text = "Title 2";
            toolDescriptionMarkdownH2.Click += toolDescriptionMarkdown_Click;
            // 
            // toolDescriptionMarkdownH3
            // 
            toolDescriptionMarkdownH3.Name = "toolDescriptionMarkdownH3";
            toolDescriptionMarkdownH3.Size = new Size(146, 22);
            toolDescriptionMarkdownH3.Text = "Title 3";
            toolDescriptionMarkdownH3.Click += toolDescriptionMarkdown_Click;
            // 
            // toolDescriptionMarkdownH4
            // 
            toolDescriptionMarkdownH4.Name = "toolDescriptionMarkdownH4";
            toolDescriptionMarkdownH4.Size = new Size(146, 22);
            toolDescriptionMarkdownH4.Text = "Title 4";
            toolDescriptionMarkdownH4.Click += toolDescriptionMarkdown_Click;
            // 
            // toolDescriptionMarkdownS1
            // 
            toolDescriptionMarkdownS1.Name = "toolDescriptionMarkdownS1";
            toolDescriptionMarkdownS1.Size = new Size(143, 6);
            // 
            // toolDescriptionMarkdownBold
            // 
            toolDescriptionMarkdownBold.Name = "toolDescriptionMarkdownBold";
            toolDescriptionMarkdownBold.Size = new Size(146, 22);
            toolDescriptionMarkdownBold.Text = "Bold";
            toolDescriptionMarkdownBold.Click += toolDescriptionMarkdown_Click;
            // 
            // toolDescriptionMarkdownStrikethrough
            // 
            toolDescriptionMarkdownStrikethrough.Name = "toolDescriptionMarkdownStrikethrough";
            toolDescriptionMarkdownStrikethrough.Size = new Size(146, 22);
            toolDescriptionMarkdownStrikethrough.Text = "Strikethrough";
            toolDescriptionMarkdownStrikethrough.Click += toolDescriptionMarkdown_Click;
            // 
            // toolDescriptionMarkdownItalic
            // 
            toolDescriptionMarkdownItalic.Name = "toolDescriptionMarkdownItalic";
            toolDescriptionMarkdownItalic.Size = new Size(146, 22);
            toolDescriptionMarkdownItalic.Text = "Italic";
            toolDescriptionMarkdownItalic.Click += toolDescriptionMarkdown_Click;
            // 
            // toolDescriptionMarkdownS2
            // 
            toolDescriptionMarkdownS2.Name = "toolDescriptionMarkdownS2";
            toolDescriptionMarkdownS2.Size = new Size(143, 6);
            // 
            // toolDescriptionMarkdownList
            // 
            toolDescriptionMarkdownList.Name = "toolDescriptionMarkdownList";
            toolDescriptionMarkdownList.Size = new Size(146, 22);
            toolDescriptionMarkdownList.Text = "List";
            toolDescriptionMarkdownList.Click += toolDescriptionMarkdown_Click;
            // 
            // toolDescriptionMarkdownListOrdered
            // 
            toolDescriptionMarkdownListOrdered.Name = "toolDescriptionMarkdownListOrdered";
            toolDescriptionMarkdownListOrdered.Size = new Size(146, 22);
            toolDescriptionMarkdownListOrdered.Text = "ListOrdered";
            toolDescriptionMarkdownListOrdered.Click += toolDescriptionMarkdown_Click;
            // 
            // toolDescriptionMarkdownLine
            // 
            toolDescriptionMarkdownLine.Name = "toolDescriptionMarkdownLine";
            toolDescriptionMarkdownLine.Size = new Size(146, 22);
            toolDescriptionMarkdownLine.Text = "Line";
            toolDescriptionMarkdownLine.Click += toolDescriptionMarkdown_Click;
            // 
            // toolDescriptionMarkdownLink
            // 
            toolDescriptionMarkdownLink.Name = "toolDescriptionMarkdownLink";
            toolDescriptionMarkdownLink.Size = new Size(146, 22);
            toolDescriptionMarkdownLink.Text = "Link";
            toolDescriptionMarkdownLink.Click += toolDescriptionMarkdown_Click;
            // 
            // toolDescriptionMarkdownImage
            // 
            toolDescriptionMarkdownImage.Name = "toolDescriptionMarkdownImage";
            toolDescriptionMarkdownImage.Size = new Size(146, 22);
            toolDescriptionMarkdownImage.Text = "Image";
            toolDescriptionMarkdownImage.Click += toolDescriptionMarkdown_Click;
            // 
            // toolDescriptionMarkdownTable
            // 
            toolDescriptionMarkdownTable.Name = "toolDescriptionMarkdownTable";
            toolDescriptionMarkdownTable.Size = new Size(146, 22);
            toolDescriptionMarkdownTable.Text = "Table";
            toolDescriptionMarkdownTable.Click += toolDescriptionMarkdown_Click;
            // 
            // toolDescriptionMarkdownCode
            // 
            toolDescriptionMarkdownCode.Name = "toolDescriptionMarkdownCode";
            toolDescriptionMarkdownCode.Size = new Size(146, 22);
            toolDescriptionMarkdownCode.Text = "Code";
            toolDescriptionMarkdownCode.Click += toolDescriptionMarkdown_Click;
            // 
            // toolDescriptionS3
            // 
            toolDescriptionS3.Name = "toolDescriptionS3";
            toolDescriptionS3.Size = new Size(31, 6);
            // 
            // toolDescriptionUploadResource
            // 
            toolDescriptionUploadResource.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolDescriptionUploadResource.Image = (Image)resources.GetObject("toolDescriptionUploadResource.Image");
            toolDescriptionUploadResource.ImageTransparentColor = Color.Magenta;
            toolDescriptionUploadResource.Name = "toolDescriptionUploadResource";
            toolDescriptionUploadResource.Size = new Size(31, 24);
            toolDescriptionUploadResource.Text = "Upload resource";
            toolDescriptionUploadResource.Click += toolDescriptionUploadResource_Click;
            // 
            // toolDescriptionUploadResourceFromClipboard
            // 
            toolDescriptionUploadResourceFromClipboard.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolDescriptionUploadResourceFromClipboard.Image = (Image)resources.GetObject("toolDescriptionUploadResourceFromClipboard.Image");
            toolDescriptionUploadResourceFromClipboard.ImageTransparentColor = Color.Magenta;
            toolDescriptionUploadResourceFromClipboard.Name = "toolDescriptionUploadResourceFromClipboard";
            toolDescriptionUploadResourceFromClipboard.Size = new Size(31, 24);
            toolDescriptionUploadResourceFromClipboard.Text = "Add imge from clipboard";
            toolDescriptionUploadResourceFromClipboard.Click += toolDescriptionUploadResourceFromClipboard_Click;
            // 
            // labelLoadingHtml
            // 
            labelLoadingHtml.AutoSize = true;
            labelLoadingHtml.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            labelLoadingHtml.Location = new Point(136, 112);
            labelLoadingHtml.Name = "labelLoadingHtml";
            labelLoadingHtml.Size = new Size(138, 15);
            labelLoadingHtml.TabIndex = 54;
            labelLoadingHtml.Text = "Loading html content ...";
            labelLoadingHtml.Visible = false;
            // 
            // label7
            // 
            label7.Location = new Point(9, 68);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(64, 18);
            label7.TabIndex = 50;
            label7.Text = "Priority:";
            label7.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonEditMarkdown
            // 
            buttonEditMarkdown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonEditMarkdown.ImageAlign = ContentAlignment.MiddleLeft;
            buttonEditMarkdown.ImageList = imageListTabNoteData;
            buttonEditMarkdown.Location = new Point(542, 105);
            buttonEditMarkdown.Margin = new Padding(4, 3, 4, 3);
            buttonEditMarkdown.Name = "buttonEditMarkdown";
            buttonEditMarkdown.Size = new Size(79, 24);
            buttonEditMarkdown.TabIndex = 5;
            buttonEditMarkdown.Text = "Markdown";
            buttonEditMarkdown.UseVisualStyleBackColor = true;
            buttonEditMarkdown.Click += buttonEditMarkdown_Click;
            // 
            // buttonViewHtml
            // 
            buttonViewHtml.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonViewHtml.Location = new Point(624, 105);
            buttonViewHtml.Margin = new Padding(4, 3, 4, 3);
            buttonViewHtml.Name = "buttonViewHtml";
            buttonViewHtml.Size = new Size(80, 24);
            buttonViewHtml.TabIndex = 6;
            buttonViewHtml.Text = "Html editor";
            buttonViewHtml.UseVisualStyleBackColor = true;
            buttonViewHtml.Click += buttonViewHtml_Click;
            // 
            // textPriority
            // 
            textPriority.Location = new Point(81, 68);
            textPriority.Margin = new Padding(4, 3, 4, 3);
            textPriority.Name = "textPriority";
            textPriority.Size = new Size(75, 23);
            textPriority.TabIndex = 3;
            // 
            // label6
            // 
            label6.Location = new Point(9, 112);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(103, 18);
            label6.TabIndex = 44;
            label6.Text = "Content:";
            // 
            // textTags
            // 
            textTags.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textTags.Location = new Point(211, 68);
            textTags.Margin = new Padding(4, 3, 4, 3);
            textTags.MaxLength = 255;
            textTags.Name = "textTags";
            textTags.Size = new Size(315, 23);
            textTags.TabIndex = 4;
            // 
            // textFolder
            // 
            textFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textFolder.Enabled = false;
            textFolder.Location = new Point(81, 38);
            textFolder.Margin = new Padding(4, 3, 4, 3);
            textFolder.MaxLength = 255;
            textFolder.Name = "textFolder";
            textFolder.Size = new Size(625, 23);
            textFolder.TabIndex = 1;
            // 
            // textTopic
            // 
            textTopic.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textTopic.Location = new Point(81, 8);
            textTopic.Margin = new Padding(4, 3, 4, 3);
            textTopic.MaxLength = 255;
            textTopic.Name = "textTopic";
            textTopic.Size = new Size(625, 23);
            textTopic.TabIndex = 0;
            // 
            // buttonFolderSearch
            // 
            buttonFolderSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonFolderSearch.BackColor = SystemColors.Control;
            buttonFolderSearch.FlatStyle = FlatStyle.System;
            buttonFolderSearch.Location = new Point(760, 38);
            buttonFolderSearch.Margin = new Padding(4, 3, 4, 3);
            buttonFolderSearch.Name = "buttonFolderSearch";
            buttonFolderSearch.Size = new Size(27, 23);
            buttonFolderSearch.TabIndex = 2;
            buttonFolderSearch.Text = "...";
            buttonFolderSearch.UseVisualStyleBackColor = false;
            buttonFolderSearch.Click += buttonFolderSearch_Click;
            // 
            // label3
            // 
            label3.Location = new Point(172, 68);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(35, 18);
            label3.TabIndex = 36;
            label3.Text = "Tags:";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            label2.Location = new Point(9, 38);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(64, 18);
            label2.TabIndex = 34;
            label2.Text = "Folder:";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            label1.Location = new Point(9, 8);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(64, 18);
            label1.TabIndex = 33;
            label1.Text = "Topic:";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // textNoteNumber
            // 
            textNoteNumber.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            textNoteNumber.BackColor = SystemColors.Control;
            textNoteNumber.Enabled = false;
            textNoteNumber.Location = new Point(702, 8);
            textNoteNumber.Name = "textNoteNumber";
            textNoteNumber.Size = new Size(84, 23);
            textNoteNumber.TabIndex = 51;
            textNoteNumber.TabStop = false;
            textNoteNumber.TextAlign = HorizontalAlignment.Right;
            // 
            // textFolderNumber
            // 
            textFolderNumber.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            textFolderNumber.BackColor = SystemColors.Control;
            textFolderNumber.Enabled = false;
            textFolderNumber.Location = new Point(702, 38);
            textFolderNumber.Name = "textFolderNumber";
            textFolderNumber.Size = new Size(54, 23);
            textFolderNumber.TabIndex = 52;
            textFolderNumber.TabStop = false;
            textFolderNumber.TextAlign = HorizontalAlignment.Right;
            // 
            // tabAttributes
            // 
            tabAttributes.Controls.Add(buttonDeleteType);
            tabAttributes.Controls.Add(listViewAttributes);
            tabAttributes.Controls.Add(textNoteType);
            tabAttributes.Controls.Add(buttonNoteType);
            tabAttributes.Controls.Add(label15);
            tabAttributes.Controls.Add(buttonAttributeEdit);
            tabAttributes.Controls.Add(label10);
            tabAttributes.ImageIndex = 5;
            tabAttributes.Location = new Point(4, 30);
            tabAttributes.Margin = new Padding(4, 3, 4, 3);
            tabAttributes.Name = "tabAttributes";
            tabAttributes.Size = new Size(794, 567);
            tabAttributes.TabIndex = 3;
            tabAttributes.Text = "Attributes  ";
            tabAttributes.UseVisualStyleBackColor = true;
            // 
            // buttonDeleteType
            // 
            buttonDeleteType.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonDeleteType.BackColor = SystemColors.Control;
            buttonDeleteType.FlatStyle = FlatStyle.System;
            buttonDeleteType.Location = new Point(730, 16);
            buttonDeleteType.Margin = new Padding(4, 3, 4, 3);
            buttonDeleteType.Name = "buttonDeleteType";
            buttonDeleteType.Size = new Size(27, 23);
            buttonDeleteType.TabIndex = 47;
            buttonDeleteType.Text = "X";
            toolTipHelps.SetToolTip(buttonDeleteType, "Delete type");
            buttonDeleteType.UseVisualStyleBackColor = false;
            buttonDeleteType.Click += buttonDeleteType_Click;
            // 
            // listViewAttributes
            // 
            listViewAttributes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewAttributes.Location = new Point(10, 79);
            listViewAttributes.MultiSelect = false;
            listViewAttributes.Name = "listViewAttributes";
            listViewAttributes.Size = new Size(774, 476);
            listViewAttributes.TabIndex = 46;
            listViewAttributes.UseCompatibleStateImageBehavior = false;
            listViewAttributes.DoubleClick += listViewAttributes_DoubleClick;
            listViewAttributes.Resize += listView_Resize;
            // 
            // textNoteType
            // 
            textNoteType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textNoteType.Enabled = false;
            textNoteType.Location = new Point(103, 16);
            textNoteType.Margin = new Padding(4, 3, 4, 3);
            textNoteType.MaxLength = 255;
            textNoteType.Name = "textNoteType";
            textNoteType.ShortcutsEnabled = false;
            textNoteType.Size = new Size(624, 23);
            textNoteType.TabIndex = 44;
            // 
            // buttonNoteType
            // 
            buttonNoteType.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonNoteType.BackColor = SystemColors.Control;
            buttonNoteType.FlatStyle = FlatStyle.System;
            buttonNoteType.Location = new Point(758, 16);
            buttonNoteType.Margin = new Padding(4, 3, 4, 3);
            buttonNoteType.Name = "buttonNoteType";
            buttonNoteType.Size = new Size(27, 23);
            buttonNoteType.TabIndex = 45;
            buttonNoteType.Text = "...";
            toolTipHelps.SetToolTip(buttonNoteType, "Select new type");
            buttonNoteType.UseVisualStyleBackColor = false;
            buttonNoteType.Click += buttonNoteType_Click;
            // 
            // label15
            // 
            label15.Location = new Point(10, 16);
            label15.Margin = new Padding(4, 0, 4, 0);
            label15.Name = "label15";
            label15.Size = new Size(85, 18);
            label15.TabIndex = 43;
            label15.Text = "Node type:";
            label15.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonAttributeEdit
            // 
            buttonAttributeEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonAttributeEdit.Location = new Point(758, 54);
            buttonAttributeEdit.Margin = new Padding(4, 3, 4, 3);
            buttonAttributeEdit.Name = "buttonAttributeEdit";
            buttonAttributeEdit.Size = new Size(27, 23);
            buttonAttributeEdit.TabIndex = 12;
            buttonAttributeEdit.Text = "...";
            buttonAttributeEdit.UseVisualStyleBackColor = true;
            buttonAttributeEdit.Click += buttonAttributeEdit_Click;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(10, 61);
            label10.Margin = new Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new Size(62, 15);
            label10.TabIndex = 3;
            label10.Text = "Attributes:";
            // 
            // tabResources
            // 
            tabResources.Controls.Add(labelPreview);
            tabResources.Controls.Add(textDescriptionResource);
            tabResources.Controls.Add(webViewResource);
            tabResources.Controls.Add(buttonSaveResource);
            tabResources.Controls.Add(buttonInsertLink);
            tabResources.Controls.Add(panelPreview);
            tabResources.Controls.Add(listViewResources);
            tabResources.Controls.Add(buttonResourceEdit);
            tabResources.Controls.Add(buttonResourceDelete);
            tabResources.Controls.Add(buttonResourceAdd);
            tabResources.Controls.Add(label12);
            tabResources.ImageIndex = 3;
            tabResources.Location = new Point(4, 30);
            tabResources.Margin = new Padding(4, 3, 4, 3);
            tabResources.Name = "tabResources";
            tabResources.Size = new Size(794, 567);
            tabResources.TabIndex = 4;
            tabResources.Text = "Resources  ";
            tabResources.ToolTipText = "Save resource file";
            tabResources.UseVisualStyleBackColor = true;
            // 
            // labelPreview
            // 
            labelPreview.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelPreview.AutoSize = true;
            labelPreview.Location = new Point(685, 18);
            labelPreview.Name = "labelPreview";
            labelPreview.Size = new Size(99, 15);
            labelPreview.TabIndex = 24;
            labelPreview.Text = "Preview resource:";
            // 
            // textDescriptionResource
            // 
            textDescriptionResource.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textDescriptionResource.Location = new Point(396, 511);
            textDescriptionResource.Multiline = true;
            textDescriptionResource.Name = "textDescriptionResource";
            textDescriptionResource.Size = new Size(392, 52);
            textDescriptionResource.TabIndex = 19;
            // 
            // webViewResource
            // 
            webViewResource.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webViewResource.EnableUrlBox = true;
            webViewResource.ForceHttps = false;
            webViewResource.IsInitialized = false;
            webViewResource.Location = new Point(392, 39);
            webViewResource.Margin = new Padding(3, 4, 3, 4);
            webViewResource.Name = "webViewResource";
            webViewResource.ShowNavigationTools = false;
            webViewResource.ShowStatusInfo = false;
            webViewResource.Size = new Size(392, 155);
            webViewResource.TabIndex = 21;
            webViewResource.TextUrl = "";
            webViewResource.Visible = false;
            // 
            // buttonSaveResource
            // 
            buttonSaveResource.Location = new Point(434, 10);
            buttonSaveResource.Name = "buttonSaveResource";
            buttonSaveResource.Size = new Size(32, 23);
            buttonSaveResource.TabIndex = 20;
            buttonSaveResource.Text = "S";
            toolTipHelps.SetToolTip(buttonSaveResource, "Save resource");
            buttonSaveResource.UseVisualStyleBackColor = true;
            buttonSaveResource.Click += buttonSaveResource_Click;
            // 
            // buttonInsertLink
            // 
            buttonInsertLink.Location = new Point(396, 10);
            buttonInsertLink.Name = "buttonInsertLink";
            buttonInsertLink.Size = new Size(32, 23);
            buttonInsertLink.TabIndex = 19;
            buttonInsertLink.Text = "<>";
            toolTipHelps.SetToolTip(buttonInsertLink, "Insert resouce link in note description ");
            buttonInsertLink.UseVisualStyleBackColor = true;
            buttonInsertLink.Click += buttonInsertLink_Click;
            // 
            // panelPreview
            // 
            panelPreview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelPreview.BorderStyle = BorderStyle.FixedSingle;
            panelPreview.Controls.Add(linkViewFile);
            panelPreview.Location = new Point(392, 202);
            panelPreview.Name = "panelPreview";
            panelPreview.Size = new Size(392, 82);
            panelPreview.TabIndex = 18;
            panelPreview.Visible = false;
            // 
            // linkViewFile
            // 
            linkViewFile.AutoSize = true;
            linkViewFile.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            linkViewFile.Location = new Point(13, 14);
            linkViewFile.Name = "linkViewFile";
            linkViewFile.Size = new Size(128, 19);
            linkViewFile.TabIndex = 1;
            linkViewFile.TabStop = true;
            linkViewFile.Text = "View resource file ...";
            linkViewFile.Click += linkViewFile_Click;
            // 
            // listViewResources
            // 
            listViewResources.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            listViewResources.Location = new Point(5, 36);
            listViewResources.MultiSelect = false;
            listViewResources.Name = "listViewResources";
            listViewResources.Size = new Size(381, 527);
            listViewResources.TabIndex = 14;
            listViewResources.UseCompatibleStateImageBehavior = false;
            listViewResources.SelectedIndexChanged += listViewResources_SelectedIndexChanged;
            listViewResources.DoubleClick += listViewResources_DoubleClick;
            // 
            // buttonResourceEdit
            // 
            buttonResourceEdit.Location = new Point(360, 10);
            buttonResourceEdit.Margin = new Padding(4, 3, 4, 3);
            buttonResourceEdit.Name = "buttonResourceEdit";
            buttonResourceEdit.Size = new Size(27, 23);
            buttonResourceEdit.TabIndex = 12;
            buttonResourceEdit.Text = "...";
            buttonResourceEdit.UseVisualStyleBackColor = true;
            buttonResourceEdit.Click += buttonResourceEdit_Click;
            // 
            // buttonResourceDelete
            // 
            buttonResourceDelete.Location = new Point(330, 10);
            buttonResourceDelete.Margin = new Padding(4, 3, 4, 3);
            buttonResourceDelete.Name = "buttonResourceDelete";
            buttonResourceDelete.Size = new Size(27, 23);
            buttonResourceDelete.TabIndex = 11;
            buttonResourceDelete.Text = "-";
            buttonResourceDelete.UseVisualStyleBackColor = true;
            buttonResourceDelete.Click += buttonResourceDelete_Click;
            // 
            // buttonResourceAdd
            // 
            buttonResourceAdd.Location = new Point(300, 10);
            buttonResourceAdd.Margin = new Padding(4, 3, 4, 3);
            buttonResourceAdd.Name = "buttonResourceAdd";
            buttonResourceAdd.Size = new Size(27, 23);
            buttonResourceAdd.TabIndex = 10;
            buttonResourceAdd.Text = "+";
            toolTipHelps.SetToolTip(buttonResourceAdd, "Add resource");
            buttonResourceAdd.UseVisualStyleBackColor = true;
            buttonResourceAdd.Click += buttonResourceAdd_Click;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(6, 14);
            label12.Margin = new Padding(4, 0, 4, 0);
            label12.Name = "label12";
            label12.Size = new Size(63, 15);
            label12.TabIndex = 5;
            label12.Text = "Resources:";
            // 
            // tabTasks
            // 
            tabTasks.Controls.Add(textTaskTags);
            tabTasks.Controls.Add(label17);
            tabTasks.Controls.Add(textTaskDescription);
            tabTasks.Controls.Add(label16);
            tabTasks.Controls.Add(listViewTasks);
            tabTasks.Controls.Add(buttonTaskEdit);
            tabTasks.Controls.Add(buttonTaskDelete);
            tabTasks.Controls.Add(buttonTaskAdd);
            tabTasks.Controls.Add(label11);
            tabTasks.ImageIndex = 6;
            tabTasks.Location = new Point(4, 30);
            tabTasks.Margin = new Padding(4, 3, 4, 3);
            tabTasks.Name = "tabTasks";
            tabTasks.Size = new Size(794, 567);
            tabTasks.TabIndex = 2;
            tabTasks.Text = "Tasks  ";
            tabTasks.UseVisualStyleBackColor = true;
            // 
            // textTaskTags
            // 
            textTaskTags.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textTaskTags.Location = new Point(400, 525);
            textTaskTags.Name = "textTaskTags";
            textTaskTags.Size = new Size(389, 23);
            textTaskTags.TabIndex = 16;
            // 
            // label17
            // 
            label17.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label17.AutoSize = true;
            label17.Location = new Point(400, 502);
            label17.Name = "label17";
            label17.Size = new Size(33, 15);
            label17.TabIndex = 16;
            label17.Text = "Tags:";
            // 
            // textTaskDescription
            // 
            textTaskDescription.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textTaskDescription.Location = new Point(400, 36);
            textTaskDescription.Multiline = true;
            textTaskDescription.Name = "textTaskDescription";
            textTaskDescription.Size = new Size(388, 463);
            textTaskDescription.TabIndex = 15;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(400, 16);
            label16.Name = "label16";
            label16.Size = new Size(94, 15);
            label16.TabIndex = 14;
            label16.Text = "Task description:";
            // 
            // listViewTasks
            // 
            listViewTasks.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            listViewTasks.Location = new Point(5, 36);
            listViewTasks.MultiSelect = false;
            listViewTasks.Name = "listViewTasks";
            listViewTasks.Size = new Size(387, 528);
            listViewTasks.TabIndex = 13;
            listViewTasks.UseCompatibleStateImageBehavior = false;
            listViewTasks.SelectedIndexChanged += listViewTasks_SelectedIndexChanged;
            listViewTasks.DoubleClick += listViewTasks_DoubleClick;
            listViewTasks.Resize += listView_Resize;
            // 
            // buttonTaskEdit
            // 
            buttonTaskEdit.Location = new Point(360, 10);
            buttonTaskEdit.Margin = new Padding(4, 3, 4, 3);
            buttonTaskEdit.Name = "buttonTaskEdit";
            buttonTaskEdit.Size = new Size(27, 23);
            buttonTaskEdit.TabIndex = 12;
            buttonTaskEdit.Text = "...";
            buttonTaskEdit.UseVisualStyleBackColor = true;
            buttonTaskEdit.Click += buttonTaskEdit_Click;
            // 
            // buttonTaskDelete
            // 
            buttonTaskDelete.Location = new Point(330, 10);
            buttonTaskDelete.Margin = new Padding(4, 3, 4, 3);
            buttonTaskDelete.Name = "buttonTaskDelete";
            buttonTaskDelete.Size = new Size(27, 23);
            buttonTaskDelete.TabIndex = 11;
            buttonTaskDelete.Text = "-";
            buttonTaskDelete.UseVisualStyleBackColor = true;
            buttonTaskDelete.Click += buttonTaskDelete_Click;
            // 
            // buttonTaskAdd
            // 
            buttonTaskAdd.Location = new Point(300, 10);
            buttonTaskAdd.Margin = new Padding(4, 3, 4, 3);
            buttonTaskAdd.Name = "buttonTaskAdd";
            buttonTaskAdd.Size = new Size(27, 23);
            buttonTaskAdd.TabIndex = 10;
            buttonTaskAdd.Text = "+";
            buttonTaskAdd.UseVisualStyleBackColor = true;
            buttonTaskAdd.Click += buttonTaskAdd_Click;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(6, 16);
            label11.Margin = new Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new Size(37, 15);
            label11.TabIndex = 5;
            label11.Text = "Tasks:";
            // 
            // tabAlarms
            // 
            tabAlarms.Controls.Add(listViewAlarms);
            tabAlarms.Controls.Add(buttonEditAlarm);
            tabAlarms.Controls.Add(buttonDeleteAlarm);
            tabAlarms.Controls.Add(buttonAddAlarm);
            tabAlarms.Controls.Add(label4);
            tabAlarms.ImageIndex = 0;
            tabAlarms.Location = new Point(4, 30);
            tabAlarms.Margin = new Padding(4, 3, 4, 3);
            tabAlarms.Name = "tabAlarms";
            tabAlarms.Padding = new Padding(4, 3, 4, 3);
            tabAlarms.Size = new Size(794, 567);
            tabAlarms.TabIndex = 1;
            tabAlarms.Text = "Alarms  ";
            tabAlarms.UseVisualStyleBackColor = true;
            // 
            // listViewAlarms
            // 
            listViewAlarms.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewAlarms.Location = new Point(6, 34);
            listViewAlarms.MultiSelect = false;
            listViewAlarms.Name = "listViewAlarms";
            listViewAlarms.Size = new Size(781, 530);
            listViewAlarms.TabIndex = 11;
            listViewAlarms.UseCompatibleStateImageBehavior = false;
            listViewAlarms.DoubleClick += listViewAlarms_DoubleClick;
            listViewAlarms.Resize += listView_Resize;
            // 
            // buttonEditAlarm
            // 
            buttonEditAlarm.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonEditAlarm.Location = new Point(760, 10);
            buttonEditAlarm.Margin = new Padding(4, 3, 4, 3);
            buttonEditAlarm.Name = "buttonEditAlarm";
            buttonEditAlarm.Size = new Size(27, 23);
            buttonEditAlarm.TabIndex = 11;
            buttonEditAlarm.Text = "...";
            buttonEditAlarm.UseVisualStyleBackColor = true;
            buttonEditAlarm.Click += buttonEditAlarm_Click;
            // 
            // buttonDeleteAlarm
            // 
            buttonDeleteAlarm.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonDeleteAlarm.Location = new Point(725, 10);
            buttonDeleteAlarm.Margin = new Padding(4, 3, 4, 3);
            buttonDeleteAlarm.Name = "buttonDeleteAlarm";
            buttonDeleteAlarm.Size = new Size(27, 23);
            buttonDeleteAlarm.TabIndex = 10;
            buttonDeleteAlarm.Text = "-";
            buttonDeleteAlarm.UseVisualStyleBackColor = true;
            buttonDeleteAlarm.Click += buttonDeleteAlarm_Click;
            // 
            // buttonAddAlarm
            // 
            buttonAddAlarm.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonAddAlarm.Location = new Point(690, 10);
            buttonAddAlarm.Margin = new Padding(4, 3, 4, 3);
            buttonAddAlarm.Name = "buttonAddAlarm";
            buttonAddAlarm.Size = new Size(27, 23);
            buttonAddAlarm.TabIndex = 9;
            buttonAddAlarm.Text = "+";
            buttonAddAlarm.UseVisualStyleBackColor = true;
            buttonAddAlarm.Click += buttonAddAlarm_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 16);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(47, 15);
            label4.TabIndex = 0;
            label4.Text = "Alarms:";
            // 
            // tabCode
            // 
            tabCode.Controls.Add(textScriptCode);
            tabCode.Controls.Add(label9);
            tabCode.ImageIndex = 8;
            tabCode.Location = new Point(4, 30);
            tabCode.Margin = new Padding(4, 3, 4, 3);
            tabCode.Name = "tabCode";
            tabCode.Size = new Size(794, 567);
            tabCode.TabIndex = 6;
            tabCode.Text = "Code   ";
            tabCode.UseVisualStyleBackColor = true;
            // 
            // textScriptCode
            // 
            textScriptCode.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textScriptCode.Font = new Font("Courier New", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            textScriptCode.Location = new Point(7, 34);
            textScriptCode.Margin = new Padding(4, 3, 4, 3);
            textScriptCode.Multiline = true;
            textScriptCode.Name = "textScriptCode";
            textScriptCode.ScrollBars = ScrollBars.Both;
            textScriptCode.Size = new Size(780, 530);
            textScriptCode.TabIndex = 6;
            textScriptCode.WordWrap = false;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(6, 16);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(69, 15);
            label9.TabIndex = 4;
            label9.Text = "Script code:";
            // 
            // tabTraceNotes
            // 
            tabTraceNotes.Controls.Add(listViewTraceNoteTo);
            tabTraceNotes.Controls.Add(listViewTraceNoteFrom);
            tabTraceNotes.Controls.Add(textTraceNodeType);
            tabTraceNotes.Controls.Add(buttonTraceNodeTypes);
            tabTraceNotes.Controls.Add(label5);
            tabTraceNotes.Controls.Add(buttonTraceToEdit);
            tabTraceNotes.Controls.Add(buttonTraceToRemove);
            tabTraceNotes.Controls.Add(buttonTraceToAdd);
            tabTraceNotes.Controls.Add(buttonTraceFromEdit);
            tabTraceNotes.Controls.Add(buttonTraceFromRemove);
            tabTraceNotes.Controls.Add(buttonTraceFromAdd);
            tabTraceNotes.Controls.Add(label13);
            tabTraceNotes.Controls.Add(label14);
            tabTraceNotes.ImageIndex = 2;
            tabTraceNotes.Location = new Point(4, 30);
            tabTraceNotes.Margin = new Padding(4, 3, 4, 3);
            tabTraceNotes.Name = "tabTraceNotes";
            tabTraceNotes.Size = new Size(794, 567);
            tabTraceNotes.TabIndex = 5;
            tabTraceNotes.Text = "Trace notes  ";
            tabTraceNotes.UseVisualStyleBackColor = true;
            // 
            // listViewTraceNoteTo
            // 
            listViewTraceNoteTo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewTraceNoteTo.Location = new Point(395, 89);
            listViewTraceNoteTo.Name = "listViewTraceNoteTo";
            listViewTraceNoteTo.Size = new Size(393, 475);
            listViewTraceNoteTo.TabIndex = 44;
            listViewTraceNoteTo.UseCompatibleStateImageBehavior = false;
            // 
            // listViewTraceNoteFrom
            // 
            listViewTraceNoteFrom.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            listViewTraceNoteFrom.Location = new Point(11, 89);
            listViewTraceNoteFrom.MultiSelect = false;
            listViewTraceNoteFrom.Name = "listViewTraceNoteFrom";
            listViewTraceNoteFrom.Size = new Size(378, 475);
            listViewTraceNoteFrom.TabIndex = 43;
            listViewTraceNoteFrom.UseCompatibleStateImageBehavior = false;
            // 
            // textTraceNodeType
            // 
            textTraceNodeType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textTraceNodeType.Enabled = false;
            textTraceNodeType.Location = new Point(135, 22);
            textTraceNodeType.Margin = new Padding(4, 3, 4, 3);
            textTraceNodeType.MaxLength = 255;
            textTraceNodeType.Name = "textTraceNodeType";
            textTraceNodeType.Size = new Size(617, 23);
            textTraceNodeType.TabIndex = 41;
            // 
            // buttonTraceNodeTypes
            // 
            buttonTraceNodeTypes.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonTraceNodeTypes.BackColor = SystemColors.Control;
            buttonTraceNodeTypes.FlatStyle = FlatStyle.System;
            buttonTraceNodeTypes.Location = new Point(760, 22);
            buttonTraceNodeTypes.Margin = new Padding(4, 3, 4, 3);
            buttonTraceNodeTypes.Name = "buttonTraceNodeTypes";
            buttonTraceNodeTypes.Size = new Size(24, 23);
            buttonTraceNodeTypes.TabIndex = 42;
            buttonTraceNodeTypes.Text = "...";
            buttonTraceNodeTypes.UseVisualStyleBackColor = false;
            // 
            // label5
            // 
            label5.Location = new Point(9, 22);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(131, 18);
            label5.TabIndex = 40;
            label5.Text = "Trace node types:";
            label5.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonTraceToEdit
            // 
            buttonTraceToEdit.Location = new Point(566, 60);
            buttonTraceToEdit.Margin = new Padding(4, 3, 4, 3);
            buttonTraceToEdit.Name = "buttonTraceToEdit";
            buttonTraceToEdit.Size = new Size(27, 23);
            buttonTraceToEdit.TabIndex = 15;
            buttonTraceToEdit.Text = "...";
            buttonTraceToEdit.UseVisualStyleBackColor = true;
            // 
            // buttonTraceToRemove
            // 
            buttonTraceToRemove.Location = new Point(531, 60);
            buttonTraceToRemove.Margin = new Padding(4, 3, 4, 3);
            buttonTraceToRemove.Name = "buttonTraceToRemove";
            buttonTraceToRemove.Size = new Size(27, 23);
            buttonTraceToRemove.TabIndex = 14;
            buttonTraceToRemove.Text = "-";
            buttonTraceToRemove.UseVisualStyleBackColor = true;
            // 
            // buttonTraceToAdd
            // 
            buttonTraceToAdd.Location = new Point(498, 60);
            buttonTraceToAdd.Margin = new Padding(4, 3, 4, 3);
            buttonTraceToAdd.Name = "buttonTraceToAdd";
            buttonTraceToAdd.Size = new Size(27, 23);
            buttonTraceToAdd.TabIndex = 13;
            buttonTraceToAdd.Text = "+";
            buttonTraceToAdd.UseVisualStyleBackColor = true;
            // 
            // buttonTraceFromEdit
            // 
            buttonTraceFromEdit.Location = new Point(196, 60);
            buttonTraceFromEdit.Margin = new Padding(4, 3, 4, 3);
            buttonTraceFromEdit.Name = "buttonTraceFromEdit";
            buttonTraceFromEdit.Size = new Size(27, 23);
            buttonTraceFromEdit.TabIndex = 12;
            buttonTraceFromEdit.Text = "...";
            buttonTraceFromEdit.UseVisualStyleBackColor = true;
            // 
            // buttonTraceFromRemove
            // 
            buttonTraceFromRemove.Location = new Point(162, 60);
            buttonTraceFromRemove.Margin = new Padding(4, 3, 4, 3);
            buttonTraceFromRemove.Name = "buttonTraceFromRemove";
            buttonTraceFromRemove.Size = new Size(27, 23);
            buttonTraceFromRemove.TabIndex = 11;
            buttonTraceFromRemove.Text = "-";
            buttonTraceFromRemove.UseVisualStyleBackColor = true;
            // 
            // buttonTraceFromAdd
            // 
            buttonTraceFromAdd.Location = new Point(128, 60);
            buttonTraceFromAdd.Margin = new Padding(4, 3, 4, 3);
            buttonTraceFromAdd.Name = "buttonTraceFromAdd";
            buttonTraceFromAdd.Size = new Size(27, 23);
            buttonTraceFromAdd.TabIndex = 10;
            buttonTraceFromAdd.Text = "+";
            buttonTraceFromAdd.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(397, 64);
            label13.Margin = new Padding(4, 0, 4, 0);
            label13.Name = "label13";
            label13.Size = new Size(81, 15);
            label13.TabIndex = 5;
            label13.Text = "Trace node to:";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(13, 64);
            label14.Margin = new Padding(4, 0, 4, 0);
            label14.Name = "label14";
            label14.Size = new Size(99, 15);
            label14.TabIndex = 4;
            label14.Text = "Trace node from :";
            // 
            // NoteEditorForm
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            ClientSize = new Size(808, 637);
            Controls.Add(panelForm);
            Controls.Add(toolBarNoteEditor);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new Padding(4, 3, 4, 3);
            Name = "NoteEditorForm";
            Text = "Note Editor";
            FormClosing += NoteEditorForm_FormClosing;
            Load += NoteEditorForm_Load;
            KeyPress += NoteEditorForm_KeyPress;
            KeyUp += NoteEditorForm_KeyUp;
            toolBarNoteEditor.ResumeLayout(false);
            toolBarNoteEditor.PerformLayout();
            panelForm.ResumeLayout(false);
            tabNoteData.ResumeLayout(false);
            tabBasicData.ResumeLayout(false);
            tabBasicData.PerformLayout();
            panelDescription.ResumeLayout(false);
            panelDescription.PerformLayout();
            toolDescription.ResumeLayout(false);
            toolDescription.PerformLayout();
            tabAttributes.ResumeLayout(false);
            tabAttributes.PerformLayout();
            tabResources.ResumeLayout(false);
            tabResources.PerformLayout();
            panelPreview.ResumeLayout(false);
            panelPreview.PerformLayout();
            tabTasks.ResumeLayout(false);
            tabTasks.PerformLayout();
            tabAlarms.ResumeLayout(false);
            tabAlarms.PerformLayout();
            tabCode.ResumeLayout(false);
            tabCode.PerformLayout();
            tabTraceNotes.ResumeLayout(false);
            tabTraceNotes.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolBarNoteEditor;
        private System.Windows.Forms.ToolStripButton buttonSave;
        private System.Windows.Forms.ToolStripButton buttonDelete;
        private System.Windows.Forms.ToolStripButton buttonUndo;
        private System.Windows.Forms.ToolStripSeparator toolStripS1;
        private System.Windows.Forms.ToolStripButton buttonPostIt;
        private System.Windows.Forms.ToolStripSeparator toolStripS2;
        private System.Windows.Forms.ToolStripButton buttonCheck;
        private System.Windows.Forms.ToolStripSeparator toolStripS3;
        private System.Windows.Forms.ToolStripButton buttonPrint;
        private System.Windows.Forms.ToolStripSeparator toolStripS4;
        private System.Windows.Forms.ToolStripDropDownButton buttonTools;
        private System.Windows.Forms.ToolStripMenuItem buttonInsertTemplate;
        private System.Windows.Forms.ToolStripSeparator toolStripToolS1;
        private System.Windows.Forms.ToolStripMenuItem buttonExecuteKntScript;
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
        private System.Windows.Forms.Button buttonTraceToEdit;
        private System.Windows.Forms.Button buttonTraceToRemove;
        private System.Windows.Forms.Button buttonTraceToAdd;
        private System.Windows.Forms.Button buttonTraceFromEdit;
        private System.Windows.Forms.Button buttonTraceFromRemove;
        private System.Windows.Forms.Button buttonTraceFromAdd;
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
        private System.Windows.Forms.Button buttonTraceNodeTypes;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textFolderNumber;
        private System.Windows.Forms.TextBox textNoteNumber;
        private System.Windows.Forms.ListView listViewAttributes;
        private System.Windows.Forms.ListView listViewResources;
        private System.Windows.Forms.ListView listViewTasks;
        private System.Windows.Forms.ListView listViewAlarms;
        private System.Windows.Forms.ListView listViewTraceNoteTo;
        private System.Windows.Forms.ListView listViewTraceNoteFrom;
        private System.Windows.Forms.Label labelLoadingHtml;
        private System.Windows.Forms.Panel panelDescription;
        private Pavonis.Html.Editor.HtmlEditorControl htmlDescription;
        private System.Windows.Forms.TextBox textDescription;
        private System.Windows.Forms.ToolStrip toolDescription;
        private System.Windows.Forms.ToolStripDropDownButton toolDescriptionHtml;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionHtmlTitle1;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionHtmlTitle2;
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
        private System.Windows.Forms.Panel panelPreview;
        private System.Windows.Forms.LinkLabel linkViewFile;
        private System.Windows.Forms.Button buttonInsertLink;
        private System.Windows.Forms.ToolTip toolTipHelps;
        private System.Windows.Forms.ToolStripSeparator toolDescriptionHtmlS3;
        private System.Windows.Forms.ToolStripMenuItem toolDescriptionHtmlEdit;
        private System.Windows.Forms.ToolStripSeparator toolDescriptionS3;
        private System.Windows.Forms.ToolStripButton toolDescriptionUploadResource;
        private System.Windows.Forms.Button buttonDeleteType;
        private System.Windows.Forms.ToolStripButton toolDescriptionUploadResourceFromClipboard;
        private System.Windows.Forms.TextBox textStatus;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox textTaskDescription;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Button buttonSaveResource;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private Button buttonNavigate;
        private KntWebView.KWebView webView2;
        private KntWebView.KWebView webViewResource;
        private TextBox textDescriptionResource;
        private TextBox textTaskTags;
        private Label labelPreview;
    }
}