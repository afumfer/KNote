
namespace KNote.ClientWin.Views
{
    partial class AppInfoAlarmsForm
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
            this.panelForm = new System.Windows.Forms.Panel();
            this.listViewAlarms = new System.Windows.Forms.ListView();
            this.contextMenuAlarms = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuRemoveFromList = new System.Windows.Forms.ToolStripMenuItem();
            this.panelForm.SuspendLayout();
            this.contextMenuAlarms.SuspendLayout();
            this.SuspendLayout();
            //
            // panelForm
            //
            this.panelForm.Controls.Add(this.listViewAlarms);
            this.panelForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelForm.Location = new System.Drawing.Point(0, 0);
            this.panelForm.Name = "panelForm";
            this.panelForm.Padding = new System.Windows.Forms.Padding(6);
            this.panelForm.Size = new System.Drawing.Size(700, 300);
            this.panelForm.TabIndex = 0;
            //
            // listViewAlarms
            //
            this.listViewAlarms.ContextMenuStrip = this.contextMenuAlarms;
            this.listViewAlarms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewAlarms.HideSelection = false;
            this.listViewAlarms.Location = new System.Drawing.Point(6, 6);
            this.listViewAlarms.Name = "listViewAlarms";
            this.listViewAlarms.Size = new System.Drawing.Size(688, 288);
            this.listViewAlarms.TabIndex = 0;
            this.listViewAlarms.UseCompatibleStateImageBehavior = false;
            this.listViewAlarms.DoubleClick += new System.EventHandler(this.listViewAlarms_DoubleClick);
            //
            // contextMenuAlarms
            //
            this.contextMenuAlarms.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuRemoveFromList});
            this.contextMenuAlarms.Name = "contextMenuAlarms";
            this.contextMenuAlarms.Size = new System.Drawing.Size(180, 26);
            //
            // menuRemoveFromList
            //
            this.menuRemoveFromList.Name = "menuRemoveFromList";
            this.menuRemoveFromList.Size = new System.Drawing.Size(179, 22);
            this.menuRemoveFromList.Text = "Remove from list";
            this.menuRemoveFromList.Click += new System.EventHandler(this.menuRemoveFromList_Click);
            //
            // AppInfoAlarmsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 300);
            this.Controls.Add(this.panelForm);
            this.MinimumSize = new System.Drawing.Size(450, 200);
            this.Name = "AppInfoAlarmsForm";
            this.ShowInTaskbar = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Application info alarms";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AppInfoAlarmsForm_FormClosing);
            this.Load += new System.EventHandler(this.AppInfoAlarmsForm_Load);
            this.Move += new System.EventHandler(this.AppInfoAlarmsForm_Move);
            this.Resize += new System.EventHandler(this.AppInfoAlarmsForm_Resize);
            this.ResizeEnd += new System.EventHandler(this.AppInfoAlarmsForm_ResizeEnd);
            this.panelForm.ResumeLayout(false);
            this.contextMenuAlarms.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.ListView listViewAlarms;
        private System.Windows.Forms.ContextMenuStrip contextMenuAlarms;
        private System.Windows.Forms.ToolStripMenuItem menuRemoveFromList;
    }
}
