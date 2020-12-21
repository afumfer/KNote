
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.panelForm = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.checkResolved = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.textUser = new System.Windows.Forms.TextBox();
            this.textTags = new System.Windows.Forms.TextBox();
            this.textPriority = new System.Windows.Forms.TextBox();
            this.textExStartDate = new System.Windows.Forms.TextBox();
            this.textEstimatedTime = new System.Windows.Forms.TextBox();
            this.textExEndDate = new System.Windows.Forms.TextBox();
            this.textSpendTime = new System.Windows.Forms.TextBox();
            this.textDificultyLevel = new System.Windows.Forms.TextBox();
            this.textStartDate = new System.Windows.Forms.TextBox();
            this.textEndDate = new System.Windows.Forms.TextBox();
            this.textDescription = new System.Windows.Forms.TextBox();
            this.panelForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(525, 425);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(81, 29);
            this.buttonCancel.TabIndex = 14;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonAccept
            // 
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(438, 425);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(81, 29);
            this.buttonAccept.TabIndex = 13;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // panelForm
            // 
            this.panelForm.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelForm.Controls.Add(this.textDescription);
            this.panelForm.Controls.Add(this.textEndDate);
            this.panelForm.Controls.Add(this.textStartDate);
            this.panelForm.Controls.Add(this.textDificultyLevel);
            this.panelForm.Controls.Add(this.textSpendTime);
            this.panelForm.Controls.Add(this.textExEndDate);
            this.panelForm.Controls.Add(this.textEstimatedTime);
            this.panelForm.Controls.Add(this.textExStartDate);
            this.panelForm.Controls.Add(this.textPriority);
            this.panelForm.Controls.Add(this.textTags);
            this.panelForm.Controls.Add(this.textUser);
            this.panelForm.Controls.Add(this.label11);
            this.panelForm.Controls.Add(this.label10);
            this.panelForm.Controls.Add(this.label9);
            this.panelForm.Controls.Add(this.label8);
            this.panelForm.Controls.Add(this.label7);
            this.panelForm.Controls.Add(this.label6);
            this.panelForm.Controls.Add(this.checkResolved);
            this.panelForm.Controls.Add(this.label5);
            this.panelForm.Controls.Add(this.label4);
            this.panelForm.Controls.Add(this.label3);
            this.panelForm.Controls.Add(this.label2);
            this.panelForm.Controls.Add(this.label1);
            this.panelForm.Location = new System.Drawing.Point(6, 6);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(607, 413);
            this.panelForm.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "User:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Tags:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "Priority:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(226, 68);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 15);
            this.label4.TabIndex = 3;
            this.label4.Text = "Estimated time:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(454, 70);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 15);
            this.label5.TabIndex = 4;
            this.label5.Text = "Spend time:";
            // 
            // checkResolved
            // 
            this.checkResolved.AutoSize = true;
            this.checkResolved.Location = new System.Drawing.Point(462, 127);
            this.checkResolved.Name = "checkResolved";
            this.checkResolved.Size = new System.Drawing.Size(73, 19);
            this.checkResolved.TabIndex = 5;
            this.checkResolved.Text = "Resolved";
            this.checkResolved.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(454, 100);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(81, 15);
            this.label6.TabIndex = 6;
            this.label6.Text = "Dificulty level:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 97);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(110, 15);
            this.label7.TabIndex = 7;
            this.label7.Text = "Expected start date:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(226, 100);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(107, 15);
            this.label8.TabIndex = 8;
            this.label8.Text = "Expected end date:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 128);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(60, 15);
            this.label9.TabIndex = 9;
            this.label9.Text = "Start date:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(226, 128);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(56, 15);
            this.label10.TabIndex = 10;
            this.label10.Text = "End date:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 159);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(70, 15);
            this.label11.TabIndex = 11;
            this.label11.Text = "Description:";
            // 
            // textUser
            // 
            this.textUser.Enabled = false;
            this.textUser.Location = new System.Drawing.Point(117, 9);
            this.textUser.Name = "textUser";
            this.textUser.Size = new System.Drawing.Size(482, 23);
            this.textUser.TabIndex = 12;
            // 
            // textTags
            // 
            this.textTags.Location = new System.Drawing.Point(117, 38);
            this.textTags.Name = "textTags";
            this.textTags.Size = new System.Drawing.Size(482, 23);
            this.textTags.TabIndex = 13;
            // 
            // textPriority
            // 
            this.textPriority.Location = new System.Drawing.Point(117, 65);
            this.textPriority.Name = "textPriority";
            this.textPriority.Size = new System.Drawing.Size(92, 23);
            this.textPriority.TabIndex = 14;
            // 
            // textExStartDate
            // 
            this.textExStartDate.Location = new System.Drawing.Point(117, 94);
            this.textExStartDate.Name = "textExStartDate";
            this.textExStartDate.Size = new System.Drawing.Size(92, 23);
            this.textExStartDate.TabIndex = 15;
            // 
            // textEstimatedTime
            // 
            this.textEstimatedTime.Location = new System.Drawing.Point(339, 67);
            this.textEstimatedTime.Name = "textEstimatedTime";
            this.textEstimatedTime.Size = new System.Drawing.Size(92, 23);
            this.textEstimatedTime.TabIndex = 16;
            // 
            // textExEndDate
            // 
            this.textExEndDate.Location = new System.Drawing.Point(339, 96);
            this.textExEndDate.Name = "textExEndDate";
            this.textExEndDate.Size = new System.Drawing.Size(92, 23);
            this.textExEndDate.TabIndex = 17;
            // 
            // textSpendTime
            // 
            this.textSpendTime.Location = new System.Drawing.Point(541, 67);
            this.textSpendTime.Name = "textSpendTime";
            this.textSpendTime.Size = new System.Drawing.Size(58, 23);
            this.textSpendTime.TabIndex = 18;
            // 
            // textDificultyLevel
            // 
            this.textDificultyLevel.Location = new System.Drawing.Point(541, 97);
            this.textDificultyLevel.Name = "textDificultyLevel";
            this.textDificultyLevel.Size = new System.Drawing.Size(58, 23);
            this.textDificultyLevel.TabIndex = 19;
            // 
            // textStartDate
            // 
            this.textStartDate.Location = new System.Drawing.Point(117, 123);
            this.textStartDate.Name = "textStartDate";
            this.textStartDate.Size = new System.Drawing.Size(92, 23);
            this.textStartDate.TabIndex = 20;
            // 
            // textEndDate
            // 
            this.textEndDate.Location = new System.Drawing.Point(339, 125);
            this.textEndDate.Name = "textEndDate";
            this.textEndDate.Size = new System.Drawing.Size(92, 23);
            this.textEndDate.TabIndex = 21;
            // 
            // textDescription
            // 
            this.textDescription.Location = new System.Drawing.Point(6, 177);
            this.textDescription.Multiline = true;
            this.textDescription.Name = "textDescription";
            this.textDescription.Size = new System.Drawing.Size(593, 230);
            this.textDescription.TabIndex = 22;
            // 
            // TaskEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(618, 466);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TaskEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "TaskEditorForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TaskEditorForm_FormClosing);
            this.Load += new System.EventHandler(this.TaskEditorForm_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TaskEditorForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TaskEditorForm_KeyUp);
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkResolved;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textDescription;
        private System.Windows.Forms.TextBox textEndDate;
        private System.Windows.Forms.TextBox textStartDate;
        private System.Windows.Forms.TextBox textDificultyLevel;
        private System.Windows.Forms.TextBox textSpendTime;
        private System.Windows.Forms.TextBox textExEndDate;
        private System.Windows.Forms.TextBox textEstimatedTime;
        private System.Windows.Forms.TextBox textExStartDate;
        private System.Windows.Forms.TextBox textPriority;
        private System.Windows.Forms.TextBox textTags;
        private System.Windows.Forms.TextBox textUser;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
    }
}