
namespace KNote.ClientWin.Views
{
    partial class TaskEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaskEditorForm));
            buttonCancel = new Button();
            buttonAccept = new Button();
            tabTaskData = new TabControl();
            tabPageBasicData = new TabPage();
            textPriority = new TextBox();
            label3 = new Label();
            checkResolved = new CheckBox();
            buttonSelDateEnd = new Button();
            textEndDate = new TextBox();
            label10 = new Label();
            buttonSelDateStart = new Button();
            textStartDate = new TextBox();
            label9 = new Label();
            textTags = new TextBox();
            label2 = new Label();
            textDescription = new TextBox();
            textUser = new TextBox();
            label11 = new Label();
            label1 = new Label();
            tabPageMoreDetails = new TabPage();
            buttonSelDateExE = new Button();
            buttonSelDateExS = new Button();
            textDificultyLevel = new TextBox();
            textSpendTime = new TextBox();
            textExEndDate = new TextBox();
            textExStartDate = new TextBox();
            label8 = new Label();
            label7 = new Label();
            label6 = new Label();
            label5 = new Label();
            textEstimatedTime = new TextBox();
            label4 = new Label();
            tabTaskData.SuspendLayout();
            tabPageBasicData.SuspendLayout();
            tabPageMoreDetails.SuspendLayout();
            SuspendLayout();
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Location = new Point(538, 455);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(81, 29);
            buttonCancel.TabIndex = 10;
            buttonCancel.Text = "&Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonAccept
            // 
            buttonAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAccept.Location = new Point(451, 455);
            buttonAccept.Name = "buttonAccept";
            buttonAccept.Size = new Size(81, 29);
            buttonAccept.TabIndex = 9;
            buttonAccept.Text = "&Accept";
            buttonAccept.UseVisualStyleBackColor = true;
            buttonAccept.Click += buttonAccept_Click;
            // 
            // tabTaskData
            // 
            tabTaskData.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabTaskData.Controls.Add(tabPageBasicData);
            tabTaskData.Controls.Add(tabPageMoreDetails);
            tabTaskData.Location = new Point(6, 6);
            tabTaskData.Name = "tabTaskData";
            tabTaskData.SelectedIndex = 0;
            tabTaskData.Size = new Size(623, 434);
            tabTaskData.TabIndex = 11;
            // 
            // tabPageBasicData
            // 
            tabPageBasicData.Controls.Add(textPriority);
            tabPageBasicData.Controls.Add(label3);
            tabPageBasicData.Controls.Add(checkResolved);
            tabPageBasicData.Controls.Add(buttonSelDateEnd);
            tabPageBasicData.Controls.Add(textEndDate);
            tabPageBasicData.Controls.Add(label10);
            tabPageBasicData.Controls.Add(buttonSelDateStart);
            tabPageBasicData.Controls.Add(textStartDate);
            tabPageBasicData.Controls.Add(label9);
            tabPageBasicData.Controls.Add(textTags);
            tabPageBasicData.Controls.Add(label2);
            tabPageBasicData.Controls.Add(textDescription);
            tabPageBasicData.Controls.Add(textUser);
            tabPageBasicData.Controls.Add(label11);
            tabPageBasicData.Controls.Add(label1);
            tabPageBasicData.Location = new Point(4, 24);
            tabPageBasicData.Name = "tabPageBasicData";
            tabPageBasicData.Padding = new Padding(3);
            tabPageBasicData.Size = new Size(615, 406);
            tabPageBasicData.TabIndex = 0;
            tabPageBasicData.Text = "Basic task/activity data";
            tabPageBasicData.UseVisualStyleBackColor = true;
            // 
            // textPriority
            // 
            textPriority.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            textPriority.Location = new Point(62, 376);
            textPriority.Name = "textPriority";
            textPriority.Size = new Size(34, 23);
            textPriority.TabIndex = 3;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new Point(8, 380);
            label3.Name = "label3";
            label3.Size = new Size(48, 15);
            label3.TabIndex = 26;
            label3.Text = "Priority:";
            // 
            // checkResolved
            // 
            checkResolved.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            checkResolved.AutoSize = true;
            checkResolved.Location = new Point(536, 378);
            checkResolved.Name = "checkResolved";
            checkResolved.Size = new Size(73, 19);
            checkResolved.TabIndex = 8;
            checkResolved.Text = "&Resolved";
            checkResolved.UseVisualStyleBackColor = true;
            checkResolved.Click += checkResolved_Click;
            // 
            // buttonSelDateEnd
            // 
            buttonSelDateEnd.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonSelDateEnd.Location = new Point(498, 375);
            buttonSelDateEnd.Name = "buttonSelDateEnd";
            buttonSelDateEnd.Size = new Size(24, 24);
            buttonSelDateEnd.TabIndex = 7;
            buttonSelDateEnd.Text = "...";
            buttonSelDateEnd.UseVisualStyleBackColor = true;
            buttonSelDateEnd.Click += buttonSelDate_Click;
            // 
            // textEndDate
            // 
            textEndDate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            textEndDate.Location = new Point(386, 376);
            textEndDate.Name = "textEndDate";
            textEndDate.Size = new Size(106, 23);
            textEndDate.TabIndex = 6;
            // 
            // label10
            // 
            label10.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label10.AutoSize = true;
            label10.Location = new Point(326, 380);
            label10.Name = "label10";
            label10.Size = new Size(56, 15);
            label10.TabIndex = 21;
            label10.Text = "End date:";
            // 
            // buttonSelDateStart
            // 
            buttonSelDateStart.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonSelDateStart.Location = new Point(285, 375);
            buttonSelDateStart.Name = "buttonSelDateStart";
            buttonSelDateStart.Size = new Size(24, 24);
            buttonSelDateStart.TabIndex = 5;
            buttonSelDateStart.Text = "...";
            buttonSelDateStart.UseVisualStyleBackColor = true;
            buttonSelDateStart.Click += buttonSelDate_Click;
            // 
            // textStartDate
            // 
            textStartDate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            textStartDate.Location = new Point(172, 376);
            textStartDate.Name = "textStartDate";
            textStartDate.Size = new Size(107, 23);
            textStartDate.TabIndex = 4;
            // 
            // label9
            // 
            label9.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label9.AutoSize = true;
            label9.Location = new Point(108, 380);
            label9.Name = "label9";
            label9.Size = new Size(60, 15);
            label9.TabIndex = 18;
            label9.Text = "Start date:";
            // 
            // textTags
            // 
            textTags.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textTags.Location = new Point(52, 32);
            textTags.Name = "textTags";
            textTags.Size = new Size(557, 23);
            textTags.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 35);
            label2.Name = "label2";
            label2.Size = new Size(33, 15);
            label2.TabIndex = 17;
            label2.Text = "Tags:";
            // 
            // textDescription
            // 
            textDescription.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textDescription.Location = new Point(6, 84);
            textDescription.Multiline = true;
            textDescription.Name = "textDescription";
            textDescription.ScrollBars = ScrollBars.Vertical;
            textDescription.Size = new Size(603, 280);
            textDescription.TabIndex = 2;
            // 
            // textUser
            // 
            textUser.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textUser.Enabled = false;
            textUser.Location = new Point(52, 6);
            textUser.Name = "textUser";
            textUser.Size = new Size(557, 23);
            textUser.TabIndex = 0;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(6, 64);
            label11.Name = "label11";
            label11.Size = new Size(70, 15);
            label11.TabIndex = 15;
            label11.Text = "Description:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 9);
            label1.Name = "label1";
            label1.Size = new Size(33, 15);
            label1.TabIndex = 14;
            label1.Text = "User:";
            // 
            // tabPageMoreDetails
            // 
            tabPageMoreDetails.Controls.Add(buttonSelDateExE);
            tabPageMoreDetails.Controls.Add(buttonSelDateExS);
            tabPageMoreDetails.Controls.Add(textDificultyLevel);
            tabPageMoreDetails.Controls.Add(textSpendTime);
            tabPageMoreDetails.Controls.Add(textExEndDate);
            tabPageMoreDetails.Controls.Add(textExStartDate);
            tabPageMoreDetails.Controls.Add(label8);
            tabPageMoreDetails.Controls.Add(label7);
            tabPageMoreDetails.Controls.Add(label6);
            tabPageMoreDetails.Controls.Add(label5);
            tabPageMoreDetails.Controls.Add(textEstimatedTime);
            tabPageMoreDetails.Controls.Add(label4);
            tabPageMoreDetails.Location = new Point(4, 24);
            tabPageMoreDetails.Name = "tabPageMoreDetails";
            tabPageMoreDetails.Padding = new Padding(3);
            tabPageMoreDetails.Size = new Size(615, 406);
            tabPageMoreDetails.TabIndex = 1;
            tabPageMoreDetails.Text = "Additional data";
            tabPageMoreDetails.UseVisualStyleBackColor = true;
            // 
            // buttonSelDateExE
            // 
            buttonSelDateExE.Location = new Point(276, 52);
            buttonSelDateExE.Name = "buttonSelDateExE";
            buttonSelDateExE.Size = new Size(24, 24);
            buttonSelDateExE.TabIndex = 15;
            buttonSelDateExE.Text = "...";
            buttonSelDateExE.UseVisualStyleBackColor = true;
            buttonSelDateExE.Click += buttonSelDate_Click;
            // 
            // buttonSelDateExS
            // 
            buttonSelDateExS.Location = new Point(276, 16);
            buttonSelDateExS.Name = "buttonSelDateExS";
            buttonSelDateExS.Size = new Size(24, 24);
            buttonSelDateExS.TabIndex = 13;
            buttonSelDateExS.Text = "...";
            buttonSelDateExS.UseVisualStyleBackColor = true;
            buttonSelDateExS.Click += buttonSelDate_Click;
            // 
            // textDificultyLevel
            // 
            textDificultyLevel.Location = new Point(141, 160);
            textDificultyLevel.Name = "textDificultyLevel";
            textDificultyLevel.Size = new Size(92, 23);
            textDificultyLevel.TabIndex = 18;
            // 
            // textSpendTime
            // 
            textSpendTime.Location = new Point(141, 124);
            textSpendTime.Name = "textSpendTime";
            textSpendTime.Size = new Size(92, 23);
            textSpendTime.TabIndex = 17;
            // 
            // textExEndDate
            // 
            textExEndDate.Location = new Point(141, 52);
            textExEndDate.Name = "textExEndDate";
            textExEndDate.Size = new Size(129, 23);
            textExEndDate.TabIndex = 14;
            // 
            // textExStartDate
            // 
            textExStartDate.Location = new Point(141, 16);
            textExStartDate.Name = "textExStartDate";
            textExStartDate.Size = new Size(129, 23);
            textExStartDate.TabIndex = 12;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(16, 55);
            label8.Name = "label8";
            label8.Size = new Size(107, 15);
            label8.TabIndex = 23;
            label8.Text = "Expected end date:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(16, 19);
            label7.Name = "label7";
            label7.Size = new Size(110, 15);
            label7.TabIndex = 21;
            label7.Text = "Expected start date:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(16, 163);
            label6.Name = "label6";
            label6.Size = new Size(81, 15);
            label6.TabIndex = 19;
            label6.Text = "Dificulty level:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(16, 127);
            label5.Name = "label5";
            label5.Size = new Size(70, 15);
            label5.TabIndex = 16;
            label5.Text = "Spend time:";
            // 
            // textEstimatedTime
            // 
            textEstimatedTime.Location = new Point(141, 88);
            textEstimatedTime.Name = "textEstimatedTime";
            textEstimatedTime.Size = new Size(92, 23);
            textEstimatedTime.TabIndex = 16;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(16, 91);
            label4.Name = "label4";
            label4.Size = new Size(89, 15);
            label4.TabIndex = 6;
            label4.Text = "Estimated time:";
            // 
            // TaskEditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(631, 496);
            Controls.Add(tabTaskData);
            Controls.Add(buttonCancel);
            Controls.Add(buttonAccept);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TaskEditorForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Task editor";
            FormClosing += TaskEditorForm_FormClosing;
            Shown += TaskEditorForm_Shown;
            KeyPress += TaskEditorForm_KeyPress;
            KeyUp += TaskEditorForm_KeyUp;
            tabTaskData.ResumeLayout(false);
            tabPageBasicData.ResumeLayout(false);
            tabPageBasicData.PerformLayout();
            tabPageMoreDetails.ResumeLayout(false);
            tabPageMoreDetails.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private TabControl tabTaskData;
        private TabPage tabPageBasicData;
        private Label label9;
        private TextBox textTags;
        private Label label2;
        private TextBox textDescription;
        private TextBox textUser;
        private Label label11;
        private Label label1;
        private TabPage tabPageMoreDetails;
        private TextBox textPriority;
        private Label label3;
        private CheckBox checkResolved;
        private Button buttonSelDateEnd;
        private TextBox textEndDate;
        private Label label10;
        private Button buttonSelDateStart;
        private TextBox textStartDate;
        private Button buttonSelDateExE;
        private Button buttonSelDateExS;
        private TextBox textDificultyLevel;
        private TextBox textSpendTime;
        private TextBox textExEndDate;
        private TextBox textExStartDate;
        private Label label8;
        private Label label7;
        private Label label6;
        private Label label5;
        private TextBox textEstimatedTime;
        private Label label4;
    }
}