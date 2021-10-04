using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KNote.ClientWin.Views
{
    public partial class KNoteAboutForm : Form, IViewBase
    {
        private readonly KNoteManagmentComponent _com;

        public KNoteAboutForm(KNoteManagmentComponent com)
        {
            InitializeComponent();
            _com = com;
        }

        #region IViewBase implementation

        public void ShowView()
        {
            this.Show();
        }

        public Result<EComponentResult> ShowModalView()
        {
            return _com.DialogResultToComponentResult(this.ShowDialog());
        }

        public DialogResult ShowInfo(string info, string caption = "KaNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Asterisk)
        {
            return MessageBox.Show(info, caption, buttons, icon);
        }

        public void OnClosingView()
        {

        }

        #endregion 

        #region Form events handlers 

        private void KNoteAboutForm_Load(object sender, EventArgs e)
        {
            var info = @"Permission is hereby granted, free of charge, to any person obtaining a copy of this ";
            info += "software and associated documentation files (the 'Software'), to deal in the Software without ";
            info += "restriction, including without limitation the rights to use and copy."; 
            info += Environment.NewLine + Environment.NewLine;
            info += "THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, ";
            info += "INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR ";
            info += "PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE ";
            info += "FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ";
            info += "ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE."; ;

            labelRepository.Text = @"https://github.com/afumfer/knote";
            labelVersion.Text = $"Version: {_com.Store.AppVersion}";
            labelInfo.Text = info;
        }

        #endregion

        private void buttonAccept_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
