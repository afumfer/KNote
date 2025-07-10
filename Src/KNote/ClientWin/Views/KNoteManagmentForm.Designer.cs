namespace KNote.ClientWin.Views
{
    partial class KNoteManagmentForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KNoteManagmentForm));
            menuMangment = new MenuStrip();
            menuFile = new ToolStripMenuItem();
            menuRepositories = new ToolStripMenuItem();
            menuCreateRepository = new ToolStripMenuItem();
            menuManagmentRepository = new ToolStripMenuItem();
            menuAddRepositoryLink = new ToolStripMenuItem();
            menuRemoveRepositoryLink = new ToolStripMenuItem();
            toolMenuIRepositoryS1 = new ToolStripSeparator();
            menuRefreshTreeFolders = new ToolStripMenuItem();
            toolMenuIRepositoryS2 = new ToolStripSeparator();
            menuImportData = new ToolStripMenuItem();
            menuFolders = new ToolStripMenuItem();
            menuNewFolder = new ToolStripMenuItem();
            menuEditFolder = new ToolStripMenuItem();
            menuDeleteFolder = new ToolStripMenuItem();
            menuFileS1 = new ToolStripSeparator();
            menuHide = new ToolStripMenuItem();
            menuFilesS2 = new ToolStripSeparator();
            menuExit = new ToolStripMenuItem();
            menuEdit = new ToolStripMenuItem();
            menuNewNote = new ToolStripMenuItem();
            menuNewNoteAsPostIt = new ToolStripMenuItem();
            menuEditNote = new ToolStripMenuItem();
            menuEditNoteAsPostIt = new ToolStripMenuItem();
            menuDeleteNote = new ToolStripMenuItem();
            menuEditS1 = new ToolStripSeparator();
            menuMoveSelectedNotes = new ToolStripMenuItem();
            menuEditS2 = new ToolStripSeparator();
            menuAddTags = new ToolStripMenuItem();
            menuRemoveTags = new ToolStripMenuItem();
            menuEditS3 = new ToolStripSeparator();
            menuMoreOptions = new ToolStripMenuItem();
            menuExecuteKntScript = new ToolStripMenuItem();
            menuView = new ToolStripMenuItem();
            menuFoldersExplorer = new ToolStripMenuItem();
            menuSearchPanel = new ToolStripMenuItem();
            menuViewS1 = new ToolStripSeparator();
            menuVerticalPanelForNotes = new ToolStripMenuItem();
            menuHeaderPanelVisible = new ToolStripMenuItem();
            menuToolbarVisible = new ToolStripMenuItem();
            menuMainVisible = new ToolStripMenuItem();
            menuTools = new ToolStripMenuItem();
            menuReports = new ToolStripMenuItem();
            menuToolsS1 = new ToolStripSeparator();
            menuKntScriptConsole = new ToolStripMenuItem();
            menuChat = new ToolStripMenuItem();
            menuChatGPT = new ToolStripMenuItem();
            menuCOMPortServer = new ToolStripMenuItem();
            menuKNoteLab = new ToolStripMenuItem();
            menuToolsS2 = new ToolStripSeparator();
            menuOptions = new ToolStripMenuItem();
            menuHelp = new ToolStripMenuItem();
            menuHelpDoc = new ToolStripMenuItem();
            menuAbout = new ToolStripMenuItem();
            statusBarManagment = new StatusStrip();
            statusLabel1 = new ToolStripStatusLabel();
            statusS1 = new ToolStripStatusLabel();
            statusLabel2 = new ToolStripStatusLabel();
            progressBar = new ToolStripProgressBar();
            toolBarManagment = new ToolStrip();
            toolNewNote = new ToolStripButton();
            toolEditNote = new ToolStripButton();
            toolDeleteNote = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            toolPrintReports = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            toolConfiguration = new ToolStripButton();
            panelSupManagment = new Panel();
            labelReResources = new Label();
            labelRepAliasCon = new Label();
            labelFolderDetail = new Label();
            labelFolder = new Label();
            pictureBoxFolder = new PictureBox();
            splitContainer1 = new SplitContainer();
            tabExplorers = new TabControl();
            tabTreeFolders = new TabPage();
            tabSearch = new TabPage();
            imageTabExplorer = new ImageList(components);
            splitContainer2 = new SplitContainer();
            menuMangment.SuspendLayout();
            statusBarManagment.SuspendLayout();
            toolBarManagment.SuspendLayout();
            panelSupManagment.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxFolder).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tabExplorers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.SuspendLayout();
            SuspendLayout();
            // 
            // menuMangment
            // 
            menuMangment.Items.AddRange(new ToolStripItem[] { menuFile, menuEdit, menuView, menuTools, menuHelp });
            menuMangment.Location = new Point(0, 0);
            menuMangment.Name = "menuMangment";
            menuMangment.Padding = new Padding(7, 2, 0, 2);
            menuMangment.Size = new Size(1014, 24);
            menuMangment.TabIndex = 2;
            menuMangment.Text = "KNote menu managment";
            // 
            // menuFile
            // 
            menuFile.DropDownItems.AddRange(new ToolStripItem[] { menuRepositories, menuFolders, menuFileS1, menuHide, menuFilesS2, menuExit });
            menuFile.Name = "menuFile";
            menuFile.Size = new Size(37, 20);
            menuFile.Text = "&File";
            // 
            // menuRepositories
            // 
            menuRepositories.DropDownItems.AddRange(new ToolStripItem[] { menuCreateRepository, menuManagmentRepository, menuAddRepositoryLink, menuRemoveRepositoryLink, toolMenuIRepositoryS1, menuRefreshTreeFolders, toolMenuIRepositoryS2, menuImportData });
            menuRepositories.Name = "menuRepositories";
            menuRepositories.Size = new Size(203, 22);
            menuRepositories.Text = "&Repositories";
            // 
            // menuCreateRepository
            // 
            menuCreateRepository.Name = "menuCreateRepository";
            menuCreateRepository.Size = new Size(282, 22);
            menuCreateRepository.Text = "Create new repository ,,,";
            menuCreateRepository.Click += menu_Click;
            // 
            // menuManagmentRepository
            // 
            menuManagmentRepository.Name = "menuManagmentRepository";
            menuManagmentRepository.Size = new Size(282, 22);
            menuManagmentRepository.Text = "Edit repository properties ...";
            menuManagmentRepository.Click += menu_Click;
            // 
            // menuAddRepositoryLink
            // 
            menuAddRepositoryLink.Name = "menuAddRepositoryLink";
            menuAddRepositoryLink.Size = new Size(282, 22);
            menuAddRepositoryLink.Text = "Add link to an existing repository ...";
            menuAddRepositoryLink.Click += menu_Click;
            // 
            // menuRemoveRepositoryLink
            // 
            menuRemoveRepositoryLink.Name = "menuRemoveRepositoryLink";
            menuRemoveRepositoryLink.Size = new Size(282, 22);
            menuRemoveRepositoryLink.Text = "Remove link from selected repository ...";
            menuRemoveRepositoryLink.Click += menu_Click;
            // 
            // toolMenuIRepositoryS1
            // 
            toolMenuIRepositoryS1.Name = "toolMenuIRepositoryS1";
            toolMenuIRepositoryS1.Size = new Size(279, 6);
            // 
            // menuRefreshTreeFolders
            // 
            menuRefreshTreeFolders.Name = "menuRefreshTreeFolders";
            menuRefreshTreeFolders.Size = new Size(282, 22);
            menuRefreshTreeFolders.Text = "Refresh repositories and folders tree";
            menuRefreshTreeFolders.Click += menu_Click;
            // 
            // toolMenuIRepositoryS2
            // 
            toolMenuIRepositoryS2.Name = "toolMenuIRepositoryS2";
            toolMenuIRepositoryS2.Size = new Size(279, 6);
            toolMenuIRepositoryS2.Visible = false;
            // 
            // menuImportData
            // 
            menuImportData.Name = "menuImportData";
            menuImportData.Size = new Size(282, 22);
            menuImportData.Text = "Import data ...";
            menuImportData.Visible = false;
            menuImportData.Click += menu_Click;
            // 
            // menuFolders
            // 
            menuFolders.DropDownItems.AddRange(new ToolStripItem[] { menuNewFolder, menuEditFolder, menuDeleteFolder });
            menuFolders.Name = "menuFolders";
            menuFolders.Size = new Size(203, 22);
            menuFolders.Text = "&Folders";
            // 
            // menuNewFolder
            // 
            menuNewFolder.Name = "menuNewFolder";
            menuNewFolder.Size = new Size(179, 22);
            menuNewFolder.Text = "Create new folder ...";
            menuNewFolder.Click += menu_Click;
            // 
            // menuEditFolder
            // 
            menuEditFolder.Name = "menuEditFolder";
            menuEditFolder.Size = new Size(179, 22);
            menuEditFolder.Text = "&Edit folder ...";
            menuEditFolder.Click += menu_Click;
            // 
            // menuDeleteFolder
            // 
            menuDeleteFolder.Name = "menuDeleteFolder";
            menuDeleteFolder.Size = new Size(179, 22);
            menuDeleteFolder.Text = "Delete folder ...";
            menuDeleteFolder.Click += menu_Click;
            // 
            // menuFileS1
            // 
            menuFileS1.Name = "menuFileS1";
            menuFileS1.Size = new Size(200, 6);
            // 
            // menuHide
            // 
            menuHide.Name = "menuHide";
            menuHide.Size = new Size(203, 22);
            menuHide.Text = "&Hide KNote managment";
            menuHide.Click += menu_Click;
            // 
            // menuFilesS2
            // 
            menuFilesS2.Name = "menuFilesS2";
            menuFilesS2.Size = new Size(200, 6);
            // 
            // menuExit
            // 
            menuExit.Name = "menuExit";
            menuExit.Size = new Size(203, 22);
            menuExit.Text = "&Exit";
            menuExit.Click += menu_Click;
            // 
            // menuEdit
            // 
            menuEdit.DropDownItems.AddRange(new ToolStripItem[] { menuNewNote, menuNewNoteAsPostIt, menuEditNote, menuEditNoteAsPostIt, menuDeleteNote, menuEditS1, menuMoveSelectedNotes, menuEditS2, menuAddTags, menuRemoveTags, menuEditS3, menuMoreOptions });
            menuEdit.Name = "menuEdit";
            menuEdit.Size = new Size(39, 20);
            menuEdit.Text = "&Edit";
            // 
            // menuNewNote
            // 
            menuNewNote.Name = "menuNewNote";
            menuNewNote.ShortcutKeys = Keys.F2;
            menuNewNote.Size = new Size(261, 22);
            menuNewNote.Text = "&New note";
            menuNewNote.Click += menu_Click;
            // 
            // menuNewNoteAsPostIt
            // 
            menuNewNoteAsPostIt.Name = "menuNewNoteAsPostIt";
            menuNewNoteAsPostIt.Size = new Size(261, 22);
            menuNewNoteAsPostIt.Text = "New note as &PostIt";
            menuNewNoteAsPostIt.Click += menu_Click;
            // 
            // menuEditNote
            // 
            menuEditNote.Name = "menuEditNote";
            menuEditNote.ShortcutKeys = Keys.F3;
            menuEditNote.Size = new Size(261, 22);
            menuEditNote.Text = "&Edit note";
            menuEditNote.Click += menu_Click;
            // 
            // menuEditNoteAsPostIt
            // 
            menuEditNoteAsPostIt.Name = "menuEditNoteAsPostIt";
            menuEditNoteAsPostIt.ShortcutKeys = Keys.F4;
            menuEditNoteAsPostIt.Size = new Size(261, 22);
            menuEditNoteAsPostIt.Text = "Edit note as PostI&t";
            menuEditNoteAsPostIt.Click += menu_Click;
            // 
            // menuDeleteNote
            // 
            menuDeleteNote.Name = "menuDeleteNote";
            menuDeleteNote.Size = new Size(261, 22);
            menuDeleteNote.Text = "&Delete note";
            menuDeleteNote.Click += menu_Click;
            // 
            // menuEditS1
            // 
            menuEditS1.Name = "menuEditS1";
            menuEditS1.Size = new Size(258, 6);
            // 
            // menuMoveSelectedNotes
            // 
            menuMoveSelectedNotes.Name = "menuMoveSelectedNotes";
            menuMoveSelectedNotes.Size = new Size(261, 22);
            menuMoveSelectedNotes.Text = "&Move selected notes ...";
            menuMoveSelectedNotes.Click += menu_Click;
            // 
            // menuEditS2
            // 
            menuEditS2.Name = "menuEditS2";
            menuEditS2.Size = new Size(258, 6);
            // 
            // menuAddTags
            // 
            menuAddTags.Name = "menuAddTags";
            menuAddTags.Size = new Size(261, 22);
            menuAddTags.Text = "Add tags to selected notes ...";
            menuAddTags.Click += menu_Click;
            // 
            // menuRemoveTags
            // 
            menuRemoveTags.Name = "menuRemoveTags";
            menuRemoveTags.Size = new Size(261, 22);
            menuRemoveTags.Text = "Remove tags from selected notes ...";
            menuRemoveTags.Click += menu_Click;
            // 
            // menuEditS3
            // 
            menuEditS3.Name = "menuEditS3";
            menuEditS3.Size = new Size(258, 6);
            // 
            // menuMoreOptions
            // 
            menuMoreOptions.DropDownItems.AddRange(new ToolStripItem[] { menuExecuteKntScript });
            menuMoreOptions.Name = "menuMoreOptions";
            menuMoreOptions.Size = new Size(261, 22);
            menuMoreOptions.Text = "More note options";
            // 
            // menuExecuteKntScript
            // 
            menuExecuteKntScript.Name = "menuExecuteKntScript";
            menuExecuteKntScript.ShortcutKeys = Keys.F5;
            menuExecuteKntScript.Size = new Size(313, 22);
            menuExecuteKntScript.Text = "Execute KntScript code for selected  notes";
            menuExecuteKntScript.Click += menu_Click;
            // 
            // menuView
            // 
            menuView.DropDownItems.AddRange(new ToolStripItem[] { menuFoldersExplorer, menuSearchPanel, menuViewS1, menuVerticalPanelForNotes, menuHeaderPanelVisible, menuToolbarVisible, menuMainVisible });
            menuView.Name = "menuView";
            menuView.Size = new Size(44, 20);
            menuView.Text = "&View";
            // 
            // menuFoldersExplorer
            // 
            menuFoldersExplorer.Checked = true;
            menuFoldersExplorer.CheckState = CheckState.Checked;
            menuFoldersExplorer.Name = "menuFoldersExplorer";
            menuFoldersExplorer.ShortcutKeys = Keys.F11;
            menuFoldersExplorer.Size = new Size(237, 22);
            menuFoldersExplorer.Text = "&Folders explorer";
            menuFoldersExplorer.Click += menu_Click;
            // 
            // menuSearchPanel
            // 
            menuSearchPanel.Name = "menuSearchPanel";
            menuSearchPanel.ShortcutKeys = Keys.Shift | Keys.F11;
            menuSearchPanel.Size = new Size(237, 22);
            menuSearchPanel.Text = "Search panel";
            menuSearchPanel.Click += menu_Click;
            // 
            // menuViewS1
            // 
            menuViewS1.Name = "menuViewS1";
            menuViewS1.Size = new Size(234, 6);
            // 
            // menuVerticalPanelForNotes
            // 
            menuVerticalPanelForNotes.Name = "menuVerticalPanelForNotes";
            menuVerticalPanelForNotes.ShortcutKeys = Keys.F12;
            menuVerticalPanelForNotes.Size = new Size(237, 22);
            menuVerticalPanelForNotes.Text = "Vertical panel for list notes";
            menuVerticalPanelForNotes.Click += menu_Click;
            // 
            // menuHeaderPanelVisible
            // 
            menuHeaderPanelVisible.Checked = true;
            menuHeaderPanelVisible.CheckOnClick = true;
            menuHeaderPanelVisible.CheckState = CheckState.Checked;
            menuHeaderPanelVisible.Name = "menuHeaderPanelVisible";
            menuHeaderPanelVisible.ShortcutKeys = Keys.Alt | Keys.F12;
            menuHeaderPanelVisible.Size = new Size(237, 22);
            menuHeaderPanelVisible.Text = "Show header panel";
            menuHeaderPanelVisible.Click += menu_Click;
            // 
            // menuToolbarVisible
            // 
            menuToolbarVisible.Checked = true;
            menuToolbarVisible.CheckState = CheckState.Checked;
            menuToolbarVisible.Name = "menuToolbarVisible";
            menuToolbarVisible.ShortcutKeys = Keys.Control | Keys.F12;
            menuToolbarVisible.Size = new Size(237, 22);
            menuToolbarVisible.Text = "Show toolbar";
            menuToolbarVisible.Click += menu_Click;
            // 
            // menuMainVisible
            // 
            menuMainVisible.Checked = true;
            menuMainVisible.CheckOnClick = true;
            menuMainVisible.CheckState = CheckState.Checked;
            menuMainVisible.Name = "menuMainVisible";
            menuMainVisible.ShortcutKeys = Keys.Shift | Keys.F12;
            menuMainVisible.Size = new Size(237, 22);
            menuMainVisible.Text = "Show main menu";
            menuMainVisible.Click += menu_Click;
            // 
            // menuTools
            // 
            menuTools.DropDownItems.AddRange(new ToolStripItem[] { menuReports, menuToolsS1, menuKntScriptConsole, menuChat, menuChatGPT, menuCOMPortServer, menuKNoteLab, menuToolsS2, menuOptions });
            menuTools.Name = "menuTools";
            menuTools.Size = new Size(46, 20);
            menuTools.Text = "&Tools";
            // 
            // menuReports
            // 
            menuReports.Name = "menuReports";
            menuReports.Size = new Size(185, 22);
            menuReports.Text = "&Reports ...";
            menuReports.Click += menu_Click;
            // 
            // menuToolsS1
            // 
            menuToolsS1.Name = "menuToolsS1";
            menuToolsS1.Size = new Size(182, 6);
            // 
            // menuKntScriptConsole
            // 
            menuKntScriptConsole.Name = "menuKntScriptConsole";
            menuKntScriptConsole.ShortcutKeys = Keys.F6;
            menuKntScriptConsole.Size = new Size(185, 22);
            menuKntScriptConsole.Text = "Knt&Script console";
            menuKntScriptConsole.Click += menu_Click;
            // 
            // menuChat
            // 
            menuChat.Name = "menuChat";
            menuChat.Size = new Size(185, 22);
            menuChat.Text = "Chat ...";
            menuChat.Click += menu_Click;
            // 
            // menuChatGPT
            // 
            menuChatGPT.Name = "menuChatGPT";
            menuChatGPT.ShortcutKeys = Keys.F7;
            menuChatGPT.Size = new Size(185, 22);
            menuChatGPT.Text = "ChatGPT ...";
            menuChatGPT.Click += menu_Click;
            // 
            // menuCOMPortServer
            // 
            menuCOMPortServer.Name = "menuCOMPortServer";
            menuCOMPortServer.Size = new Size(185, 22);
            menuCOMPortServer.Text = "COM port server ...";
            menuCOMPortServer.Click += menu_Click;
            // 
            // menuKNoteLab
            // 
            menuKNoteLab.Name = "menuKNoteLab";
            menuKNoteLab.Size = new Size(185, 22);
            menuKNoteLab.Text = "KNote &lab ...";
            menuKNoteLab.Visible = false;
            menuKNoteLab.Click += menu_Click;
            // 
            // menuToolsS2
            // 
            menuToolsS2.Name = "menuToolsS2";
            menuToolsS2.Size = new Size(182, 6);
            // 
            // menuOptions
            // 
            menuOptions.Name = "menuOptions";
            menuOptions.Size = new Size(185, 22);
            menuOptions.Text = "&Options ...";
            menuOptions.Click += menu_Click;
            // 
            // menuHelp
            // 
            menuHelp.DropDownItems.AddRange(new ToolStripItem[] { menuHelpDoc, menuAbout });
            menuHelp.Name = "menuHelp";
            menuHelp.Size = new Size(24, 20);
            menuHelp.Text = "&?";
            // 
            // menuHelpDoc
            // 
            menuHelpDoc.Name = "menuHelpDoc";
            menuHelpDoc.Size = new Size(119, 22);
            menuHelpDoc.Text = "&Help ...";
            menuHelpDoc.Click += menu_Click;
            // 
            // menuAbout
            // 
            menuAbout.Name = "menuAbout";
            menuAbout.Size = new Size(119, 22);
            menuAbout.Text = "&About ...";
            menuAbout.Click += menu_Click;
            // 
            // statusBarManagment
            // 
            statusBarManagment.Items.AddRange(new ToolStripItem[] { statusLabel1, statusS1, statusLabel2, progressBar });
            statusBarManagment.Location = new Point(0, 668);
            statusBarManagment.Name = "statusBarManagment";
            statusBarManagment.Padding = new Padding(1, 0, 16, 0);
            statusBarManagment.Size = new Size(1014, 22);
            statusBarManagment.TabIndex = 3;
            statusBarManagment.Text = "statusStrip1";
            // 
            // statusLabel1
            // 
            statusLabel1.AutoSize = false;
            statusLabel1.BorderStyle = Border3DStyle.RaisedOuter;
            statusLabel1.Name = "statusLabel1";
            statusLabel1.Size = new Size(200, 17);
            statusLabel1.Text = "....";
            statusLabel1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // statusS1
            // 
            statusS1.Name = "statusS1";
            statusS1.Size = new Size(10, 17);
            statusS1.Text = "|";
            // 
            // statusLabel2
            // 
            statusLabel2.Name = "statusLabel2";
            statusLabel2.Size = new Size(19, 17);
            statusLabel2.Text = "....";
            statusLabel2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(100, 16);
            progressBar.Visible = false;
            // 
            // toolBarManagment
            // 
            toolBarManagment.Items.AddRange(new ToolStripItem[] { toolNewNote, toolEditNote, toolDeleteNote, toolStripSeparator1, toolPrintReports, toolStripSeparator2, toolConfiguration });
            toolBarManagment.Location = new Point(0, 24);
            toolBarManagment.Name = "toolBarManagment";
            toolBarManagment.Size = new Size(1014, 25);
            toolBarManagment.TabIndex = 4;
            toolBarManagment.Text = "toolStrip1";
            // 
            // toolNewNote
            // 
            toolNewNote.Image = (Image)resources.GetObject("toolNewNote.Image");
            toolNewNote.ImageAlign = ContentAlignment.MiddleLeft;
            toolNewNote.ImageTransparentColor = Color.Magenta;
            toolNewNote.Name = "toolNewNote";
            toolNewNote.Size = new Size(57, 22);
            toolNewNote.Text = "New  ";
            toolNewNote.ToolTipText = "New note";
            toolNewNote.Click += buttonToolBar_Click;
            // 
            // toolEditNote
            // 
            toolEditNote.Image = (Image)resources.GetObject("toolEditNote.Image");
            toolEditNote.ImageTransparentColor = Color.Magenta;
            toolEditNote.Name = "toolEditNote";
            toolEditNote.Size = new Size(53, 22);
            toolEditNote.Text = "Edit  ";
            toolEditNote.ToolTipText = "Edit note";
            toolEditNote.Click += buttonToolBar_Click;
            // 
            // toolDeleteNote
            // 
            toolDeleteNote.Image = (Image)resources.GetObject("toolDeleteNote.Image");
            toolDeleteNote.ImageTransparentColor = Color.Magenta;
            toolDeleteNote.Name = "toolDeleteNote";
            toolDeleteNote.Size = new Size(66, 22);
            toolDeleteNote.Text = "Delete  ";
            toolDeleteNote.ToolTipText = "Delete note";
            toolDeleteNote.Click += buttonToolBar_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // toolPrintReports
            // 
            toolPrintReports.Image = (Image)resources.GetObject("toolPrintReports.Image");
            toolPrintReports.ImageTransparentColor = Color.Magenta;
            toolPrintReports.Name = "toolPrintReports";
            toolPrintReports.Size = new Size(58, 22);
            toolPrintReports.Text = "Print  ";
            toolPrintReports.ToolTipText = "Print reports";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // toolConfiguration
            // 
            toolConfiguration.Image = (Image)resources.GetObject("toolConfiguration.Image");
            toolConfiguration.ImageTransparentColor = Color.Magenta;
            toolConfiguration.Name = "toolConfiguration";
            toolConfiguration.Size = new Size(107, 22);
            toolConfiguration.Text = "Configuration  ";
            toolConfiguration.ToolTipText = "Repository configuration";
            toolConfiguration.Click += buttonToolBar_Click;
            // 
            // panelSupManagment
            // 
            panelSupManagment.BackColor = SystemColors.ControlDarkDark;
            panelSupManagment.Controls.Add(labelReResources);
            panelSupManagment.Controls.Add(labelRepAliasCon);
            panelSupManagment.Controls.Add(labelFolderDetail);
            panelSupManagment.Controls.Add(labelFolder);
            panelSupManagment.Controls.Add(pictureBoxFolder);
            panelSupManagment.Dock = DockStyle.Top;
            panelSupManagment.Location = new Point(0, 49);
            panelSupManagment.Margin = new Padding(4, 3, 4, 3);
            panelSupManagment.Name = "panelSupManagment";
            panelSupManagment.Size = new Size(1014, 50);
            panelSupManagment.TabIndex = 5;
            // 
            // labelReResources
            // 
            labelReResources.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelReResources.BackColor = SystemColors.ControlDarkDark;
            labelReResources.ForeColor = Color.White;
            labelReResources.Location = new Point(623, 28);
            labelReResources.Margin = new Padding(0, 4, 0, 4);
            labelReResources.Name = "labelReResources";
            labelReResources.Size = new Size(382, 18);
            labelReResources.TabIndex = 4;
            labelReResources.Text = "-";
            labelReResources.TextAlign = ContentAlignment.MiddleRight;
            // 
            // labelRepAliasCon
            // 
            labelRepAliasCon.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelRepAliasCon.BackColor = SystemColors.ControlDarkDark;
            labelRepAliasCon.ForeColor = Color.White;
            labelRepAliasCon.Location = new Point(623, 5);
            labelRepAliasCon.Margin = new Padding(0, 4, 0, 4);
            labelRepAliasCon.Name = "labelRepAliasCon";
            labelRepAliasCon.Size = new Size(382, 18);
            labelRepAliasCon.TabIndex = 3;
            labelRepAliasCon.Text = "-";
            labelRepAliasCon.TextAlign = ContentAlignment.MiddleRight;
            // 
            // labelFolderDetail
            // 
            labelFolderDetail.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelFolderDetail.BackColor = SystemColors.ControlDarkDark;
            labelFolderDetail.ForeColor = Color.White;
            labelFolderDetail.Location = new Point(55, 26);
            labelFolderDetail.Margin = new Padding(0);
            labelFolderDetail.Name = "labelFolderDetail";
            labelFolderDetail.Size = new Size(560, 20);
            labelFolderDetail.TabIndex = 2;
            labelFolderDetail.Text = "Uncatalog notes folder";
            labelFolderDetail.TextAlign = ContentAlignment.MiddleLeft;
            labelFolderDetail.Click += labelFolderDetail_Click;
            // 
            // labelFolder
            // 
            labelFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelFolder.BackColor = SystemColors.ControlDarkDark;
            labelFolder.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold);
            labelFolder.ForeColor = Color.White;
            labelFolder.Location = new Point(55, 0);
            labelFolder.Margin = new Padding(4, 0, 4, 0);
            labelFolder.Name = "labelFolder";
            labelFolder.Size = new Size(560, 33);
            labelFolder.TabIndex = 1;
            labelFolder.Text = "[ - Not Catalogued ]";
            labelFolder.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pictureBoxFolder
            // 
            pictureBoxFolder.Image = (Image)resources.GetObject("pictureBoxFolder.Image");
            pictureBoxFolder.Location = new Point(8, 5);
            pictureBoxFolder.Margin = new Padding(4, 3, 4, 3);
            pictureBoxFolder.Name = "pictureBoxFolder";
            pictureBoxFolder.Size = new Size(42, 39);
            pictureBoxFolder.TabIndex = 0;
            pictureBoxFolder.TabStop = false;
            // 
            // splitContainer1
            // 
            splitContainer1.BorderStyle = BorderStyle.FixedSingle;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 99);
            splitContainer1.Margin = new Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.BackColor = SystemColors.Control;
            splitContainer1.Panel1.Controls.Add(tabExplorers);
            splitContainer1.Panel1MinSize = 20;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(1014, 569);
            splitContainer1.SplitterDistance = 294;
            splitContainer1.SplitterWidth = 6;
            splitContainer1.TabIndex = 6;
            // 
            // tabExplorers
            // 
            tabExplorers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabExplorers.Controls.Add(tabTreeFolders);
            tabExplorers.Controls.Add(tabSearch);
            tabExplorers.ImageList = imageTabExplorer;
            tabExplorers.Location = new Point(4, 4);
            tabExplorers.Margin = new Padding(4, 3, 4, 3);
            tabExplorers.Name = "tabExplorers";
            tabExplorers.Padding = new Point(6, 6);
            tabExplorers.SelectedIndex = 0;
            tabExplorers.Size = new Size(286, 560);
            tabExplorers.TabIndex = 0;
            tabExplorers.SelectedIndexChanged += tabExplorers_SelectedIndexChanged;
            // 
            // tabTreeFolders
            // 
            tabTreeFolders.ImageIndex = 0;
            tabTreeFolders.Location = new Point(4, 30);
            tabTreeFolders.Margin = new Padding(4, 3, 4, 3);
            tabTreeFolders.Name = "tabTreeFolders";
            tabTreeFolders.Padding = new Padding(4, 3, 4, 3);
            tabTreeFolders.Size = new Size(278, 526);
            tabTreeFolders.TabIndex = 0;
            tabTreeFolders.Text = "Tree folders  ";
            tabTreeFolders.ToolTipText = "Tree folders view";
            tabTreeFolders.UseVisualStyleBackColor = true;
            // 
            // tabSearch
            // 
            tabSearch.ImageIndex = 1;
            tabSearch.Location = new Point(4, 30);
            tabSearch.Margin = new Padding(4, 3, 4, 3);
            tabSearch.Name = "tabSearch";
            tabSearch.Padding = new Padding(4, 3, 4, 3);
            tabSearch.Size = new Size(278, 526);
            tabSearch.TabIndex = 1;
            tabSearch.Text = "Search  ";
            tabSearch.ToolTipText = "Search panel";
            tabSearch.UseVisualStyleBackColor = true;
            // 
            // imageTabExplorer
            // 
            imageTabExplorer.ColorDepth = ColorDepth.Depth8Bit;
            imageTabExplorer.ImageStream = (ImageListStreamer)resources.GetObject("imageTabExplorer.ImageStream");
            imageTabExplorer.TransparentColor = Color.Transparent;
            imageTabExplorer.Images.SetKeyName(0, "folderOpene_16.png");
            imageTabExplorer.Images.SetKeyName(1, "search_16.png");
            // 
            // splitContainer2
            // 
            splitContainer2.BorderStyle = BorderStyle.FixedSingle;
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Margin = new Padding(4, 3, 4, 3);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.BackColor = SystemColors.Control;
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.BackColor = SystemColors.Control;
            splitContainer2.Size = new Size(714, 569);
            splitContainer2.SplitterDistance = 192;
            splitContainer2.SplitterWidth = 7;
            splitContainer2.TabIndex = 0;
            // 
            // KNoteManagmentForm
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            ClientSize = new Size(1014, 690);
            Controls.Add(splitContainer1);
            Controls.Add(panelSupManagment);
            Controls.Add(toolBarManagment);
            Controls.Add(statusBarManagment);
            Controls.Add(menuMangment);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new Padding(4, 3, 4, 3);
            Name = "KNoteManagmentForm";
            Text = "KNote Managment";
            FormClosing += KNoteManagmentForm_FormClosing;
            Load += KNoteManagmentForm_Load;
            menuMangment.ResumeLayout(false);
            menuMangment.PerformLayout();
            statusBarManagment.ResumeLayout(false);
            statusBarManagment.PerformLayout();
            toolBarManagment.ResumeLayout(false);
            toolBarManagment.PerformLayout();
            panelSupManagment.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxFolder).EndInit();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tabExplorers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private MenuStrip menuMangment;
        private ToolStripMenuItem menuFile;
        private ToolStripMenuItem menuRepositories;
        private ToolStripMenuItem menuCreateRepository;
        private ToolStripMenuItem menuAddRepositoryLink;
        private ToolStripMenuItem menuManagmentRepository;
        private ToolStripMenuItem menuRemoveRepositoryLink;
        private ToolStripSeparator toolMenuIRepositoryS1;
        private ToolStripMenuItem menuRefreshTreeFolders;
        private ToolStripSeparator toolMenuIRepositoryS2;
        private ToolStripMenuItem menuImportData;
        private ToolStripMenuItem menuFolders;
        private ToolStripMenuItem menuNewFolder;
        private ToolStripMenuItem menuEditFolder;
        private ToolStripMenuItem menuDeleteFolder;
        private ToolStripSeparator menuFileS1;
        private ToolStripMenuItem menuHide;
        private ToolStripSeparator menuFilesS2;
        private ToolStripMenuItem menuExit;
        private ToolStripMenuItem menuEdit;
        private ToolStripMenuItem menuNewNote;
        private ToolStripMenuItem menuEditNote;
        private ToolStripMenuItem menuDeleteNote;
        private ToolStripSeparator menuEditS1;
        private ToolStripMenuItem menuMoveSelectedNotes;
        private ToolStripSeparator menuEditS2;
        private ToolStripMenuItem menuAddTags;
        private ToolStripMenuItem menuRemoveTags;
        private ToolStripSeparator menuEditS3;
        private ToolStripMenuItem menuMoreOptions;
        private ToolStripMenuItem menuExecuteKntScript;
        private ToolStripMenuItem menuView;
        private ToolStripMenuItem menuFoldersExplorer;
        private ToolStripMenuItem menuSearchPanel;
        private ToolStripMenuItem menuTools;
        private ToolStripMenuItem menuReports;
        private ToolStripSeparator menuToolsS1;
        private ToolStripSeparator menuToolsS2;
        private ToolStripMenuItem menuKntScriptConsole;
        private ToolStripMenuItem menuKNoteLab;
        private ToolStripMenuItem menuOptions;
        private ToolStripMenuItem menuHelp;
        private ToolStripMenuItem menuHelpDoc;
        private ToolStripMenuItem menuAbout;
        private StatusStrip statusBarManagment;
        private ToolStripStatusLabel statusLabel1;
        private ToolStripStatusLabel statusS1;
        private ToolStripStatusLabel statusLabel2;
        private ToolStrip toolBarManagment;
        private ToolStripButton toolNewNote;
        private ToolStripButton toolEditNote;
        private ToolStripButton toolDeleteNote;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton toolPrintReports;
        private ToolStripButton toolConfiguration;
        private Panel panelSupManagment;
        private Label labelFolderDetail;
        private Label labelFolder;
        private PictureBox pictureBoxFolder;
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private TabControl tabExplorers;
        private TabPage tabTreeFolders;
        private TabPage tabSearch;
        private ImageList imageTabExplorer;
        #endregion

        private ToolStripMenuItem menuNewNoteAsPostIt;
        private ToolStripMenuItem menuEditNoteAsPostIt;
        private ToolStripMenuItem menuHeaderPanelVisible;
        private ToolStripMenuItem menuMainVisible;
        private ToolStripMenuItem menuVerticalPanelForNotes;
        private ToolStripSeparator menuViewS1;
        private ToolStripMenuItem menuToolbarVisible;
        private ToolStripMenuItem menuChat;
        private ToolStripMenuItem menuChatGPT;
        private ToolStripProgressBar progressBar;
        private ToolStripMenuItem menuCOMPortServer;
        private Label labelReResources;
        private Label labelRepAliasCon;
    }
}