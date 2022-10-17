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
    public partial class KntRedminePluginForm : Form
    {
        private readonly PluginContext? _pluginContext;
        private IKntService? _service;

        #region Constructor / Load
        public KntRedminePluginForm()
        {
            InitializeComponent();
        }

        public KntRedminePluginForm(PluginContext? pluginContext) : this()
        {            
            _pluginContext = pluginContext;
        }

        private void KntRedminePluginForm_Load(object sender, EventArgs e)
        {
            if (_pluginContext != null)
            {
                var repositoryRef = new RepositoryRef
                {

                    Alias = _pluginContext?.RepositoryInfo.Alias,
                    ConnectionString = _pluginContext?.RepositoryInfo.ConnectionString,
                    Provider = _pluginContext?.RepositoryInfo.Provider,
                    Orm = _pluginContext?.RepositoryInfo.Orm,
                    ResourcesContainer = _pluginContext?.RepositoryInfo.ResourcesContainer,
                    ResourcesContainerCacheRootPath = _pluginContext?.RepositoryInfo.ResourcesContainerCacheRootPath,
                    ResourcesContainerCacheRootUrl = _pluginContext?.RepositoryInfo.ResourcesContainerCacheRootUrl
                };

                var aa = repositoryRef;
                var serviceRef = new ServiceRef(repositoryRef);
                _service = serviceRef.Service;
            }
            else
            {
                MessageBox.Show("Invalid plugin context");
                this.Close();
            }
        }



        #endregion

        private async void buttonTestService_Click(object sender, EventArgs e)
        {
            if (_service == null)
                return;
            var folders = (await _service.Folders.GetAllAsync()).Entity;

            listInfoLab.Items.Clear();

            foreach (var f in folders)
                listInfoLab.Items.Add(f.Name);

        }
    }
}
