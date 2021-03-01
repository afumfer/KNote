using System;
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
    public partial class NoteTypesSelectorForm : Form, ISelectorView<NoteTypeDto>
    {
        private readonly NoteTypesSelectorComponent _com;
        private bool _viewFinalized = false;

        public NoteTypesSelectorForm(NoteTypesSelectorComponent com)
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

        public Result<EComponentResult> ShowModalView()
        {
            return _com.DialogResultToComponentResult(this.ShowDialog());
        }

        public void OnClosingView()
        {
            _viewFinalized = true;
            this.Close();
        }

        public void RefreshView()
        {
            if (_com.ListEntities == null)
                return;
            else
            {
                listViewNoteTypes.Clear();

                foreach (var type in _com.ListEntities)
                {
                    listViewNoteTypes.Items.Add(NoteTypeDtoToListViewItem(type));
                }
                
                listViewNoteTypes.Columns.Add("Name", 120, HorizontalAlignment.Left);
                listViewNoteTypes.Columns.Add("Description", 240, HorizontalAlignment.Left);
            }
        }

        private ListViewItem NoteTypeDtoToListViewItem(NoteTypeDto type)
        {
            var itemList = new ListViewItem(type.Name);
            itemList.Name = type.NoteTypeId.ToString();
            itemList.SubItems.Add(type.Description?.ToString());
            return itemList;            
        }

        public DialogResult ShowInfo(string info, string caption = "KaNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            return MessageBox.Show(info, caption, buttons, icon);
        }

        public void ConfigureEmbededMode()
        {
            
        }

        public void ConfigureWindowMode()
        {
            
        }

        public object SelectItem(NoteTypeDto item)
        {
            throw new NotImplementedException();
        }

        public void AddItem(NoteTypeDto item)
        {
            throw new NotImplementedException();
        }

        public void DeleteItem(NoteTypeDto item)
        {
            throw new NotImplementedException();
        }

        public void RefreshItem(NoteTypeDto item)
        {
            throw new NotImplementedException();
        }

        public List<NoteTypeDto> GetSelectedListItem()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Form events handlers

        private void NoteTypesSelectorForm_FormClosing(object sender, FormClosingEventArgs e)
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

        private void listViewNoteTypes_DoubleClick(object sender, EventArgs e)
        {

        }

        private void listViewNoteTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedItemChanged();
        }

        #endregion

        private void listViewNoteTypes_Resize(object sender, EventArgs e)
        {
            SizeLastColumn((ListView)sender);
        }
        private void SizeLastColumn(ListView lv)
        {
            // Hack for control undeterminated error
            try
            {
                lv.Columns[lv.Columns.Count - 1].Width = -2;
            }
            catch (Exception)
            {
            }
        }

        private void OnSelectedItemChanged()
        {            
            try
            {
                if (_com.ListEntities == null)
                    return;

                if (listViewNoteTypes.SelectedItems.Count > 0)
                {
                    this.Cursor = Cursors.WaitCursor;                    
                    var selectedItem = Guid.Parse(listViewNoteTypes.SelectedItems[0].Name);
                    _com.SelectedEntity = _com.ListEntities.Where(_ => _.NoteTypeId == selectedItem).SingleOrDefault();
                    _com.NotifySelectedEntity();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"OnSelectedItemChanged error: {ex.Message}");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void PersonalizeListView(ListView listView)
        {
            listView.View = View.Details;
            listView.LabelEdit = false;
            listView.AllowColumnReorder = false;
            listView.CheckBoxes = false;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.Sorting = SortOrder.None;
        }

        private void NoteTypesSelectorForm_Load(object sender, EventArgs e)
        {
            PersonalizeListView(listViewNoteTypes);
        }
    }
}
