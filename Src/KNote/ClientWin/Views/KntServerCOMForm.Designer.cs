namespace KNote.ClientWin.Views
{
    partial class KntServerCOMForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KntServerCOMForm));
            textBoxSend = new TextBox();
            buttonStart = new Button();
            buttonSend = new Button();
            listBoxEcho = new ListBox();
            buttonStop = new Button();
            label1 = new Label();
            label2 = new Label();
            statusInfo = new StatusStrip();
            statusLabelInfo = new ToolStripStatusLabel();
            panelLine = new Panel();
            tabControlMain = new TabControl();
            tabPageService = new TabPage();
            tabPageSettings = new TabPage();
            panelSettings = new Panel();
            labelSettingsInfo = new Label();
            labelPortName = new Label();
            comboPortName = new ComboBox();
            buttonRefreshPorts = new Button();
            labelBaudRate = new Label();
            comboBaudRate = new ComboBox();
            labelHandshake = new Label();
            comboHandshake = new ComboBox();
            labelRetroDelay = new Label();
            numericRetroDelay = new NumericUpDown();
            labelRetroDelayUnit = new Label();
            groupBoxFrame = new GroupBox();
            labelParity = new Label();
            comboParity = new ComboBox();
            labelDataBits = new Label();
            numericDataBits = new NumericUpDown();
            labelStopBits = new Label();
            comboStopBits = new ComboBox();
            buttonRestoreDefaults = new Button();
            buttonSaveSettings = new Button();
            labelSaveResult = new Label();
            labelAiProvider = new Label();
            comboAiProviders = new ComboBox();
            buttonManageProviders = new Button();
            statusInfo.SuspendLayout();
            tabControlMain.SuspendLayout();
            tabPageService.SuspendLayout();
            tabPageSettings.SuspendLayout();
            panelSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericRetroDelay).BeginInit();
            groupBoxFrame.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericDataBits).BeginInit();
            SuspendLayout();
            //
            // textBoxSend
            //
            textBoxSend.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxSend.Location = new Point(6, 33);
            textBoxSend.Margin = new Padding(4, 3, 4, 3);
            textBoxSend.MaxLength = 0;
            textBoxSend.Multiline = true;
            textBoxSend.Name = "textBoxSend";
            textBoxSend.ScrollBars = ScrollBars.Vertical;
            textBoxSend.Size = new Size(546, 150);
            textBoxSend.TabIndex = 1;
            //
            // buttonStart
            //
            buttonStart.Location = new Point(10, 8);
            buttonStart.Margin = new Padding(4, 3, 4, 3);
            buttonStart.Name = "buttonStart";
            buttonStart.Size = new Size(112, 28);
            buttonStart.TabIndex = 0;
            buttonStart.Text = "&Start service";
            buttonStart.UseVisualStyleBackColor = true;
            buttonStart.Click += buttonStart_Click;
            //
            // buttonSend
            //
            buttonSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSend.Location = new Point(363, 4);
            buttonSend.Margin = new Padding(4, 3, 4, 3);
            buttonSend.Name = "buttonSend";
            buttonSend.Size = new Size(189, 24);
            buttonSend.TabIndex = 0;
            buttonSend.Text = "Send text &to client";
            buttonSend.UseVisualStyleBackColor = true;
            buttonSend.Click += buttonSend_Click;
            //
            // listBoxEcho
            //
            listBoxEcho.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listBoxEcho.FormattingEnabled = true;
            listBoxEcho.ItemHeight = 15;
            listBoxEcho.Location = new Point(6, 212);
            listBoxEcho.Margin = new Padding(4, 3, 4, 3);
            listBoxEcho.Name = "listBoxEcho";
            listBoxEcho.Size = new Size(546, 124);
            listBoxEcho.TabIndex = 3;
            //
            // buttonStop
            //
            buttonStop.Location = new Point(130, 8);
            buttonStop.Margin = new Padding(4, 3, 4, 3);
            buttonStop.Name = "buttonStop";
            buttonStop.Size = new Size(112, 28);
            buttonStop.TabIndex = 1;
            buttonStop.Text = "Sto&p service";
            buttonStop.UseVisualStyleBackColor = true;
            buttonStop.Click += buttonStop_Click;
            //
            // label1
            //
            label1.AutoSize = true;
            label1.Location = new Point(8, 194);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(97, 15);
            label1.TabIndex = 2;
            label1.Text = "Echo from client:";
            //
            // label2
            //
            label2.AutoSize = true;
            label2.Location = new Point(6, 9);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(81, 15);
            label2.TabIndex = 4;
            label2.Text = "Text for client:";
            //
            // statusInfo
            //
            statusInfo.Items.AddRange(new ToolStripItem[] { statusLabelInfo });
            statusInfo.Location = new Point(0, 439);
            statusInfo.Name = "statusInfo";
            statusInfo.Padding = new Padding(1, 0, 16, 0);
            statusInfo.Size = new Size(584, 22);
            statusInfo.TabIndex = 3;
            statusInfo.Text = "statusStrip1";
            //
            // statusLabelInfo
            //
            statusLabelInfo.Name = "statusLabelInfo";
            statusLabelInfo.Size = new Size(16, 17);
            statusLabelInfo.Text = "...";
            //
            // panelLine
            //
            panelLine.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelLine.BackColor = Color.DarkGray;
            panelLine.ForeColor = Color.Gray;
            panelLine.Location = new Point(11, 42);
            panelLine.Name = "panelLine";
            panelLine.Size = new Size(561, 2);
            panelLine.TabIndex = 2;
            //
            // tabControlMain
            //
            tabControlMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControlMain.Controls.Add(tabPageService);
            tabControlMain.Controls.Add(tabPageSettings);
            tabControlMain.Location = new Point(9, 50);
            tabControlMain.Name = "tabControlMain";
            tabControlMain.SelectedIndex = 0;
            tabControlMain.Size = new Size(566, 383);
            tabControlMain.TabIndex = 4;
            //
            // tabPageService
            //
            tabPageService.Controls.Add(label2);
            tabPageService.Controls.Add(buttonSend);
            tabPageService.Controls.Add(textBoxSend);
            tabPageService.Controls.Add(label1);
            tabPageService.Controls.Add(listBoxEcho);
            tabPageService.Location = new Point(4, 24);
            tabPageService.Name = "tabPageService";
            tabPageService.Padding = new Padding(3);
            tabPageService.Size = new Size(558, 355);
            tabPageService.TabIndex = 0;
            tabPageService.Text = "Service";
            tabPageService.UseVisualStyleBackColor = true;
            //
            // tabPageSettings
            //
            tabPageSettings.Controls.Add(panelSettings);
            tabPageSettings.Controls.Add(labelSettingsInfo);
            tabPageSettings.Location = new Point(4, 24);
            tabPageSettings.Name = "tabPageSettings";
            tabPageSettings.Padding = new Padding(3);
            tabPageSettings.Size = new Size(558, 355);
            tabPageSettings.TabIndex = 1;
            tabPageSettings.Text = "Settings";
            tabPageSettings.UseVisualStyleBackColor = true;
            //
            // panelSettings
            //
            panelSettings.Controls.Add(labelPortName);
            panelSettings.Controls.Add(comboPortName);
            panelSettings.Controls.Add(buttonRefreshPorts);
            panelSettings.Controls.Add(labelBaudRate);
            panelSettings.Controls.Add(comboBaudRate);
            panelSettings.Controls.Add(labelHandshake);
            panelSettings.Controls.Add(comboHandshake);
            panelSettings.Controls.Add(labelRetroDelay);
            panelSettings.Controls.Add(numericRetroDelay);
            panelSettings.Controls.Add(labelRetroDelayUnit);
            panelSettings.Controls.Add(groupBoxFrame);
            panelSettings.Controls.Add(buttonSaveSettings);
            panelSettings.Controls.Add(labelSaveResult);
            panelSettings.Dock = DockStyle.Fill;
            panelSettings.Location = new Point(3, 3);
            panelSettings.Name = "panelSettings";
            panelSettings.Size = new Size(552, 313);
            panelSettings.TabIndex = 0;
            //
            // labelSettingsInfo
            //
            labelSettingsInfo.Dock = DockStyle.Bottom;
            labelSettingsInfo.Location = new Point(3, 316);
            labelSettingsInfo.Name = "labelSettingsInfo";
            labelSettingsInfo.Padding = new Padding(6, 0, 0, 0);
            labelSettingsInfo.Size = new Size(552, 36);
            labelSettingsInfo.TabIndex = 1;
            labelSettingsInfo.Text = "...";
            labelSettingsInfo.TextAlign = ContentAlignment.MiddleLeft;
            //
            // labelPortName
            //
            labelPortName.AutoSize = true;
            labelPortName.Location = new Point(12, 17);
            labelPortName.Name = "labelPortName";
            labelPortName.Size = new Size(64, 15);
            labelPortName.TabIndex = 0;
            labelPortName.Text = "Port name:";
            //
            // comboPortName
            //
            comboPortName.Location = new Point(130, 14);
            comboPortName.Name = "comboPortName";
            comboPortName.Size = new Size(140, 23);
            comboPortName.TabIndex = 1;
            //
            // buttonRefreshPorts
            //
            buttonRefreshPorts.Location = new Point(278, 13);
            buttonRefreshPorts.Name = "buttonRefreshPorts";
            buttonRefreshPorts.Size = new Size(96, 25);
            buttonRefreshPorts.TabIndex = 2;
            buttonRefreshPorts.Text = "Refresh ports";
            buttonRefreshPorts.UseVisualStyleBackColor = true;
            buttonRefreshPorts.Click += buttonRefreshPorts_Click;
            //
            // labelBaudRate
            //
            labelBaudRate.AutoSize = true;
            labelBaudRate.Location = new Point(12, 49);
            labelBaudRate.Name = "labelBaudRate";
            labelBaudRate.Size = new Size(63, 15);
            labelBaudRate.TabIndex = 3;
            labelBaudRate.Text = "Baud rate:";
            //
            // comboBaudRate
            //
            comboBaudRate.Location = new Point(130, 46);
            comboBaudRate.Name = "comboBaudRate";
            comboBaudRate.Size = new Size(140, 23);
            comboBaudRate.TabIndex = 4;
            //
            // labelHandshake
            //
            labelHandshake.AutoSize = true;
            labelHandshake.Location = new Point(12, 81);
            labelHandshake.Name = "labelHandshake";
            labelHandshake.Size = new Size(66, 15);
            labelHandshake.TabIndex = 5;
            labelHandshake.Text = "Handshake:";
            //
            // comboHandshake
            //
            comboHandshake.DropDownStyle = ComboBoxStyle.DropDownList;
            comboHandshake.Location = new Point(130, 78);
            comboHandshake.Name = "comboHandshake";
            comboHandshake.Size = new Size(200, 23);
            comboHandshake.TabIndex = 6;
            //
            // labelRetroDelay
            //
            labelRetroDelay.AutoSize = true;
            labelRetroDelay.Location = new Point(12, 113);
            labelRetroDelay.Name = "labelRetroDelay";
            labelRetroDelay.Size = new Size(72, 15);
            labelRetroDelay.TabIndex = 7;
            labelRetroDelay.Text = "Retro delay:";
            //
            // numericRetroDelay
            //
            numericRetroDelay.Location = new Point(130, 110);
            numericRetroDelay.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            numericRetroDelay.Name = "numericRetroDelay";
            numericRetroDelay.Size = new Size(80, 23);
            numericRetroDelay.TabIndex = 8;
            numericRetroDelay.Value = new decimal(new int[] { 60, 0, 0, 0 });
            //
            // labelRetroDelayUnit
            //
            labelRetroDelayUnit.AutoSize = true;
            labelRetroDelayUnit.Location = new Point(216, 113);
            labelRetroDelayUnit.Name = "labelRetroDelayUnit";
            labelRetroDelayUnit.Size = new Size(158, 15);
            labelRetroDelayUnit.TabIndex = 9;
            labelRetroDelayUnit.Text = "ms between RS-232 packets";
            //
            // groupBoxFrame
            //
            groupBoxFrame.Controls.Add(labelParity);
            groupBoxFrame.Controls.Add(comboParity);
            groupBoxFrame.Controls.Add(labelDataBits);
            groupBoxFrame.Controls.Add(numericDataBits);
            groupBoxFrame.Controls.Add(labelStopBits);
            groupBoxFrame.Controls.Add(comboStopBits);
            groupBoxFrame.Controls.Add(buttonRestoreDefaults);
            groupBoxFrame.Location = new Point(12, 149);
            groupBoxFrame.Name = "groupBoxFrame";
            groupBoxFrame.Size = new Size(430, 118);
            groupBoxFrame.TabIndex = 10;
            groupBoxFrame.TabStop = false;
            groupBoxFrame.Text = "Frame parameters";
            //
            // labelParity
            //
            labelParity.AutoSize = true;
            labelParity.Location = new Point(12, 28);
            labelParity.Name = "labelParity";
            labelParity.Size = new Size(41, 15);
            labelParity.TabIndex = 0;
            labelParity.Text = "Parity:";
            //
            // comboParity
            //
            comboParity.DropDownStyle = ComboBoxStyle.DropDownList;
            comboParity.Location = new Point(118, 25);
            comboParity.Name = "comboParity";
            comboParity.Size = new Size(140, 23);
            comboParity.TabIndex = 1;
            //
            // labelDataBits
            //
            labelDataBits.AutoSize = true;
            labelDataBits.Location = new Point(12, 60);
            labelDataBits.Name = "labelDataBits";
            labelDataBits.Size = new Size(58, 15);
            labelDataBits.TabIndex = 2;
            labelDataBits.Text = "Data bits:";
            //
            // numericDataBits
            //
            numericDataBits.Location = new Point(118, 57);
            numericDataBits.Maximum = new decimal(new int[] { 8, 0, 0, 0 });
            numericDataBits.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            numericDataBits.Name = "numericDataBits";
            numericDataBits.Size = new Size(60, 23);
            numericDataBits.TabIndex = 3;
            numericDataBits.Value = new decimal(new int[] { 8, 0, 0, 0 });
            //
            // labelStopBits
            //
            labelStopBits.AutoSize = true;
            labelStopBits.Location = new Point(12, 92);
            labelStopBits.Name = "labelStopBits";
            labelStopBits.Size = new Size(58, 15);
            labelStopBits.TabIndex = 4;
            labelStopBits.Text = "Stop bits:";
            //
            // comboStopBits
            //
            comboStopBits.DropDownStyle = ComboBoxStyle.DropDownList;
            comboStopBits.Location = new Point(118, 89);
            comboStopBits.Name = "comboStopBits";
            comboStopBits.Size = new Size(140, 23);
            comboStopBits.TabIndex = 5;
            //
            // buttonRestoreDefaults
            //
            buttonRestoreDefaults.Location = new Point(280, 25);
            buttonRestoreDefaults.Name = "buttonRestoreDefaults";
            buttonRestoreDefaults.Size = new Size(138, 25);
            buttonRestoreDefaults.TabIndex = 6;
            buttonRestoreDefaults.Text = "Restore defaults";
            buttonRestoreDefaults.UseVisualStyleBackColor = true;
            buttonRestoreDefaults.Click += buttonRestoreDefaults_Click;
            //
            // buttonSaveSettings
            //
            buttonSaveSettings.Location = new Point(12, 281);
            buttonSaveSettings.Name = "buttonSaveSettings";
            buttonSaveSettings.Size = new Size(120, 26);
            buttonSaveSettings.TabIndex = 11;
            buttonSaveSettings.Text = "Save settings";
            buttonSaveSettings.UseVisualStyleBackColor = true;
            buttonSaveSettings.Click += buttonSaveSettings_Click;
            //
            // labelSaveResult
            //
            labelSaveResult.AutoSize = true;
            labelSaveResult.Location = new Point(142, 287);
            labelSaveResult.Name = "labelSaveResult";
            labelSaveResult.Size = new Size(0, 15);
            labelSaveResult.TabIndex = 12;
            //
            // labelAiProvider
            //
            labelAiProvider.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelAiProvider.AutoSize = true;
            labelAiProvider.Location = new Point(253, 15);
            labelAiProvider.Name = "labelAiProvider";
            labelAiProvider.Size = new Size(57, 15);
            labelAiProvider.TabIndex = 5;
            labelAiProvider.Text = "AI model:";
            //
            // comboAiProviders
            //
            comboAiProviders.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            comboAiProviders.DropDownStyle = ComboBoxStyle.DropDownList;
            comboAiProviders.FormattingEnabled = true;
            comboAiProviders.Location = new Point(316, 12);
            comboAiProviders.Name = "comboAiProviders";
            comboAiProviders.Size = new Size(184, 23);
            comboAiProviders.TabIndex = 6;
            comboAiProviders.SelectedIndexChanged += comboAiProviders_SelectedIndexChanged;
            //
            // buttonManageProviders
            //
            buttonManageProviders.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonManageProviders.Location = new Point(506, 8);
            buttonManageProviders.Name = "buttonManageProviders";
            buttonManageProviders.Size = new Size(68, 28);
            buttonManageProviders.TabIndex = 7;
            buttonManageProviders.Text = "&Manage...";
            buttonManageProviders.UseVisualStyleBackColor = true;
            buttonManageProviders.Click += buttonManageProviders_Click;
            //
            // KntServerCOMForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(584, 461);
            Controls.Add(tabControlMain);
            Controls.Add(buttonManageProviders);
            Controls.Add(comboAiProviders);
            Controls.Add(labelAiProvider);
            Controls.Add(panelLine);
            Controls.Add(statusInfo);
            Controls.Add(buttonStop);
            Controls.Add(buttonStart);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            MinimumSize = new Size(600, 500);
            Name = "KntServerCOMForm";
            Text = "KNote ServerCOM";
            Load += KntServerCOMForm_Load;
            statusInfo.ResumeLayout(false);
            statusInfo.PerformLayout();
            tabControlMain.ResumeLayout(false);
            tabPageService.ResumeLayout(false);
            tabPageService.PerformLayout();
            tabPageSettings.ResumeLayout(false);
            panelSettings.ResumeLayout(false);
            panelSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericRetroDelay).EndInit();
            groupBoxFrame.ResumeLayout(false);
            groupBoxFrame.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericDataBits).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBoxSend;
        private Button buttonStart;
        private Button buttonSend;
        private ListBox listBoxEcho;
        private Button buttonStop;
        private Label label1;
        private Label label2;
        private StatusStrip statusInfo;
        private ToolStripStatusLabel statusLabelInfo;
        private Panel panelLine;
        private TabControl tabControlMain;
        private TabPage tabPageService;
        private TabPage tabPageSettings;
        private Panel panelSettings;
        private Label labelSettingsInfo;
        private Label labelPortName;
        private ComboBox comboPortName;
        private Button buttonRefreshPorts;
        private Label labelBaudRate;
        private ComboBox comboBaudRate;
        private Label labelHandshake;
        private ComboBox comboHandshake;
        private Label labelRetroDelay;
        private NumericUpDown numericRetroDelay;
        private Label labelRetroDelayUnit;
        private GroupBox groupBoxFrame;
        private Label labelParity;
        private ComboBox comboParity;
        private Label labelDataBits;
        private NumericUpDown numericDataBits;
        private Label labelStopBits;
        private ComboBox comboStopBits;
        private Button buttonRestoreDefaults;
        private Button buttonSaveSettings;
        private Label labelSaveResult;
        private Label labelAiProvider;
        private ComboBox comboAiProviders;
        private Button buttonManageProviders;
    }
}
