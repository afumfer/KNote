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
    public partial class DateSelectorForm : Form
    {
        public DateSelectorForm()
        {
            InitializeComponent();
        }

        private void DateSelectorForm_Load(object sender, EventArgs e)
        {
            Size tamagnoForm = new Size(monthCalendar.Width, monthCalendar.Height);
            this.ClientSize = tamagnoForm;
        }


        private void monthCalendar_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.DialogResult = DialogResult.Cancel;
                    break;
                case Keys.Enter:
                    this.DialogResult = DialogResult.OK;
                    break;
            }
        }

        private void monthCalendar_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Y > 30)
                this.DialogResult = DialogResult.OK;
        }

        public DateTime Date
        {
            get { return monthCalendar.SelectionStart; }
            set
            {
                if ((DateTime)value < new DateTime(1900, 1, 3))
                    monthCalendar.SelectionStart = DateTime.Now;
                else
                    monthCalendar.SelectionStart = (DateTime)value;
            }
        }

    }
}
