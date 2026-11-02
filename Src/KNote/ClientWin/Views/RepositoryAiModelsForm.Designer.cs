namespace KNote.ClientWin.Views
{
    partial class RepositoryAiModelsForm
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
            this.labelDescription = new System.Windows.Forms.Label();
            this.labelLlmModelInfo = new System.Windows.Forms.Label();
            this.labelEmbeddingModelInfo = new System.Windows.Forms.Label();
            this.comboLlmModel = new System.Windows.Forms.ComboBox();
            this.labelLlmModel = new System.Windows.Forms.Label();
            this.textQueryInstruction = new System.Windows.Forms.TextBox();
            this.labelQueryInstruction = new System.Windows.Forms.Label();
            this.textDimensions = new System.Windows.Forms.TextBox();
            this.labelDimensions = new System.Windows.Forms.Label();
            this.comboEmbeddingModel = new System.Windows.Forms.ComboBox();
            this.labelEmbeddingModel = new System.Windows.Forms.Label();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonManageProviders = new System.Windows.Forms.Button();
            this.panelForm.SuspendLayout();
            this.SuspendLayout();
            //
            // panelForm
            //
            this.panelForm.Controls.Add(this.buttonManageProviders);
            this.panelForm.Controls.Add(this.buttonSave);
            this.panelForm.Controls.Add(this.labelDescription);
            this.panelForm.Controls.Add(this.labelLlmModelInfo);
            this.panelForm.Controls.Add(this.labelEmbeddingModelInfo);
            this.panelForm.Controls.Add(this.comboLlmModel);
            this.panelForm.Controls.Add(this.labelLlmModel);
            this.panelForm.Controls.Add(this.textQueryInstruction);
            this.panelForm.Controls.Add(this.labelQueryInstruction);
            this.panelForm.Controls.Add(this.textDimensions);
            this.panelForm.Controls.Add(this.labelDimensions);
            this.panelForm.Controls.Add(this.comboEmbeddingModel);
            this.panelForm.Controls.Add(this.labelEmbeddingModel);
            this.panelForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelForm.Location = new System.Drawing.Point(0, 0);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(626, 440);
            this.panelForm.TabIndex = 0;
            //
            // labelDescription
            //
            this.labelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDescription.Location = new System.Drawing.Point(9, 12);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(608, 48);
            this.labelDescription.TabIndex = 10;
            this.labelDescription.Text = "The models everyone working with this repository uses: the embedding model of the search by meaning (all its notes are indexed with it) and the LLM recommended for its AI tasks. Where each model runs is set in each installation (Tools > Manage AI providers).";
            //
            // labelEmbeddingModel
            //
            this.labelEmbeddingModel.AutoSize = true;
            this.labelEmbeddingModel.Location = new System.Drawing.Point(9, 69);
            this.labelEmbeddingModel.Name = "labelEmbeddingModel";
            this.labelEmbeddingModel.Size = new System.Drawing.Size(271, 15);
            this.labelEmbeddingModel.TabIndex = 11;
            this.labelEmbeddingModel.Text = "Embedding model (empty: no search by meaning):";
            //
            // comboEmbeddingModel
            //
            this.comboEmbeddingModel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboEmbeddingModel.FormattingEnabled = true;
            this.comboEmbeddingModel.Location = new System.Drawing.Point(9, 89);
            this.comboEmbeddingModel.Name = "comboEmbeddingModel";
            this.comboEmbeddingModel.Size = new System.Drawing.Size(608, 23);
            this.comboEmbeddingModel.TabIndex = 0;
            this.comboEmbeddingModel.TextChanged += new System.EventHandler(this.comboEmbeddingModel_TextChanged);
            //
            // labelEmbeddingModelInfo
            //
            this.labelEmbeddingModelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelEmbeddingModelInfo.Location = new System.Drawing.Point(9, 116);
            this.labelEmbeddingModelInfo.Name = "labelEmbeddingModelInfo";
            this.labelEmbeddingModelInfo.Size = new System.Drawing.Size(608, 34);
            this.labelEmbeddingModelInfo.TabIndex = 16;
            //
            // labelDimensions
            //
            this.labelDimensions.AutoSize = true;
            this.labelDimensions.Location = new System.Drawing.Point(9, 155);
            this.labelDimensions.Name = "labelDimensions";
            this.labelDimensions.Size = new System.Drawing.Size(307, 15);
            this.labelDimensions.TabIndex = 12;
            this.labelDimensions.Text = "Dimensions (empty: the model's own; fewer, if made for it):";
            //
            // textDimensions
            //
            this.textDimensions.Location = new System.Drawing.Point(9, 175);
            this.textDimensions.Name = "textDimensions";
            this.textDimensions.Size = new System.Drawing.Size(120, 23);
            this.textDimensions.TabIndex = 1;
            //
            // labelQueryInstruction
            //
            this.labelQueryInstruction.AutoSize = true;
            this.labelQueryInstruction.Location = new System.Drawing.Point(9, 207);
            this.labelQueryInstruction.Name = "labelQueryInstruction";
            this.labelQueryInstruction.Size = new System.Drawing.Size(352, 15);
            this.labelQueryInstruction.TabIndex = 13;
            this.labelQueryInstruction.Text = "Query instruction (models that expect one, e.g. Qwen3-Embedding):";
            //
            // textQueryInstruction
            //
            this.textQueryInstruction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textQueryInstruction.Location = new System.Drawing.Point(9, 227);
            this.textQueryInstruction.Multiline = true;
            this.textQueryInstruction.Name = "textQueryInstruction";
            this.textQueryInstruction.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textQueryInstruction.Size = new System.Drawing.Size(608, 52);
            this.textQueryInstruction.TabIndex = 2;
            //
            // labelLlmModel
            //
            this.labelLlmModel.AutoSize = true;
            this.labelLlmModel.Location = new System.Drawing.Point(9, 289);
            this.labelLlmModel.Name = "labelLlmModel";
            this.labelLlmModel.Size = new System.Drawing.Size(157, 15);
            this.labelLlmModel.TabIndex = 14;
            this.labelLlmModel.Text = "Recommended LLM (optional):";
            //
            // comboLlmModel
            //
            this.comboLlmModel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboLlmModel.FormattingEnabled = true;
            this.comboLlmModel.Location = new System.Drawing.Point(9, 309);
            this.comboLlmModel.Name = "comboLlmModel";
            this.comboLlmModel.Size = new System.Drawing.Size(608, 23);
            this.comboLlmModel.TabIndex = 3;
            this.comboLlmModel.TextChanged += new System.EventHandler(this.comboLlmModel_TextChanged);
            //
            // labelLlmModelInfo
            //
            this.labelLlmModelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLlmModelInfo.Location = new System.Drawing.Point(9, 336);
            this.labelLlmModelInfo.Name = "labelLlmModelInfo";
            this.labelLlmModelInfo.Size = new System.Drawing.Size(608, 34);
            this.labelLlmModelInfo.TabIndex = 18;
            //
            // buttonSave
            //
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(507, 403);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(110, 29);
            this.buttonSave.TabIndex = 5;
            this.buttonSave.Text = "&Save models";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            //
            // buttonManageProviders
            //
            this.buttonManageProviders.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonManageProviders.Location = new System.Drawing.Point(9, 403);
            this.buttonManageProviders.Name = "buttonManageProviders";
            this.buttonManageProviders.Size = new System.Drawing.Size(150, 29);
            this.buttonManageProviders.TabIndex = 4;
            this.buttonManageProviders.Text = "&Manage models ...";
            this.buttonManageProviders.UseVisualStyleBackColor = true;
            this.buttonManageProviders.Click += new System.EventHandler(this.buttonManageProviders_Click);
            //
            // RepositoryAiModelsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 440);
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "RepositoryAiModelsForm";
            this.Text = "Repository AI models";
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Label labelLlmModelInfo;
        private System.Windows.Forms.Label labelEmbeddingModelInfo;
        private System.Windows.Forms.ComboBox comboLlmModel;
        private System.Windows.Forms.Label labelLlmModel;
        private System.Windows.Forms.TextBox textQueryInstruction;
        private System.Windows.Forms.Label labelQueryInstruction;
        private System.Windows.Forms.TextBox textDimensions;
        private System.Windows.Forms.Label labelDimensions;
        private System.Windows.Forms.ComboBox comboEmbeddingModel;
        private System.Windows.Forms.Label labelEmbeddingModel;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonManageProviders;
    }
}
