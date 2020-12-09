using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views
{
    public partial class NotesSelectorForm : Form, ISelectorView<NoteInfoDto>
    {
        private readonly NotesSelectorComponent _com;
        private bool _viewFinalized = false;
        private bool _configuredGrid = false;
        private UInt32 _countRepetition = 0;
        private bool _skipSelectionChanged = false;
        
        private BindingSource _source = new BindingSource();


        public NotesSelectorForm(NotesSelectorComponent com)
        {
            InitializeComponent();

            _com = com;
        }

        #region ISelectorView interface 

        public Control PanelView()
        {
            return panelForm;
        }

        public void ShowView()
        {
            this.Show();
        }

        Result<EComponentResult> IViewBase.ShowModalView()
        {
            return _com.DialogResultToComponentResult(this.ShowDialog());
        }

        public void RefreshView()
        {
            if (_com.ListEntities == null)
                return;
            else
            {
                _skipSelectionChanged = true;

                // !!! plan A
                //dataGridNotes.DataSource = _com.ListNotes;

                // !!! plan B
                _source.DataSource = _com.ListEntities;                
                dataGridNotes.DataSource = _source;

                _skipSelectionChanged = false;

                CoonfigureGridStd();

                if(_com.ListEntities.Count > 0)
                    GridSelectFirstElement();
            }
        }

        public void OnClosingView()
        {
            _viewFinalized = true;
            this.Close();
        }

        public void ConfigureEmbededMode()
        {
            TopLevel = false;
            Dock = DockStyle.Fill;
            FormBorderStyle = FormBorderStyle.None;
            panelBottom.Visible = false;
            panelDataGridNotes.Dock = DockStyle.Fill;
            dataGridNotes.BorderStyle = BorderStyle.None;
            dataGridNotes.Dock = DockStyle.Fill;
        }

        public void ConfigureWindowMode()
        {
            TopLevel = true;
            Dock = DockStyle.None;
            FormBorderStyle = FormBorderStyle.Sizable;
            panelBottom.Visible = true;
            StartPosition = FormStartPosition.CenterScreen;
        }

        public object SelectItem(NoteInfoDto item)
        {
            throw new NotImplementedException();
        }

        public DialogResult ShowInfo(string info, string caption = "KeyNote", MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            return MessageBox.Show(info, caption, buttons);
        }

        #region Extension methods ...

        public void AddItem(NoteInfoDto item)
        {
            // TODO: !!! Revisar esta implementación. Esto es hack para:
            //    a) refrescar el nuevo item en la lista, 
            //    b) mantener activo el último item seleccionado 
            // (Se está forzando artificialmente si item es null entonces significa que venimos de un alta)   
            // 
            _skipSelectionChanged = true;
            _source.ResetBindings(false);            
            
            // dejar seleccionado el último item.
            int index = 0;

            if(_com.SelectedEntity != null)
            {
                foreach (DataGridViewRow r in dataGridNotes.Rows)
                {
                    if (_com.SelectedEntity.NoteId == (Guid)r.Cells["NoteId"].Value)
                    {
                        index = r.Index;
                        break;
                    }
                }
            }
            _skipSelectionChanged = false;
            dataGridNotes.Rows[index].Selected = true;
        }

        public void DeleteItem(NoteInfoDto item)
        {
            // En este caso no se usa item, la actualizació se resuelve con ResetBindings
            _skipSelectionChanged = true;
            _source.ResetBindings(false);            
            
            if (_com.ListEntities.Count == 0)
                return;

            GridSelectFirstElement(false);
        }



        public void RefreshItem(NoteInfoDto item)
        {
            dataGridNotes.Refresh();
        }

        #endregion

        #endregion

        #region Form events handlers 

        private void dataGridNotes_SelectionChanged(object sender, EventArgs e)
        {
            OnSelectedNoteItemChanged();
        }

        private void dataGridNotes_DoubleClick(object sender, EventArgs e)
        {
            _com.NotifySelectedEntityDoubleClick();
        }

        private void dataGridNotes_KeyUp(object sender, KeyEventArgs e)
        {
            _countRepetition = 0;            
        }

        private void dataGridNotes_KeyDown(object sender, KeyEventArgs e)
        {
            _countRepetition++;
        }

        private void NotesSelectorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
                _com.Finalize();
        }

        private void buttonAccept_Click(object sender, EventArgs e)
        {
            _com.Accept();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _com.Cancel();
        }

        #endregion

        #region Private methods

        private void OnSelectedNoteItemChanged()
        {
            if (_skipSelectionChanged || _countRepetition > 5)
                return;
            try
            {
                if (_com.ListEntities == null)
                    return;

                if (dataGridNotes.SelectedRows.Count > 0)
                {
                    this.Cursor = Cursors.WaitCursor;
                    var sr = dataGridNotes.SelectedRows[0];
                    _com.SelectedEntity = DataGridViewRowToNoteItemList(sr);                    
                    _com.NotifySelectedEntity();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"OnSelectedNoteItemChanged error: {ex.Message}");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }

        }

        private void CoonfigureGridStd()
        {
            if (_configuredGrid)
                return;

            // TODO: arreglar esto ... (el grid de net core parece que no se comporta de la misma manera que en netframework)

            //DataGridViewCellStyle cellStyle0 = new DataGridViewCellStyle();
            //cellStyle0.Alignment = DataGridViewContentAlignment.MiddleRight;

            //DataGridViewCellStyle cellStyle1 = new DataGridViewCellStyle();
            //cellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;

            //DataGridViewCellStyle cellStyle2 = new DataGridViewCellStyle();
            //cellStyle2.DataSourceNullValue = new DateTime(1900, 1, 1);

            dataGridNotes.Columns[0].Visible = false;  // NoteId
            //dataGridNotes.Columns[0].Width = 40;
            //dataGridNotes.Columns[0].HeaderText = "Note Id";
            //dataGridNotes.Columns[0].DataPropertyName = "NoteId";

            //dataGridNotes.Columns[1].Visible = true; // NoteNumber
            dataGridNotes.Columns[1].Width = 50;
            dataGridNotes.Columns[1].HeaderText = "Number";            
            //dataGridNotes.Columns[1].DataPropertyName = "NoteNumber";
            //dataGridNotes.Columns[1].DefaultCellStyle = cellStyle0;

            dataGridNotes.Columns[2].Width = 500;  // Topic
            //dataGridNotes.Columns[2].DataPropertyName = "Topic";

            dataGridNotes.Columns[3].Width = 125;  // CreationDateTime
            dataGridNotes.Columns[3].HeaderText = "Creation date";
            //dataGridNotes.Columns[5].DataPropertyName = "CreationDateTime";
            //dataGridNotes.Columns[5].DefaultCellStyle = cellStyle2;

            dataGridNotes.Columns[4].Width = 125;  // ModificationDateTime
            dataGridNotes.Columns[4].HeaderText = "Modification date";
            //dataGridNotes.Columns[6].DataPropertyName = "ModificationDateTime";
            //dataGridNotes.Columns[6].DefaultCellStyle = cellStyle2;

            dataGridNotes.Columns[5].Visible = false;  // Description 
            dataGridNotes.Columns[6].Visible = false;  // ContentType
            dataGridNotes.Columns[7].Visible = false;  // Script
            dataGridNotes.Columns[8].Visible = false;  // InternalTags

            dataGridNotes.Columns[9].Width = 200;  // Tags
            //dataGridNotes.Columns[9].DataPropertyName = "Tags";


            dataGridNotes.Columns[10].Width = 65;  // Priority
            //dataGridNotes.Columns[3].DefaultCellStyle = cellStyle1;
            //dataGridNotes.Columns[3].DataPropertyName = "Priority";

            dataGridNotes.Columns[11].Visible = false;  // FolderId 
            dataGridNotes.Columns[12].Visible = false;  // NoteTypeId

            ////dataGridNotes.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 8, FontStyle.Regular, GraphicsUnit.Point);

            ////dataGridNotes.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.WindowText;
            ////dataGridNotes.ColumnHeadersDefaultCellStyle.SelectionBackColor = SystemColors.ControlDark;
            ////dataGridNotes.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            //dataGridNotes.DefaultCellStyle.Font = new Font("Tahoma", 8, FontStyle.Regular, GraphicsUnit.Point);
            //dataGridNotes.DefaultCellStyle.BackColor = Color.Empty;

            //dataGridNotes.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.ControlLight;
            //dataGridNotes.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            //dataGridNotes.GridColor = SystemColors.ControlDark;

            _configuredGrid = true;
        }

        private NoteInfoDto DataGridViewRowToNoteItemList(DataGridViewRow dgr)
        {
            var n = new NoteInfoDto();

            if (dgr == null)
                return null;
            else
            {
                // TODO: valorar si buscar el item en la colección ListNotes
                //return new NoteInfoDto
                //{
                //    NoteId = (Guid)dgr.Cells[0].Value,
                //    NoteNumber = (int)dgr.Cells[1].Value,
                //    Topic = (string)dgr.Cells[2].Value,
                //    Priority = (int)dgr.Cells[3].Value,
                //    Tags = (string)dgr.Cells[4].Value,
                //    CreationDateTime = (DateTime)dgr.Cells[5].Value,
                //    ModificationDateTime = (DateTime)dgr.Cells[6].Value,
                //    FolderId = (Guid)dgr.Cells[7].Value                    
                //};


                //n.NoteId = (Guid)dgr.Cells[0].Value;
                //n.NoteNumber = (int)dgr.Cells[1].Value;
                //n.Topic = (string)dgr.Cells[2].Value;
                //n.Priority = (int)dgr.Cells[3].Value;
                //n.Tags = (string)dgr.Cells[4].Value;
                //n.CreationDateTime = (DateTime)dgr.Cells[5].Value;
                //n.ModificationDateTime = (DateTime)dgr.Cells[6].Value;
                //n.FolderId = (Guid)dgr.Cells[7].Value;

                n.NoteId = (Guid)dgr.Cells[0].Value;
                n.NoteNumber = (int)dgr.Cells[1].Value;
                n.Topic = (string)dgr.Cells[2].Value;
                n.CreationDateTime = (DateTime)dgr.Cells[3].Value;
                n.ModificationDateTime = (DateTime)dgr.Cells[4].Value;
                n.Description = (string)dgr.Cells[5].Value;
                n.ContentType = (string)dgr.Cells[6].Value;
                n.Script = (string)dgr.Cells[7].Value;
                n.InternalTags = (string)dgr.Cells[8].Value;
                n.Tags = (string)dgr.Cells[9].Value;
                n.Priority = (int)dgr.Cells[10].Value;
                n.FolderId = (Guid)dgr.Cells[11].Value;
                if (dgr.Cells[12].Value != null)
                    n.NoteTypeId = (Guid)dgr.Cells[12].Value;
                else
                    n.NoteTypeId = null;


                //n.ContentType = (string)dgr.Cells[5].Value;
                //n.Script = (string)dgr.Cells[6].Value;
                //n.InternalTags = (string)dgr.Cells[7].Value;
                //n.Tags = (string)dgr.Cells[8].Value;
                //n.Priority = (int)dgr.Cells[9].Value;
                //n.FolderId = (Guid)dgr.Cells[10].Value;
                //if (dgr.Cells[12].Value != null)
                //    n.NoteTypeId = (Guid)dgr.Cells[11].Value;
                //else
                //    n.NoteTypeId = null;

                return n;
            }
        }

        private void GridSelectFirstElement(bool skipSelectionChanged = true)
        {
            _skipSelectionChanged = skipSelectionChanged;
            dataGridNotes.ClearSelection();            
            dataGridNotes.CurrentCell = dataGridNotes.Rows[0].Cells[1];            
            dataGridNotes.Rows[0].Selected = true;
            _skipSelectionChanged = false;           
        }

        #endregion


    }
}
