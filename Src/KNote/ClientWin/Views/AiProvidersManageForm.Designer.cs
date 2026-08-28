
namespace KNote.ClientWin.Views
{
    partial class AiProvidersManageForm
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
            this.listViewProviders = new System.Windows.Forms.ListView();
            this.buttonEdit = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.panelForm.SuspendLayout();
            this.SuspendLayout();
            //
            // panelForm
            //
            this.panelForm.Controls.Add(this.listViewProviders);
            this.panelForm.Controls.Add(this.buttonEdit);
            this.panelForm.Controls.Add(this.buttonDelete);
            this.panelForm.Controls.Add(this.buttonAdd);
            this.panelForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelForm.Location = new System.Drawing.Point(0, 0);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(700, 450);
            this.panelForm.TabIndex = 0;
            //
            // listViewProviders
            //
            this.listViewProviders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewProviders.HideSelection = false;
            this.listViewProviders.Location = new System.Drawing.Point(6, 40);
            this.listViewProviders.MultiSelect = false;
            this.listViewProviders.Name = "listViewProviders";
            this.listViewProviders.Size = new System.Drawing.Size(688, 404);
            this.listViewProviders.TabIndex = 3;
            this.listViewProviders.UseCompatibleStateImageBehavior = false;
            this.listViewProviders.DoubleClick += new System.EventHandler(this.listViewProviders_DoubleClick);
            this.listViewProviders.Resize += new System.EventHandler(this.listViewProviders_Resize);
            //
            // buttonEdit
            //
            this.buttonEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEdit.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonEdit.Location = new System.Drawing.Point(655, 8);
            this.buttonEdit.Name = "buttonEdit";
            this.buttonEdit.Size = new System.Drawing.Size(27, 26);
            this.buttonEdit.TabIndex = 2;
            this.buttonEdit.Text = "...";
            this.buttonEdit.UseVisualStyleBackColor = true;
            this.buttonEdit.Click += new System.EventHandler(this.buttonEdit_Click);
            //
            // buttonDelete
            //
            this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDelete.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonDelete.Location = new System.Drawing.Point(625, 8);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(27, 26);
            this.buttonDelete.TabIndex = 1;
            this.buttonDelete.Text = "-";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            //
            // buttonAdd
            //
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonAdd.Location = new System.Drawing.Point(595, 8);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(27, 26);
            this.buttonAdd.TabIndex = 0;
            this.buttonAdd.Text = "+";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            //
            // AiProvidersManageForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 450);
            this.Controls.Add(this.panelForm);
            this.Name = "AiProvidersManageForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Manage AI providers";
            this.panelForm.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.ListView listViewProviders;
        private System.Windows.Forms.Button buttonEdit;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonAdd;
    }
}
