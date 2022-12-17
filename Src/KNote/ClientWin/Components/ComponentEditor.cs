﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using System.Windows.Forms;

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using KNote.Service.Core;

namespace KNote.ClientWin.Components
{
    public abstract class ComponentEditor<TView, TEntity> : ComponentEditorBase<TView, TEntity>
        where TView : IViewConfigurable
        where TEntity : SmartModelDtoBase, new()
    {
        public ComponentEditor(Store store) : base(store)
        {

        }

        public virtual FolderInfoDto GetFolder()
        {
            var folderSelector = new FoldersSelectorComponent(Store);
            var services = new List<ServiceRef>();
            services.Add(Store.GetServiceRef(Service.IdServiceRef));
            folderSelector.ServicesRef = services;
            var res = folderSelector.RunModal();
            if (res.Entity == EComponentResult.Executed)
                return folderSelector.SelectedEntity.FolderInfo;

            return null;
        }

    }
}
