
namespace KNote.ClientWin.Views
{
    partial class MessageEditorForm
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
            this.checkAlarmActivated = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboNotificationType = new System.Windows.Forms.ComboBox();
            this.buttonSelectDate = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboAlarmPeriodicity = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textAlarmDateTime = new System.Windows.Forms.TextBox();
            this.textContent = new System.Windows.Forms.TextBox();
            this.textUserFullName = new System.Windows.Forms.TextBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.panelForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelForm
            // 
            this.panelForm.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelForm.Controls.Add(this.checkAlarmActivated);
            this.panelForm.Controls.Add(this.label5);
            this.panelForm.Controls.Add(this.comboNotificationType);
            this.panelForm.Controls.Add(this.buttonSelectDate);
            this.panelForm.Controls.Add(this.label4);
            this.panelForm.Controls.Add(this.label3);
            this.panelForm.Controls.Add(this.comboAlarmPeriodicity);
            this.panelForm.Controls.Add(this.label2);
            this.panelForm.Controls.Add(this.label1);
            this.panelForm.Controls.Add(this.textAlarmDateTime);
            this.panelForm.Controls.Add(this.textContent);
            this.panelForm.Controls.Add(this.textUserFullName);
            this.panelForm.Location = new System.Drawing.Point(5, 12);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(407, 325);
            this.panelForm.TabIndex = 0;
            // 
            // checkAlarmActivated
            // 
            this.checkAlarmActivated.AutoSize = true;
            this.checkAlarmActivated.Location = new System.Drawing.Point(9, 9);
            this.checkAlarmActivated.Name = "checkAlarmActivated";
            this.checkAlarmActivated.Size = new System.Drawing.Size(109, 19);
            this.checkAlarmActivated.TabIndex = 0;
            this.checkAlarmActivated.Text = "Alarm activated";
            this.checkAlarmActivated.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 129);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(99, 15);
            this.label5.TabIndex = 8;
            this.label5.Text = "Notification type:";
            // 
            // comboNotificationType
            // 
            this.comboNotificationType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboNotificationType.FormattingEnabled = true;
            this.comboNotificationType.Location = new System.Drawing.Point(127, 126);
            this.comboNotificationType.Name = "comboNotificationType";
            this.comboNotificationType.Size = new System.Drawing.Size(273, 23);
            this.comboNotificationType.TabIndex = 7;
            // 
            // buttonSelectDate
            // 
            this.buttonSelectDate.Location = new System.Drawing.Point(376, 67);
            this.buttonSelectDate.Name = "buttonSelectDate";
            this.buttonSelectDate.Size = new System.Drawing.Size(24, 24);
            this.buttonSelectDate.TabIndex = 2;
            this.buttonSelectDate.Text = "...";
            this.buttonSelectDate.UseVisualStyleBackColor = true;
            this.buttonSelectDate.Click += new System.EventHandler(this.buttonSelectDate_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 158);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 15);
            this.label4.TabIndex = 5;
            this.label4.Text = "Comment:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 100);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "Alarm periodicity:";
            // 
            // comboAlarmPeriodicity
            // 
            this.comboAlarmPeriodicity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboAlarmPeriodicity.FormattingEnabled = true;
            this.comboAlarmPeriodicity.Location = new System.Drawing.Point(127, 97);
            this.comboAlarmPeriodicity.Name = "comboAlarmPeriodicity";
            this.comboAlarmPeriodicity.Size = new System.Drawing.Size(273, 23);
            this.comboAlarmPeriodicity.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Alarm date time:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "User:";
            // 
            // textAlarmDateTime
            // 
            this.textAlarmDateTime.Location = new System.Drawing.Point(127, 67);
            this.textAlarmDateTime.Name = "textAlarmDateTime";
            this.textAlarmDateTime.Size = new System.Drawing.Size(243, 23);
            this.textAlarmDateTime.TabIndex = 1;
            // 
            // textContent
            // 
            this.textContent.Location = new System.Drawing.Point(127, 155);
            this.textContent.Multiline = true;
            this.textContent.Name = "textContent";
            this.textContent.Size = new System.Drawing.Size(273, 162);
            this.textContent.TabIndex = 4;
            // 
            // textUserFullName
            // 
            this.textUserFullName.Enabled = false;
            this.textUserFullName.Location = new System.Drawing.Point(127, 38);
            this.textUserFullName.Name = "textUserFullName";
            this.textUserFullName.Size = new System.Drawing.Size(273, 23);
            this.textUserFullName.TabIndex = 1;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(324, 343);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(81, 29);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonAccept
            // 
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(237, 343);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(81, 29);
            this.buttonAccept.TabIndex = 5;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // MessageEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(417, 384);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MessageEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Message editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MessageEditorForm_FormClosing);
            this.Load += new System.EventHandler(this.MessageEditorForm_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MessageEditorForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MessageEditorForm_KeyUp);
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.TextBox textContent;
        private System.Windows.Forms.TextBox textUserFullName;
        private System.Windows.Forms.Button buttonSelectDate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboAlarmPeriodicity;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textAlarmDateTime;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboNotificationType;
        private System.Windows.Forms.CheckBox checkAlarmActivated;
    }
}