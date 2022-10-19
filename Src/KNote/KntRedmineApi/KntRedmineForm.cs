using KNote.Model;
using KNote.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KntRedmineApi
{
    public partial class KntRedmineForm : Form
    {        
        private IKntService? _service;

        public KntRedmineForm()
        {
            InitializeComponent();
        }

        public KntRedmineForm(IKntService? service) : this()
        {
            _service = service;
        }

        private async void KntRedmineForm_Load(object sender, EventArgs e)
        {
            var isValidService = await IsValidService();
            if (!isValidService)
            {
                MessageBox.Show("This database not support KaNote Redmine utils.");
                this.Close();
            }

            // RedMineAPI
            textHost.Text = await GetSystemPlugInVariable("HOST"); 
            textApiKey.Text = await GetSystemPlugInVariable("APIKEY");
            //textIssuesImportFile.Text = _store.AppConfig.IssuesImportFile;

            try
            {
                textIssuesId.Text = File.ReadAllText(textIssuesImportFile.Text);
            }
            catch { }
        }

        private async void buttonTestService_Click(object sender, EventArgs e)
        {
            if (_service == null)                        
                return;
            
            var folders = (await _service.Folders.GetAllAsync()).Entity;

            listInfoLab.Items.Clear();

            foreach (var f in folders)
                listInfoLab.Items.Add(f.Name);
        }

        private async Task<bool> IsValidService()
        {
            //// TODO: refactor this method
            //if (_service == null)            
            //    return false;

            //var supportEducaRedmine = await _service.SystemValues.GetAsync("KNT_REDMINEEDUCA_PLUGIN", "PLUGIN_SUPPORT");
            //if (supportEducaRedmine.IsValid)
            //{
            //    if (supportEducaRedmine.Entity.Value.ToLower() != "true")                
            //        return false;

            //}
            //else
            //{
            //    return false;
            //}            

            //return true;

            var supportEducaRedmine = await GetSystemPlugInVariable("PLUGIN_SUPPORT");
                        
            if (supportEducaRedmine.ToLower() == "true")
                return true;
            else
                return false;
                      
        }

        private async Task<string> GetSystemPlugInVariable(string variable)
        {
            // TODO: refactor this method
            if (_service == null)
                return "";

            var supportEducaRedmine = await _service.SystemValues.GetAsync("KNT_REDMINEEDUCA_PLUGIN", variable);
            if (supportEducaRedmine.IsValid)            
                return supportEducaRedmine.Entity.Value;                                
            else            
                return "";                        
        }


    }
}
