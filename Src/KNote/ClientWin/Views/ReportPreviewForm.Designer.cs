namespace KNote.ClientWin.Views
{
    partial class ReportPreviewForm
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
            toolBarReport = new ToolStrip();
            buttonPrint = new ToolStripButton();
            buttonSavePdf = new ToolStripButton();
            toolStripS1 = new ToolStripSeparator();
            buttonZoomOut = new ToolStripButton();
            labelZoom = new ToolStripLabel();
            buttonZoomIn = new ToolStripButton();
            buttonClose = new ToolStripButton();
            webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            statusBarReport = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            toolBarReport.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView).BeginInit();
            statusBarReport.SuspendLayout();
            SuspendLayout();
            //
            // toolBarReport
            //
            toolBarReport.GripStyle = ToolStripGripStyle.Hidden;
            toolBarReport.ImageScalingSize = new Size(20, 20);
            toolBarReport.Items.AddRange(new ToolStripItem[] { buttonPrint, buttonSavePdf, toolStripS1, buttonZoomOut, labelZoom, buttonZoomIn, buttonClose });
            toolBarReport.Location = new Point(0, 0);
            toolBarReport.Name = "toolBarReport";
            toolBarReport.Padding = new Padding(4, 2, 4, 2);
            toolBarReport.RenderMode = ToolStripRenderMode.Professional;
            toolBarReport.Size = new Size(1084, 29);
            toolBarReport.TabIndex = 0;
            //
            // buttonPrint
            //
            buttonPrint.ImageScaling = ToolStripItemImageScaling.None;
            buttonPrint.Name = "buttonPrint";
            buttonPrint.Size = new Size(56, 22);
            buttonPrint.Text = "Print ...";
            buttonPrint.ToolTipText = "Print the report (the print dialog also lets you choose \"Save as PDF\")";
            buttonPrint.Click += buttonToolBar_Click;
            //
            // buttonSavePdf
            //
            buttonSavePdf.ImageScaling = ToolStripItemImageScaling.None;
            buttonSavePdf.Name = "buttonSavePdf";
            buttonSavePdf.Size = new Size(87, 22);
            buttonSavePdf.Text = "Save as PDF ...";
            buttonSavePdf.ToolTipText = "Save the report as a PDF file";
            buttonSavePdf.Click += buttonToolBar_Click;
            //
            // toolStripS1
            //
            toolStripS1.Name = "toolStripS1";
            toolStripS1.Size = new Size(6, 25);
            //
            // buttonZoomOut
            //
            buttonZoomOut.DisplayStyle = ToolStripItemDisplayStyle.Image;
            buttonZoomOut.ImageScaling = ToolStripItemImageScaling.None;
            buttonZoomOut.Name = "buttonZoomOut";
            buttonZoomOut.Size = new Size(23, 22);
            buttonZoomOut.Text = "Zoom out";
            buttonZoomOut.Click += buttonToolBar_Click;
            //
            // labelZoom
            //
            labelZoom.AutoSize = false;
            labelZoom.Name = "labelZoom";
            labelZoom.Size = new Size(44, 22);
            labelZoom.Text = "100 %";
            //
            // buttonZoomIn
            //
            buttonZoomIn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            buttonZoomIn.ImageScaling = ToolStripItemImageScaling.None;
            buttonZoomIn.Name = "buttonZoomIn";
            buttonZoomIn.Size = new Size(23, 22);
            buttonZoomIn.Text = "Zoom in";
            buttonZoomIn.Click += buttonToolBar_Click;
            //
            // buttonClose
            //
            buttonClose.Alignment = ToolStripItemAlignment.Right;
            buttonClose.ImageScaling = ToolStripItemImageScaling.None;
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new Size(40, 22);
            buttonClose.Text = "Close";
            buttonClose.Click += buttonToolBar_Click;
            //
            // webView
            //
            webView.AllowExternalDrop = false;
            webView.CreationProperties = null;
            webView.DefaultBackgroundColor = Color.White;
            webView.Dock = DockStyle.Fill;
            webView.Location = new Point(0, 29);
            webView.Name = "webView";
            webView.Size = new Size(1084, 610);
            webView.TabIndex = 1;
            webView.ZoomFactor = 1D;
            //
            // statusBarReport
            //
            statusBarReport.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusBarReport.Location = new Point(0, 639);
            statusBarReport.Name = "statusBarReport";
            statusBarReport.Size = new Size(1084, 22);
            statusBarReport.TabIndex = 2;
            //
            // statusLabel
            //
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(1069, 17);
            statusLabel.Spring = true;
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // ReportPreviewForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1084, 661);
            Controls.Add(webView);
            Controls.Add(statusBarReport);
            Controls.Add(toolBarReport);
            MinimumSize = new Size(500, 350);
            Name = "ReportPreviewForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Report preview";
            Shown += ReportPreviewForm_Shown;
            toolBarReport.ResumeLayout(false);
            toolBarReport.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)webView).EndInit();
            statusBarReport.ResumeLayout(false);
            statusBarReport.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolBarReport;
        private ToolStripButton buttonPrint;
        private ToolStripButton buttonSavePdf;
        private ToolStripSeparator toolStripS1;
        private ToolStripButton buttonZoomOut;
        private ToolStripLabel labelZoom;
        private ToolStripButton buttonZoomIn;
        private ToolStripButton buttonClose;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private StatusStrip statusBarReport;
        private ToolStripStatusLabel statusLabel;
    }
}
