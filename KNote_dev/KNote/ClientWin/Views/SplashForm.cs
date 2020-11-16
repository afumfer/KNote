using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using KNote.ClientWin.Core;

namespace KNote.ClientWin.Views
{
    public partial class SplashForm : Form
    {

        public SplashForm(Store appContext)
        {
            InitializeComponent();

            appContext.AddedServiceRef += AppContext_AddedServiceRef; 

        }

        private void AppContext_AddedServiceRef(object sender, ComponentEventArgs<ServiceRef> e)
        {
            labelMessage.Text = "Loading " + e.Entity.Alias + "...";
            labelMessage.Refresh();
            Application.DoEvents();
        }

        private void SplashForm_Load(object sender, EventArgs e)
        {
            labelVersion.Text = "Version: " + Application.ProductVersion.ToString();
        }
    }
}
