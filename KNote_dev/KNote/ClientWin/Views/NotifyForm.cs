using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Views
{
    public partial class NotifyForm : Form, IViewBase
    {
        private readonly KNoteManagmentComponent _com;

        public NotifyForm(KNoteManagmentComponent com)
        {
            InitializeComponent();
            _com = com;
        }

        #region IManagmentView implementation
        public Control PanelView()
        {
            return null;
        }

        public void CloseView()
        {
            this.Close();
        }

        public void RefreshView()
        {
            // ...
        }

        public void ShowInfo(string info)
        {
            MessageBox.Show(info);
        }

        public void ShowView()
        {
            this.Show();            
        }

        Result<EComponentResult> IViewBase.ShowModalView()
        {
            return _com.DialogResultToComponentResult(this.ShowDialog());
        }

        public void OnClosingView()
        {
            
        }


        #endregion

        #region Timer

        private void timerKNote_Tick(object sender, EventArgs e)
        {
            // TODO: Pendiente implementar
            //ContadorSegundosAlarma += 1;
            //if (ContadorSegundosAlarma >= Ctrl.Contexto.RastreoAlarmaMinutos)
            //{
            //    try
            //    {
            //        Ctrl.MostrarNotasPostItTimer();
            //    }
            //    catch (Exception miEx)
            //    {
            //        MessageBox.Show("Ha ocurrido el siguiente error:" + miEx.Message.ToString());
            //    }
            //    finally
            //    {
            //        ContadorSegundosAlarma = 0;
            //    }
            //}
        }

        #endregion

        #region Menu events handlers 

        private void menuNewNote_Click(object sender, EventArgs e)
        {
            MessageBox.Show("pendiente de implementar");
        }

        private void menuShowKNoteManagment_Click(object sender, EventArgs e)
        {
            MessageBox.Show("pendiente de implementar");
        }

        private void menuPostItsVisibles_Click(object sender, EventArgs e)
        {
            MessageBox.Show("pendiente de implementar");
        }

        private void menuKNoteOptions_Click(object sender, EventArgs e)
        {
            MessageBox.Show("pendiente de implementar");
        }

        private void menuHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("pendiente de implementar");
        }

        private void menuAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("pendiente de implementar");
        }

        private void menuExit_Click(object sender, EventArgs e)
        {            
            _com?.Finalize();
        }

        #endregion
    }
}
