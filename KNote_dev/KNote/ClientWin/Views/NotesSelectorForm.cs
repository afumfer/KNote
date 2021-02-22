﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
        private UInt32 _countRepetition = 0;
        private bool _skipSelectionChanged = false;        
        private BindingSource _source = new BindingSource();
        private int _orderColNumber = 0;
        private SortOrder _sortOrder;

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
                CoonfigureGridStd();

                if (_orderColNumber == 0)
                {
                    _orderColNumber = 1;
                    _sortOrder = getSortOrder(_orderColNumber);
                }
                RefreshDataGridNotes();

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

        public DialogResult ShowInfo(string info, string caption = "KaNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            return MessageBox.Show(info, caption, buttons, icon);
        }

        #region Extension methods ...

        public object SelectItem(NoteInfoDto item)
        {
            throw new NotImplementedException();
        }

        public void AddItem(NoteInfoDto item)
        {
            // En este caso no se usa item, la actualización se resuelve con databindig
            RefreshDataGridNotes();

            int index = 0;
            if (_com.SelectedEntity != null)
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
            dataGridNotes.ClearSelection();
            dataGridNotes.Rows[index].Selected = true;
        }

        public void DeleteItem(NoteInfoDto item)
        {
            // En este caso no se usa item, la actualización se resuelve con databindig
            RefreshDataGridNotes();

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

        private void dataGridNotes_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            _orderColNumber = e.ColumnIndex;
            _sortOrder = getSortOrder(_orderColNumber);            
            RefreshDataGridNotes();
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

        private void RefreshDataGridNotes()
        {
            _skipSelectionChanged = true;

            CoonfigureGridStd();

            if (_sortOrder == SortOrder.Descending)
                _source.DataSource = _com.ListEntities.OrderByDescending(o => o.GetType().GetProperty(dataGridNotes.Columns[_orderColNumber].Name).GetValue(o));
            else if (_sortOrder == SortOrder.Ascending)
                _source.DataSource = _com.ListEntities.OrderBy(o => o.GetType().GetProperty(dataGridNotes.Columns[_orderColNumber].Name).GetValue(o));

            if (dataGridNotes.Columns.Count > 0)
                dataGridNotes.Columns[_orderColNumber].HeaderCell.SortGlyphDirection = _sortOrder;

            _skipSelectionChanged = false;
        }

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
            if (dataGridNotes.Columns.Count > 0)
                return;
            
            _source.DataSource = new List<NoteInfoDto>();
            dataGridNotes.DataSource = _source;

            dataGridNotes.Columns[0].DataPropertyName = "NoteId";
            dataGridNotes.Columns[0].Visible = false;  // NoteId

            dataGridNotes.Columns[1].DataPropertyName = "NoteNumber";
            dataGridNotes.Columns[1].Width = 80;
            dataGridNotes.Columns[1].HeaderText = "Number";
            dataGridNotes.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dataGridNotes.Columns[2].DataPropertyName = "Topic";
            dataGridNotes.Columns[2].Width = 500;  // Topic
            dataGridNotes.Columns[2].HeaderText = "Topic";

            dataGridNotes.Columns[3].DataPropertyName = "Priority";
            dataGridNotes.Columns[3].Width = 75;  // Priority
            dataGridNotes.Columns[3].HeaderText = "Priority";
            dataGridNotes.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dataGridNotes.Columns[4].DataPropertyName = "Tags";
            dataGridNotes.Columns[4].Width = 200;  // Tags
            dataGridNotes.Columns[4].HeaderText = "Tags";

            dataGridNotes.Columns[5].DataPropertyName = "ModificationDateTime";
            dataGridNotes.Columns[5].Width = 125;  // ModificationDateTime
            dataGridNotes.Columns[5].HeaderText = "Modification date";
            dataGridNotes.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dataGridNotes.Columns[6].DataPropertyName = "CreationDateTime";
            dataGridNotes.Columns[6].Width = 125;  // CreationDateTime
            dataGridNotes.Columns[6].HeaderText = "Creation date";
            dataGridNotes.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dataGridNotes.Columns[7].Visible = false;  // Description 
            dataGridNotes.Columns[7].DataPropertyName = "Description";
            dataGridNotes.Columns[8].Visible = false;  // ContentType
            dataGridNotes.Columns[8].DataPropertyName = "ContentType";
            dataGridNotes.Columns[9].Visible = false;  // Script
            dataGridNotes.Columns[9].DataPropertyName = "Script";
            dataGridNotes.Columns[10].Visible = false;  // InternalTags
            dataGridNotes.Columns[10].DataPropertyName = "InternalTags";
            dataGridNotes.Columns[11].Visible = false;  // FolderId 
            dataGridNotes.Columns[11].DataPropertyName = "FolderId";
            dataGridNotes.Columns[12].Visible = false;  // NoteTypeId
            dataGridNotes.Columns[12].DataPropertyName = "NoteTypeId";            
        }

        private NoteInfoDto DataGridViewRowToNoteItemList(DataGridViewRow dgr)
        {
            var n = new NoteInfoDto();

            if (dgr == null)
                return null;
            else
            {
                n.NoteId = (Guid)dgr.Cells["NoteId"].Value;
                n.NoteNumber = (int)dgr.Cells["NoteNumber"].Value;
                n.Topic = (string)dgr.Cells["Topic"].Value;
                n.Priority = (int)dgr.Cells["Priority"].Value;
                n.Tags = (string)dgr.Cells["Tags"].Value;                
                n.ModificationDateTime = (DateTime)dgr.Cells["ModificationDateTime"].Value;
                n.CreationDateTime = (DateTime)dgr.Cells["CreationDateTime"].Value;
                n.Description = (string)dgr.Cells["Description"].Value;
                n.ContentType = (string)dgr.Cells["ContentType"].Value;
                n.Script = (string)dgr.Cells["Script"].Value;
                n.InternalTags = (string)dgr.Cells["InternalTags"].Value;                                
                n.FolderId = (Guid)dgr.Cells["FolderId"].Value;
                if (dgr.Cells[12].Value != null)
                    n.NoteTypeId = (Guid)dgr.Cells["NoteTypeId"].Value;
                else
                    n.NoteTypeId = null;
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

        private SortOrder getSortOrder(int columnIndex)
        {
            if (dataGridNotes.Columns[columnIndex].HeaderCell.SortGlyphDirection == SortOrder.None ||
                dataGridNotes.Columns[columnIndex].HeaderCell.SortGlyphDirection == SortOrder.Descending)
            {
                dataGridNotes.Columns[columnIndex].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
                return SortOrder.Ascending;
            }
            else
            {
                dataGridNotes.Columns[columnIndex].HeaderCell.SortGlyphDirection = SortOrder.Descending;
                return SortOrder.Descending;
            }
        }

        #endregion

    }
}
