namespace KNote.ClientWin.Views
{
    partial class ChatRoomThreadNewForm
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
            labelTopic = new Label();
            textTopic = new TextBox();
            labelHelp = new Label();
            buttonAccept = new Button();
            buttonCancel = new Button();
            SuspendLayout();
            //
            // labelTopic
            //
            labelTopic.AutoSize = true;
            labelTopic.Location = new Point(12, 16);
            labelTopic.Name = "labelTopic";
            labelTopic.Size = new Size(39, 15);
            labelTopic.TabIndex = 0;
            labelTopic.Text = "Topic:";
            //
            // textTopic
            //
            textTopic.Location = new Point(64, 12);
            textTopic.MaxLength = 120;
            textTopic.Name = "textTopic";
            textTopic.Size = new Size(380, 23);
            textTopic.TabIndex = 1;
            //
            // labelHelp
            //
            labelHelp.AutoSize = true;
            labelHelp.ForeColor = SystemColors.GrayText;
            labelHelp.Location = new Point(64, 40);
            labelHelp.Name = "labelHelp";
            labelHelp.Size = new Size(300, 15);
            labelHelp.TabIndex = 2;
            labelHelp.Text = "A new conversation, open to every Staff user of the server.";
            //
            // buttonAccept
            //
            buttonAccept.Location = new Point(286, 70);
            buttonAccept.Name = "buttonAccept";
            buttonAccept.Size = new Size(76, 29);
            buttonAccept.TabIndex = 3;
            buttonAccept.Text = "&Create";
            buttonAccept.UseVisualStyleBackColor = true;
            buttonAccept.Click += buttonAccept_Click;
            //
            // buttonCancel
            //
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(368, 70);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(76, 29);
            buttonCancel.TabIndex = 4;
            buttonCancel.Text = "C&ancel";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // ChatRoomThreadNewForm
            //
            AcceptButton = buttonAccept;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(456, 111);
            Controls.Add(buttonCancel);
            Controls.Add(buttonAccept);
            Controls.Add(labelHelp);
            Controls.Add(textTopic);
            Controls.Add(labelTopic);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ChatRoomThreadNewForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "New thread";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelTopic;
        private TextBox textTopic;
        private Label labelHelp;
        private Button buttonAccept;
        private Button buttonCancel;
    }
}
