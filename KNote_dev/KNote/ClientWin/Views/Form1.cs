using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Windows.Forms;

namespace KNote.ClientWin.Views
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hello");
        }

        public Control p1
        {
            get {
                TopLevel = false;
                Dock = DockStyle.Fill;
                FormBorderStyle = FormBorderStyle.None;
                panel1.Dock = DockStyle.Fill;
                return panel1; 
            }
        }
    }
}
