namespace KNote.ClientWin.Views
{
    partial class AtlasContextForm
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
            labelHelp = new Label();
            labelRepository = new Label();
            comboRepositories = new ComboBox();
            tabContext = new TabControl();
            tabProfile = new TabPage();
            labelProfileHelp = new Label();
            labelProfile = new Label();
            buttonCreateProfile = new Button();
            buttonOpenProfile = new Button();
            buttonFillProfile = new Button();
            tabGlossary = new TabPage();
            labelGlossaryHelp = new Label();
            labelGlossary = new Label();
            buttonCreateGlossary = new Button();
            buttonOpenGlossary = new Button();
            tabTasks = new TabPage();
            labelTasksHelp = new Label();
            listTasks = new ListView();
            columnTask = new ColumnHeader();
            columnState = new ColumnHeader();
            labelTaskNotices = new Label();
            buttonKNote = new Button();
            buttonCustomize = new Button();
            tabPrompt = new TabPage();
            labelPromptHelp = new Label();
            labelTask = new Label();
            comboTasks = new ComboBox();
            labelTest = new Label();
            textTest = new TextBox();
            buttonTry = new Button();
            labelSummary = new Label();
            textPrompt = new TextBox();
            buttonClose = new Button();
            tabContext.SuspendLayout();
            tabProfile.SuspendLayout();
            tabGlossary.SuspendLayout();
            tabTasks.SuspendLayout();
            tabPrompt.SuspendLayout();
            SuspendLayout();
            //
            // labelHelp
            //
            labelHelp.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelHelp.Location = new Point(12, 10);
            labelHelp.Name = "labelHelp";
            labelHelp.Size = new Size(696, 50);
            labelHelp.TabIndex = 0;
            labelHelp.Text = "KNote Atlas gives the AI the context of a repository: what it is for, its vocabulary and how you want it to work. The profile, the glossary and the instructions are notes of the folder «KNote Atlas», written in the note editor; only a ProjectManager or an Admin saves them.";
            //
            // labelRepository
            //
            labelRepository.AutoSize = true;
            labelRepository.Location = new Point(12, 68);
            labelRepository.Name = "labelRepository";
            labelRepository.Size = new Size(65, 15);
            labelRepository.TabIndex = 1;
            labelRepository.Text = "&Repository:";
            //
            // comboRepositories
            //
            comboRepositories.DropDownStyle = ComboBoxStyle.DropDownList;
            comboRepositories.FormattingEnabled = true;
            comboRepositories.Location = new Point(90, 64);
            comboRepositories.Name = "comboRepositories";
            comboRepositories.Size = new Size(280, 23);
            comboRepositories.TabIndex = 2;
            //
            // tabContext
            //
            tabContext.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabContext.Controls.Add(tabProfile);
            tabContext.Controls.Add(tabGlossary);
            tabContext.Controls.Add(tabTasks);
            tabContext.Controls.Add(tabPrompt);
            tabContext.Location = new Point(12, 96);
            tabContext.Name = "tabContext";
            tabContext.SelectedIndex = 0;
            tabContext.Size = new Size(696, 416);
            tabContext.TabIndex = 3;
            //
            // tabProfile
            //
            tabProfile.Controls.Add(buttonFillProfile);
            tabProfile.Controls.Add(buttonOpenProfile);
            tabProfile.Controls.Add(buttonCreateProfile);
            tabProfile.Controls.Add(labelProfile);
            tabProfile.Controls.Add(labelProfileHelp);
            tabProfile.Location = new Point(4, 24);
            tabProfile.Name = "tabProfile";
            tabProfile.Size = new Size(688, 388);
            tabProfile.TabIndex = 0;
            tabProfile.Text = "Repository profile";
            tabProfile.UseVisualStyleBackColor = true;
            //
            // labelProfileHelp
            //
            labelProfileHelp.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelProfileHelp.Location = new Point(8, 10);
            labelProfileHelp.Name = "labelProfileHelp";
            labelProfileHelp.Size = new Size(668, 34);
            labelProfileHelp.TabIndex = 0;
            labelProfileHelp.Text = "The profile tells the AI what this repository is for. It receives it in all its tasks.";
            //
            // labelProfile
            //
            labelProfile.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelProfile.BorderStyle = BorderStyle.FixedSingle;
            labelProfile.Location = new Point(8, 48);
            labelProfile.Name = "labelProfile";
            labelProfile.Padding = new Padding(6);
            labelProfile.Size = new Size(668, 293);
            labelProfile.TabIndex = 1;
            //
            // buttonCreateProfile
            //
            buttonCreateProfile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonCreateProfile.Location = new Point(8, 351);
            buttonCreateProfile.Name = "buttonCreateProfile";
            buttonCreateProfile.Size = new Size(166, 29);
            buttonCreateProfile.TabIndex = 2;
            buttonCreateProfile.Text = "&Create the profile";
            buttonCreateProfile.UseVisualStyleBackColor = true;
            buttonCreateProfile.Visible = false;
            buttonCreateProfile.Click += buttonCreateProfile_Click;
            //
            // buttonOpenProfile
            //
            buttonOpenProfile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonOpenProfile.Location = new Point(8, 351);
            buttonOpenProfile.Name = "buttonOpenProfile";
            buttonOpenProfile.Size = new Size(166, 29);
            buttonOpenProfile.TabIndex = 3;
            buttonOpenProfile.Text = "&Open in the editor";
            buttonOpenProfile.UseVisualStyleBackColor = true;
            buttonOpenProfile.Visible = false;
            buttonOpenProfile.Click += buttonOpenProfile_Click;
            //
            // buttonFillProfile
            //
            buttonFillProfile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonFillProfile.Location = new Point(183, 351);
            buttonFillProfile.Name = "buttonFillProfile";
            buttonFillProfile.Size = new Size(166, 29);
            buttonFillProfile.TabIndex = 4;
            buttonFillProfile.Text = "&Fill in folders and types";
            buttonFillProfile.UseVisualStyleBackColor = true;
            buttonFillProfile.Click += buttonFillProfile_Click;
            //
            // tabGlossary
            //
            tabGlossary.Controls.Add(buttonOpenGlossary);
            tabGlossary.Controls.Add(buttonCreateGlossary);
            tabGlossary.Controls.Add(labelGlossary);
            tabGlossary.Controls.Add(labelGlossaryHelp);
            tabGlossary.Location = new Point(4, 24);
            tabGlossary.Name = "tabGlossary";
            tabGlossary.Size = new Size(688, 388);
            tabGlossary.TabIndex = 1;
            tabGlossary.Text = "Glossary";
            tabGlossary.UseVisualStyleBackColor = true;
            //
            // labelGlossaryHelp
            //
            labelGlossaryHelp.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelGlossaryHelp.Location = new Point(8, 10);
            labelGlossaryHelp.Name = "labelGlossaryHelp";
            labelGlossaryHelp.Size = new Size(668, 34);
            labelGlossaryHelp.TabIndex = 0;
            labelGlossaryHelp.Text = "The terms, acronyms and proper names of the repository. The AI only receives the terms that appear in the conversation.";
            //
            // labelGlossary
            //
            labelGlossary.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelGlossary.BorderStyle = BorderStyle.FixedSingle;
            labelGlossary.Location = new Point(8, 48);
            labelGlossary.Name = "labelGlossary";
            labelGlossary.Padding = new Padding(6);
            labelGlossary.Size = new Size(668, 293);
            labelGlossary.TabIndex = 1;
            //
            // buttonCreateGlossary
            //
            buttonCreateGlossary.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonCreateGlossary.Location = new Point(8, 351);
            buttonCreateGlossary.Name = "buttonCreateGlossary";
            buttonCreateGlossary.Size = new Size(166, 29);
            buttonCreateGlossary.TabIndex = 2;
            buttonCreateGlossary.Text = "&Create the glossary";
            buttonCreateGlossary.UseVisualStyleBackColor = true;
            buttonCreateGlossary.Visible = false;
            buttonCreateGlossary.Click += buttonCreateGlossary_Click;
            //
            // buttonOpenGlossary
            //
            buttonOpenGlossary.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonOpenGlossary.Location = new Point(8, 351);
            buttonOpenGlossary.Name = "buttonOpenGlossary";
            buttonOpenGlossary.Size = new Size(166, 29);
            buttonOpenGlossary.TabIndex = 3;
            buttonOpenGlossary.Text = "&Open in the editor";
            buttonOpenGlossary.UseVisualStyleBackColor = true;
            buttonOpenGlossary.Visible = false;
            buttonOpenGlossary.Click += buttonOpenGlossary_Click;
            //
            // tabTasks
            //
            tabTasks.Controls.Add(buttonCustomize);
            tabTasks.Controls.Add(buttonKNote);
            tabTasks.Controls.Add(labelTaskNotices);
            tabTasks.Controls.Add(listTasks);
            tabTasks.Controls.Add(labelTasksHelp);
            tabTasks.Location = new Point(4, 24);
            tabTasks.Name = "tabTasks";
            tabTasks.Size = new Size(688, 388);
            tabTasks.TabIndex = 2;
            tabTasks.Text = "Task instructions";
            tabTasks.UseVisualStyleBackColor = true;
            //
            // labelTasksHelp
            //
            labelTasksHelp.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelTasksHelp.Location = new Point(8, 10);
            labelTasksHelp.Name = "labelTasksHelp";
            labelTasksHelp.Size = new Size(668, 34);
            labelTasksHelp.TabIndex = 0;
            labelTasksHelp.Text = "Each task of KNote Atlas has KNote's instructions. A note of instructions adds to them or replaces them in this repository (KNote's fixed rules always stay).";
            //
            // listTasks
            //
            listTasks.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listTasks.Columns.AddRange(new ColumnHeader[] { columnTask, columnState });
            listTasks.FullRowSelect = true;
            listTasks.HideSelection = false;
            listTasks.Location = new Point(8, 48);
            listTasks.MultiSelect = false;
            listTasks.Name = "listTasks";
            listTasks.Size = new Size(668, 255);
            listTasks.TabIndex = 1;
            listTasks.UseCompatibleStateImageBehavior = false;
            listTasks.View = View.Details;
            //
            // columnTask
            //
            columnTask.Text = "Task";
            columnTask.Width = 330;
            //
            // columnState
            //
            columnState.Text = "State";
            columnState.Width = 300;
            //
            // labelTaskNotices
            //
            labelTaskNotices.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelTaskNotices.ForeColor = SystemColors.GrayText;
            labelTaskNotices.Location = new Point(8, 308);
            labelTaskNotices.Name = "labelTaskNotices";
            labelTaskNotices.Size = new Size(668, 34);
            labelTaskNotices.TabIndex = 2;
            //
            // buttonKNote
            //
            buttonKNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonKNote.Location = new Point(8, 351);
            buttonKNote.Name = "buttonKNote";
            buttonKNote.Size = new Size(166, 29);
            buttonKNote.TabIndex = 3;
            buttonKNote.Text = "See &KNote's";
            buttonKNote.UseVisualStyleBackColor = true;
            buttonKNote.Click += buttonKNote_Click;
            //
            // buttonCustomize
            //
            buttonCustomize.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonCustomize.Location = new Point(183, 351);
            buttonCustomize.Name = "buttonCustomize";
            buttonCustomize.Size = new Size(166, 29);
            buttonCustomize.TabIndex = 4;
            buttonCustomize.Text = "C&ustomize";
            buttonCustomize.UseVisualStyleBackColor = true;
            buttonCustomize.Click += buttonCustomize_Click;
            //
            // tabPrompt
            //
            tabPrompt.Controls.Add(textPrompt);
            tabPrompt.Controls.Add(labelSummary);
            tabPrompt.Controls.Add(buttonTry);
            tabPrompt.Controls.Add(textTest);
            tabPrompt.Controls.Add(labelTest);
            tabPrompt.Controls.Add(comboTasks);
            tabPrompt.Controls.Add(labelTask);
            tabPrompt.Controls.Add(labelPromptHelp);
            tabPrompt.Location = new Point(4, 24);
            tabPrompt.Name = "tabPrompt";
            tabPrompt.Size = new Size(688, 388);
            tabPrompt.TabIndex = 3;
            tabPrompt.Text = "What the AI receives";
            tabPrompt.UseVisualStyleBackColor = true;
            //
            // labelPromptHelp
            //
            labelPromptHelp.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelPromptHelp.Location = new Point(8, 10);
            labelPromptHelp.Name = "labelPromptHelp";
            labelPromptHelp.Size = new Size(668, 18);
            labelPromptHelp.TabIndex = 0;
            labelPromptHelp.Text = "The text the AI gets, as it is sent. The glossary terms that appear in the test text go in.";
            //
            // labelTask
            //
            labelTask.AutoSize = true;
            labelTask.Location = new Point(8, 37);
            labelTask.Name = "labelTask";
            labelTask.Size = new Size(32, 15);
            labelTask.TabIndex = 1;
            labelTask.Text = "T&ask:";
            //
            // comboTasks
            //
            comboTasks.DropDownStyle = ComboBoxStyle.DropDownList;
            comboTasks.FormattingEnabled = true;
            comboTasks.Location = new Point(70, 33);
            comboTasks.Name = "comboTasks";
            comboTasks.Size = new Size(220, 23);
            comboTasks.TabIndex = 2;
            //
            // labelTest
            //
            labelTest.AutoSize = true;
            labelTest.Location = new Point(8, 67);
            labelTest.Name = "labelTest";
            labelTest.Size = new Size(54, 15);
            labelTest.TabIndex = 3;
            labelTest.Text = "&Test text:";
            //
            // textTest
            //
            textTest.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textTest.Location = new Point(70, 63);
            textTest.Name = "textTest";
            textTest.Size = new Size(476, 23);
            textTest.TabIndex = 4;
            //
            // buttonTry
            //
            buttonTry.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonTry.Location = new Point(552, 61);
            buttonTry.Name = "buttonTry";
            buttonTry.Size = new Size(124, 27);
            buttonTry.TabIndex = 5;
            buttonTry.Text = "&Show its terms";
            buttonTry.UseVisualStyleBackColor = true;
            buttonTry.Click += buttonTry_Click;
            //
            // labelSummary
            //
            labelSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelSummary.ForeColor = SystemColors.GrayText;
            labelSummary.Location = new Point(8, 94);
            labelSummary.Name = "labelSummary";
            labelSummary.Size = new Size(668, 32);
            labelSummary.TabIndex = 6;
            //
            // textPrompt
            //
            textPrompt.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textPrompt.BackColor = SystemColors.Window;
            textPrompt.Font = new Font("Consolas", 9.75F);
            textPrompt.Location = new Point(8, 130);
            textPrompt.Multiline = true;
            textPrompt.Name = "textPrompt";
            textPrompt.ReadOnly = true;
            textPrompt.ScrollBars = ScrollBars.Vertical;
            textPrompt.Size = new Size(668, 250);
            textPrompt.TabIndex = 7;
            //
            // buttonClose
            //
            buttonClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonClose.DialogResult = DialogResult.Cancel;
            buttonClose.Location = new Point(632, 520);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new Size(76, 29);
            buttonClose.TabIndex = 4;
            buttonClose.Text = "C&lose";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += buttonClose_Click;
            //
            // AtlasContextForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonClose;
            ClientSize = new Size(720, 560);
            Controls.Add(buttonClose);
            Controls.Add(tabContext);
            Controls.Add(comboRepositories);
            Controls.Add(labelRepository);
            Controls.Add(labelHelp);
            MinimumSize = new Size(640, 480);
            Name = "AtlasContextForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "KNote Atlas";
            tabContext.ResumeLayout(false);
            tabProfile.ResumeLayout(false);
            tabGlossary.ResumeLayout(false);
            tabTasks.ResumeLayout(false);
            tabPrompt.ResumeLayout(false);
            tabPrompt.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelHelp;
        private Label labelRepository;
        private ComboBox comboRepositories;
        private TabControl tabContext;
        private TabPage tabProfile;
        private Label labelProfileHelp;
        private Label labelProfile;
        private Button buttonCreateProfile;
        private Button buttonOpenProfile;
        private Button buttonFillProfile;
        private TabPage tabGlossary;
        private Label labelGlossaryHelp;
        private Label labelGlossary;
        private Button buttonCreateGlossary;
        private Button buttonOpenGlossary;
        private TabPage tabTasks;
        private Label labelTasksHelp;
        private ListView listTasks;
        private ColumnHeader columnTask;
        private ColumnHeader columnState;
        private Label labelTaskNotices;
        private Button buttonKNote;
        private Button buttonCustomize;
        private TabPage tabPrompt;
        private Label labelPromptHelp;
        private Label labelTask;
        private ComboBox comboTasks;
        private Label labelTest;
        private TextBox textTest;
        private Button buttonTry;
        private Label labelSummary;
        private TextBox textPrompt;
        private Button buttonClose;
    }
}
