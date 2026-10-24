namespace KNote.ClientWin.Views
{
    partial class KntChatForm
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
            statusStripChat = new StatusStrip();
            toolStripStatusConnection = new ToolStripStatusLabel();
            toolStripStatusSeparator1 = new ToolStripStatusLabel();
            toolStripStatusUser = new ToolStripStatusLabel();
            toolStripStatusSeparator2 = new ToolStripStatusLabel();
            toolStripStatusServer = new ToolStripStatusLabel();
            splitThreads = new SplitContainer();
            splitChat = new SplitContainer();
            kntEditViewConversation = new KntWebView.KntEditView();
            panelConversationHeader = new Panel();
            labelOnline = new Label();
            buttonUnfollow = new Button();
            labelThread = new Label();
            textMessage = new TextBox();
            panelMessageHeader = new Panel();
            labelMessage = new Label();
            buttonSend = new Button();
            buttonSendImage = new Button();
            listViewThreads = new ListView();
            panelThreadsHeader = new Panel();
            labelThreads = new Label();
            panelRepositoryHeader = new Panel();
            labelRepository = new Label();
            comboRepositories = new ComboBox();
            buttonNewThread = new Button();
            menuChatRoom = new MenuStrip();
            menuActions = new ToolStripMenuItem();
            menuSend = new ToolStripMenuItem();
            menuSendImage = new ToolStripMenuItem();
            menuNewThread = new ToolStripMenuItem();
            menuUnfollow = new ToolStripMenuItem();
            menuActionsSeparator1 = new ToolStripSeparator();
            menuReconnect = new ToolStripMenuItem();
            statusStripChat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitThreads).BeginInit();
            splitThreads.Panel1.SuspendLayout();
            splitThreads.Panel2.SuspendLayout();
            splitThreads.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitChat).BeginInit();
            splitChat.Panel1.SuspendLayout();
            splitChat.Panel2.SuspendLayout();
            splitChat.SuspendLayout();
            panelConversationHeader.SuspendLayout();
            panelMessageHeader.SuspendLayout();
            panelThreadsHeader.SuspendLayout();
            panelRepositoryHeader.SuspendLayout();
            menuChatRoom.SuspendLayout();
            SuspendLayout();
            //
            // statusStripChat
            //
            statusStripChat.ImageScalingSize = new Size(20, 20);
            statusStripChat.Items.AddRange(new ToolStripItem[] { toolStripStatusConnection, toolStripStatusSeparator1, toolStripStatusUser, toolStripStatusSeparator2, toolStripStatusServer });
            statusStripChat.Location = new Point(0, 601);
            statusStripChat.Name = "statusStripChat";
            statusStripChat.Size = new Size(1100, 22);
            statusStripChat.TabIndex = 1;
            //
            // toolStripStatusConnection
            //
            toolStripStatusConnection.Name = "toolStripStatusConnection";
            toolStripStatusConnection.Size = new Size(79, 17);
            toolStripStatusConnection.Text = "Disconnected";
            //
            // toolStripStatusSeparator1
            //
            toolStripStatusSeparator1.Name = "toolStripStatusSeparator1";
            toolStripStatusSeparator1.Size = new Size(10, 17);
            toolStripStatusSeparator1.Text = "|";
            //
            // toolStripStatusUser
            //
            toolStripStatusUser.Name = "toolStripStatusUser";
            toolStripStatusUser.Size = new Size(30, 17);
            toolStripStatusUser.Text = "User";
            //
            // toolStripStatusSeparator2
            //
            toolStripStatusSeparator2.Name = "toolStripStatusSeparator2";
            toolStripStatusSeparator2.Size = new Size(10, 17);
            toolStripStatusSeparator2.Text = "|";
            //
            // toolStripStatusServer
            //
            toolStripStatusServer.Name = "toolStripStatusServer";
            toolStripStatusServer.Size = new Size(39, 17);
            toolStripStatusServer.Text = "Server";
            //
            // splitThreads
            //
            splitThreads.Dock = DockStyle.Fill;
            splitThreads.FixedPanel = FixedPanel.Panel2;
            splitThreads.Location = new Point(0, 24);
            splitThreads.Name = "splitThreads";
            //
            // splitThreads.Panel1
            //
            splitThreads.Panel1.Controls.Add(splitChat);
            splitThreads.Panel1MinSize = 480;
            //
            // splitThreads.Panel2
            //
            splitThreads.Panel2.Controls.Add(listViewThreads);
            splitThreads.Panel2.Controls.Add(panelThreadsHeader);
            splitThreads.Panel2.Controls.Add(panelRepositoryHeader);
            splitThreads.Panel2.Padding = new Padding(4);
            splitThreads.Panel2MinSize = 160;
            splitThreads.Size = new Size(1100, 577);
            splitThreads.SplitterDistance = 820;
            splitThreads.SplitterWidth = 6;
            splitThreads.TabIndex = 0;
            //
            // splitChat
            //
            splitChat.Dock = DockStyle.Fill;
            splitChat.FixedPanel = FixedPanel.Panel2;
            splitChat.Location = new Point(0, 0);
            splitChat.Name = "splitChat";
            splitChat.Orientation = Orientation.Horizontal;
            //
            // splitChat.Panel1
            //
            splitChat.Panel1.Controls.Add(kntEditViewConversation);
            splitChat.Panel1.Controls.Add(panelConversationHeader);
            splitChat.Panel1.Padding = new Padding(4);
            splitChat.Panel1MinSize = 200;
            //
            // splitChat.Panel2
            //
            splitChat.Panel2.Controls.Add(textMessage);
            splitChat.Panel2.Controls.Add(panelMessageHeader);
            splitChat.Panel2.Padding = new Padding(4);
            splitChat.Panel2MinSize = 70;
            splitChat.Size = new Size(820, 577);
            splitChat.SplitterDistance = 447;
            splitChat.SplitterWidth = 6;
            splitChat.TabIndex = 0;
            //
            // kntEditViewConversation
            //
            kntEditViewConversation.Dock = DockStyle.Fill;
            kntEditViewConversation.Location = new Point(4, 38);
            kntEditViewConversation.Margin = new Padding(3, 4, 3, 4);
            kntEditViewConversation.Name = "kntEditViewConversation";
            kntEditViewConversation.Size = new Size(812, 405);
            kntEditViewConversation.TabIndex = 1;
            //
            // panelConversationHeader
            //
            panelConversationHeader.Controls.Add(labelOnline);
            panelConversationHeader.Controls.Add(buttonUnfollow);
            panelConversationHeader.Controls.Add(labelThread);
            panelConversationHeader.Dock = DockStyle.Top;
            panelConversationHeader.Location = new Point(4, 4);
            panelConversationHeader.Name = "panelConversationHeader";
            panelConversationHeader.Size = new Size(812, 34);
            panelConversationHeader.TabIndex = 0;
            //
            // labelOnline
            //
            labelOnline.AutoSize = true;
            labelOnline.Font = new Font("Segoe UI", 8.25F);
            labelOnline.ForeColor = SystemColors.GrayText;
            labelOnline.Location = new Point(560, 13);
            labelOnline.Name = "labelOnline";
            labelOnline.Size = new Size(0, 13);
            labelOnline.TabIndex = 1;
            //
            // buttonUnfollow
            //
            buttonUnfollow.Font = new Font("Segoe UI", 8.25F);
            buttonUnfollow.Location = new Point(694, 4);
            buttonUnfollow.Name = "buttonUnfollow";
            buttonUnfollow.Size = new Size(112, 26);
            buttonUnfollow.TabIndex = 2;
            buttonUnfollow.Text = "Stop &following";
            buttonUnfollow.TextImageRelation = TextImageRelation.ImageBeforeText;
            buttonUnfollow.UseVisualStyleBackColor = true;
            buttonUnfollow.Click += buttonUnfollow_Click;
            //
            // labelThread
            //
            labelThread.AutoEllipsis = true;
            labelThread.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            labelThread.Location = new Point(4, 10);
            labelThread.Name = "labelThread";
            labelThread.Size = new Size(540, 20);
            labelThread.TabIndex = 0;
            labelThread.Text = "No thread selected";
            //
            // textMessage
            //
            textMessage.Dock = DockStyle.Fill;
            textMessage.Font = new Font("Segoe UI", 9.75F);
            textMessage.Location = new Point(4, 38);
            textMessage.MaxLength = 8000;
            textMessage.Multiline = true;
            textMessage.Name = "textMessage";
            textMessage.ScrollBars = ScrollBars.Vertical;
            textMessage.Size = new Size(812, 82);
            textMessage.TabIndex = 0;
            textMessage.AllowDrop = true;
            textMessage.KeyDown += textMessage_KeyDown;
            textMessage.DragEnter += textMessage_DragEnter;
            textMessage.DragDrop += textMessage_DragDrop;
            //
            // panelMessageHeader
            //
            panelMessageHeader.Controls.Add(labelMessage);
            panelMessageHeader.Controls.Add(buttonSendImage);
            panelMessageHeader.Controls.Add(buttonSend);
            panelMessageHeader.Dock = DockStyle.Top;
            panelMessageHeader.Location = new Point(4, 4);
            panelMessageHeader.Name = "panelMessageHeader";
            panelMessageHeader.Size = new Size(812, 34);
            panelMessageHeader.TabIndex = 1;
            //
            // labelMessage
            //
            labelMessage.AutoSize = true;
            labelMessage.Font = new Font("Segoe UI", 9.75F);
            labelMessage.Location = new Point(4, 12);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new Size(320, 17);
            labelMessage.TabIndex = 0;
            labelMessage.Text = "Message (Markdown; Enter sends, Shift+Enter new line; paste or drop an image):";
            //
            // buttonSend
            //
            buttonSend.Font = new Font("Segoe UI", 8.25F);
            buttonSend.Location = new Point(728, 4);
            buttonSend.Name = "buttonSend";
            buttonSend.Size = new Size(78, 26);
            buttonSend.TabIndex = 1;
            buttonSend.Text = "&Send";
            buttonSend.TextImageRelation = TextImageRelation.ImageBeforeText;
            buttonSend.UseVisualStyleBackColor = true;
            buttonSend.Click += buttonSend_Click;
            //
            // buttonSendImage
            //
            buttonSendImage.Font = new Font("Segoe UI", 8.25F);
            buttonSendImage.Location = new Point(632, 4);
            buttonSendImage.Name = "buttonSendImage";
            buttonSendImage.Size = new Size(90, 26);
            buttonSendImage.TabIndex = 2;
            buttonSendImage.Text = "&Image ...";
            buttonSendImage.TextImageRelation = TextImageRelation.ImageBeforeText;
            buttonSendImage.UseVisualStyleBackColor = true;
            buttonSendImage.Click += buttonSendImage_Click;
            //
            // listViewThreads
            //
            listViewThreads.Dock = DockStyle.Fill;
            listViewThreads.HideSelection = false;
            listViewThreads.Location = new Point(4, 92);
            listViewThreads.MultiSelect = false;
            listViewThreads.Name = "listViewThreads";
            listViewThreads.Size = new Size(266, 481);
            listViewThreads.TabIndex = 1;
            listViewThreads.UseCompatibleStateImageBehavior = false;
            listViewThreads.ItemSelectionChanged += listViewThreads_ItemSelectionChanged;
            //
            // panelThreadsHeader
            //
            panelThreadsHeader.Controls.Add(labelThreads);
            panelThreadsHeader.Controls.Add(buttonNewThread);
            panelThreadsHeader.Dock = DockStyle.Top;
            panelThreadsHeader.Location = new Point(4, 58);
            panelThreadsHeader.Name = "panelThreadsHeader";
            panelThreadsHeader.Size = new Size(266, 34);
            panelThreadsHeader.TabIndex = 0;
            //
            // labelThreads
            //
            labelThreads.AutoSize = true;
            labelThreads.Font = new Font("Segoe UI", 9.75F);
            labelThreads.Location = new Point(4, 12);
            labelThreads.Name = "labelThreads";
            labelThreads.Size = new Size(56, 17);
            labelThreads.TabIndex = 0;
            labelThreads.Text = "Threads:";
            //
            // buttonNewThread
            //
            buttonNewThread.Font = new Font("Segoe UI", 8.25F);
            buttonNewThread.Location = new Point(156, 4);
            buttonNewThread.Name = "buttonNewThread";
            buttonNewThread.Size = new Size(104, 26);
            buttonNewThread.TabIndex = 1;
            buttonNewThread.Text = "&New thread";
            buttonNewThread.TextImageRelation = TextImageRelation.ImageBeforeText;
            buttonNewThread.UseVisualStyleBackColor = true;
            buttonNewThread.Click += buttonNewThread_Click;
            //
            // panelRepositoryHeader
            //
            panelRepositoryHeader.Controls.Add(labelRepository);
            panelRepositoryHeader.Controls.Add(comboRepositories);
            panelRepositoryHeader.Dock = DockStyle.Top;
            panelRepositoryHeader.Location = new Point(4, 4);
            panelRepositoryHeader.Name = "panelRepositoryHeader";
            panelRepositoryHeader.Padding = new Padding(0, 0, 0, 2);
            panelRepositoryHeader.Size = new Size(266, 54);
            panelRepositoryHeader.TabIndex = 2;
            //
            // labelRepository
            //
            labelRepository.AutoSize = true;
            labelRepository.Font = new Font("Segoe UI", 9.75F);
            labelRepository.Location = new Point(4, 6);
            labelRepository.Name = "labelRepository";
            labelRepository.Size = new Size(184, 17);
            labelRepository.TabIndex = 0;
            labelRepository.Text = "Chat room of the repository:";
            //
            // comboRepositories
            //
            comboRepositories.Dock = DockStyle.Bottom;
            comboRepositories.DropDownStyle = ComboBoxStyle.DropDownList;
            comboRepositories.Location = new Point(0, 29);
            comboRepositories.Name = "comboRepositories";
            comboRepositories.Size = new Size(266, 23);
            comboRepositories.TabIndex = 1;
            //
            // menuChatRoom
            //
            menuChatRoom.Items.AddRange(new ToolStripItem[] { menuActions });
            menuChatRoom.Location = new Point(0, 0);
            menuChatRoom.Name = "menuChatRoom";
            menuChatRoom.Padding = new Padding(7, 2, 0, 2);
            menuChatRoom.Size = new Size(1100, 24);
            menuChatRoom.TabIndex = 2;
            menuChatRoom.Text = "KNote chat room menu";
            //
            // menuActions
            //
            menuActions.DropDownItems.AddRange(new ToolStripItem[] { menuSend, menuSendImage, menuNewThread, menuUnfollow, menuActionsSeparator1, menuReconnect });
            menuActions.Name = "menuActions";
            menuActions.Size = new Size(59, 20);
            menuActions.Text = "&Actions";
            //
            // menuSend
            //
            menuSend.Name = "menuSend";
            menuSend.ShortcutKeys = Keys.Control | Keys.Return;
            menuSend.Size = new Size(220, 22);
            menuSend.Text = "&Send";
            menuSend.Click += buttonSend_Click;
            //
            // menuSendImage
            //
            menuSendImage.Name = "menuSendImage";
            menuSendImage.Size = new Size(220, 22);
            menuSendImage.Text = "Send &image ...";
            menuSendImage.Click += buttonSendImage_Click;
            //
            // menuNewThread
            //
            menuNewThread.Name = "menuNewThread";
            menuNewThread.Size = new Size(220, 22);
            menuNewThread.Text = "&New thread ...";
            menuNewThread.Click += buttonNewThread_Click;
            //
            // menuUnfollow
            //
            menuUnfollow.Name = "menuUnfollow";
            menuUnfollow.Size = new Size(220, 22);
            menuUnfollow.Text = "Stop &following the thread";
            menuUnfollow.Click += buttonUnfollow_Click;
            //
            // menuActionsSeparator1
            //
            menuActionsSeparator1.Name = "menuActionsSeparator1";
            menuActionsSeparator1.Size = new Size(217, 6);
            //
            // menuReconnect
            //
            menuReconnect.Name = "menuReconnect";
            menuReconnect.Size = new Size(220, 22);
            menuReconnect.Text = "&Reconnect";
            menuReconnect.Click += menuReconnect_Click;
            //
            // KntChatForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 623);
            MinimumSize = new Size(820, 480);
            Controls.Add(splitThreads);
            Controls.Add(statusStripChat);
            Controls.Add(menuChatRoom);
            MainMenuStrip = menuChatRoom;
            Name = "KntChatForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Chat room";
            Load += ChatForm_Load;
            statusStripChat.ResumeLayout(false);
            statusStripChat.PerformLayout();
            splitThreads.Panel1.ResumeLayout(false);
            splitThreads.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitThreads).EndInit();
            splitThreads.ResumeLayout(false);
            splitChat.Panel1.ResumeLayout(false);
            splitChat.Panel2.ResumeLayout(false);
            splitChat.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitChat).EndInit();
            splitChat.ResumeLayout(false);
            panelConversationHeader.ResumeLayout(false);
            panelConversationHeader.PerformLayout();
            panelMessageHeader.ResumeLayout(false);
            panelMessageHeader.PerformLayout();
            panelThreadsHeader.ResumeLayout(false);
            panelThreadsHeader.PerformLayout();
            panelRepositoryHeader.ResumeLayout(false);
            panelRepositoryHeader.PerformLayout();
            menuChatRoom.ResumeLayout(false);
            menuChatRoom.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStripChat;
        private ToolStripStatusLabel toolStripStatusConnection;
        private ToolStripStatusLabel toolStripStatusSeparator1;
        private ToolStripStatusLabel toolStripStatusUser;
        private ToolStripStatusLabel toolStripStatusSeparator2;
        private ToolStripStatusLabel toolStripStatusServer;
        private SplitContainer splitThreads;
        private SplitContainer splitChat;
        private KntWebView.KntEditView kntEditViewConversation;
        private Panel panelConversationHeader;
        private Label labelThread;
        private Label labelOnline;
        private Button buttonUnfollow;
        private TextBox textMessage;
        private Panel panelMessageHeader;
        private Label labelMessage;
        private Button buttonSend;
        private Button buttonSendImage;
        private ListView listViewThreads;
        private Panel panelThreadsHeader;
        private Label labelThreads;
        private Panel panelRepositoryHeader;
        private Label labelRepository;
        private ComboBox comboRepositories;
        private Button buttonNewThread;
        private MenuStrip menuChatRoom;
        private ToolStripMenuItem menuActions;
        private ToolStripMenuItem menuSend;
        private ToolStripMenuItem menuSendImage;
        private ToolStripMenuItem menuNewThread;
        private ToolStripMenuItem menuUnfollow;
        private ToolStripSeparator menuActionsSeparator1;
        private ToolStripMenuItem menuReconnect;
    }
}
