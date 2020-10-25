using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace KNote.ClientWin.Views
{
    public partial class MyAppDummyForm : Form
    {
        private DocumentDummy _document;

        public MyAppDummyForm(DocumentDummy document)
        {
            InitializeComponent();

            if (document != null)
                this._document = document;
            else
                this._document = new DocumentDummy();
        }

        private void MyAppDummyForm_Load(object sender, EventArgs e)
        {
            label1.Text = _document.Topic;
            label2.Text = _document.Description;
            label1.Refresh();
            label2.Refresh();
        }

        public void Sleep(int sleep)
        {
            Thread.Sleep(sleep);
        }

        public void AddInfo(string info)
        {
            listBox1.Items.Add(info);
            listBox1.Refresh();
        }

        public void MovePic(int top, int left)
        {
            pictureBox1.Top = top;
            pictureBox1.Left = left;
            pictureBox1.Refresh();
        }

        public void ClearInfo()
        {
            listBox1.Items.Clear();

            this.Refresh();
        }
    }
}
