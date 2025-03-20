using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class NoteEditorCtrl : CtrlNoteEditorEmbeddableBase<IViewEditorEmbeddable<NoteExtendedDto>, NoteExtendedDto>
{
    #region Properties
       
    public bool EditMode { get; set; }

    #endregion

    #region Constructor, Dispose, ...

    public NoteEditorCtrl(Store store) : base(store)
    {
        ControllerName = "Note editor";
        Store.DeletedNote += Store_DeletedNote;
    }

    public override void Dispose()
    {
        Store.DeletedNote -= Store_DeletedNote;
        base.Dispose();
    }

    #endregion

    #region Store events 

    private void Store_DeletedNote(object sender, ControllerEventArgs<NoteExtendedDto> e)
    {
        if (EmbededMode)        
            return;
        if (e.Entity.NoteId == Model.NoteId)
            Finalize();
    }

    #endregion 

    #region Controller specific events 

    public event EventHandler<ControllerEventArgs<ServiceWithNoteId>> PostItEdit;
    protected virtual void OnPostItEdit()
    {
        PostItEdit?.Invoke(this, new ControllerEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = Service, NoteId = Model.NoteId }));
    }

    #endregion

    #region IViewEditorEmbeddable implementation

    protected override IViewEditorEmbeddable<NoteExtendedDto> CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    #endregion

    #region EditorBase controller override methods

    public override async Task<bool> LoadModelById(IKntService service, Guid noteId, bool refreshView = true)
    {
        try
        {                
            Service = service;

            Model = (await Service.Notes.GetExtendedAsync(noteId)).Entity;
            Model.SetIsDirty(false);
            if(refreshView)
                View.RefreshView();
            return true;
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return false;
        }
    }
        
    public override async Task<bool> NewModel(IKntService service)
    {
        try
        {
            Service = service;
                
            var response = await Service.Notes.NewExtendedAsync();
            Model = response.Entity;

            // Evaluate whether to put the following default values in the service layer 
            // (null values are by default, we need empty strings so that the IsDirty is 
            //  not altered after leaving the view when there are no modifications).
            Model.Topic = "New topic ...";                
            Model.Tags = "";
            Model.Description = "";            

            // Context default values
            if((Store.ActiveFolderWithServiceRef?.FolderInfo != null) 
                && (Store.ActiveFolderWithServiceRef.ServiceRef.IdServiceRef == Service.IdServiceRef) )
            {
                Model.FolderId = Store.ActiveFolderWithServiceRef.FolderInfo.FolderId;
                Model.FolderDto = Store.ActiveFolderWithServiceRef.FolderInfo.GetSimpleDto<FolderDto>();
            }
            else
            {                
                var folder = (await Service.Folders.GetAsync(KntConst.DefaultFolderNumber)).Entity;
                Model.FolderId = folder.FolderId;
                Model.FolderDto = folder;
            }

            Model.SetIsDirty(false);

            View.RefreshView();
                
            return true;
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);                
        }
        return false;
    }

    public override async Task<bool> SaveModel()
    {
        View.RefreshModel();

        if (!Model.IsDirty())
            return true;

        var isNew = (Model.NoteId == Guid.Empty);
                        
        var msgVal = Model.GetErrorMessage();
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return false;
        }

        try
        {                                
            var response = await Service.Notes.SaveExtendedAsync(Model);

            if (response.IsValid)
            {
                Model = response.Entity;
                    
                Model.SetIsDirty(false);
                Model.SetIsNew(false);

                if (!isNew)
                    OnSavedEntity(response.Entity);
                else
                    OnAddedEntity(response.Entity);

                // Experimental
                //View.RefreshView();
                View.RefreshViewOnlyRequiredCtrl();

                // TODO: future version ... notify actions.
                // NotifyMessage($"Note {Model?.NoteNumber.ToString()} saved");
            }
            else            
                View.ShowInfo(response.ErrorMessage);
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return true;
        }

        return true;
    }

    public override async Task<bool> DeleteModel()
    {
        return await DeleteModel(Service, Model.NoteId);
    }

    public override async Task<bool> DeleteModel(IKntService service, Guid noteId) 
    {
        var result = View.ShowInfo("Are you sure you want to delete this note?", "Delete note", MessageBoxButtons.YesNo);
        if (result == DialogResult.Yes || result == DialogResult.Yes)
        {
            try
            {
                var response = await service.Notes.DeleteExtendedAsync(noteId);

                if (response.IsValid)
                    OnDeletedEntity(response.Entity);
                
                return true;
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
            }
        }
        return false;
    }

    #endregion

    #region Controller specific methods

    public NoteKAttributeDto EditAttribute(NoteKAttributeDto noteAttribute)
    {
        var noteAttributeEditor = new NoteAttributeEditorCtrl(Store);

        noteAttributeEditor.AutoDBSave = false;  // don't save automatically
            
        noteAttributeEditor.LoadModel(Service, noteAttribute, false);

        var res = noteAttributeEditor.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            return noteAttributeEditor.Model;
        }
        else
            return null;
    }

    public async Task<bool> RequestChangeNoteType(Guid? oldSelectedId)
    {
        var noteTypesSelector = new NoteTypesSelectorCtrl(Store);
        var resCanLoadEntities = await noteTypesSelector.LoadEntities(Service, false);            
        if (resCanLoadEntities)
        {
            var res = noteTypesSelector.RunModal();
            if (res.Entity == EControllerResult.Executed)
            {
                if (oldSelectedId == noteTypesSelector.SelectedEntity.NoteTypeId)
                    return false;
                else                    
                    return await AplyChangeNoteType(noteTypesSelector.SelectedEntity);                    
            }                                    
        }
        else
            View.ShowInfo("Cannot load the list of note types");

        return false ;
    }

    public async Task<bool> AplyChangeNoteType(NoteTypeDto newType)
    {
        try
        {
            Model.NoteTypeId = newType?.NoteTypeId;
            Model.NoteTypeDto = newType;

            // Delete old attributes 
            Model.KAttributesDto.RemoveAll(_ => _.KAttributeNoteTypeId != null);

            if (newType == null)
                return true;

            // Add new attributes
            Model.KAttributesDto = await Service.Notes.UtilCompleteNoteAttributes(Model.KAttributesDto, Model.NoteId, newType.NoteTypeId);                
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<KMessageDto> NewMessage()
    {
        var messageEditor = new MessageEditorCtrl(Store);

        messageEditor.AutoDBSave = false;  // don't save automatically
            
        await messageEditor.NewModel(Service);            
        messageEditor.Model.NoteId = Model.NoteId;
        messageEditor.Model.Comment = "(Aditional text for message)";
        var userDto = (await Service.Users.GetByUserNameAsync(Store.AppUserName)).Entity;
        messageEditor.Model.UserId = userDto.UserId;
        messageEditor.Model.UserFullName = userDto.FullName;
        messageEditor.Model.AlarmActivated = true;
        messageEditor.Model.AlarmDateTime = DateTime.Now;
        messageEditor.Model.SetIsNew(true);

        var res = messageEditor.RunModal();

        if(res.Entity == EControllerResult.Executed)
        {                
            Model.Messages.Add(messageEditor.Model);
            return messageEditor.Model;
        }            
        else
            // TODO: show error here 
            return null;            
    }

    public KMessageDto EditMessage(Guid messageId)
    {
        var messageEditor = new MessageEditorCtrl(Store);

        messageEditor.AutoDBSave = false;  // don't save automatically
            
        var message = Model.Messages.Where(_ => _.KMessageId == messageId).SingleOrDefault();            
        messageEditor.LoadModel(Service, message, false);

        var res = messageEditor.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {                
            return messageEditor.Model;
        }
        else
            return null;
    }

    public bool DeleteMessage(Guid messageId)
    {
        bool res = false;

        KMessageDto msgDel = null;
        foreach (var item in Model.Messages)
        {
            if (item.KMessageId == messageId)
            {
                msgDel = item;
                break;
            }
        }

        if (msgDel != null)
        {
            if (msgDel.IsNew())
                Model.Messages.Remove(msgDel);
            else
                msgDel.SetIsDeleted(true);
            res = true;
        }

        return res;
    }

    public async Task<NoteTaskDto> NewTask()
    {
        var taskEditor = new TaskEditorCtrl(Store);

        taskEditor.AutoDBSave = false;  // don't save automatically

        await taskEditor.NewModel(Service);
        taskEditor.Model.NoteId = Model.NoteId;            
        var userDto = (await Service.Users.GetByUserNameAsync(Store.AppUserName)).Entity;
        taskEditor.Model.UserId = userDto.UserId;
        taskEditor.Model.UserFullName = userDto.FullName;
        taskEditor.Model.Description = "(Task descripcion ...)";
        taskEditor.Model.StartDate = DateTime.Now;
        taskEditor.Model.SetIsNew(true);

        var res = taskEditor.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            Model.Tasks.Add(taskEditor.Model);
            return taskEditor.Model;
        }
        else                            
            return null;
            
    }

    public NoteTaskDto EditTask(Guid taskId)
    {
        var taskEditor = new TaskEditorCtrl(Store);

        taskEditor.AutoDBSave = false;  // don't save automatically            

        var task = Model.Tasks.Where(_ => _.NoteTaskId == taskId).SingleOrDefault();            
        taskEditor.LoadModel(Service, task, false);

        var res = taskEditor.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            return taskEditor.Model;
        }
        else
            return null;
    }

    public bool DeleteTask(Guid taskId)
    {
        bool res = false;

        NoteTaskDto tskDel = null;
        foreach (var item in Model.Tasks)
        {
            if (item.NoteTaskId == taskId)
            {
                tskDel = item;
                break;
            }
        }

        if (tskDel != null)
        {
            if (tskDel.IsNew())
                Model.Tasks.Remove(tskDel);
            else
                tskDel.SetIsDeleted(true);
            res = true;
        }

        return res;
    }


    public async Task<ResourceDto> NewResource()
    {
        var resource = new ResourceEditorCtrl(Store);
        resource.AutoDBSave = false;  // don't save automatically

        await resource.NewModel(Service);
        resource.Model.NoteId = Model.NoteId;
        resource.Model.SetIsNew(true);
            
        var res = resource.RunModal();

        if (res.Entity == EControllerResult.Executed)
        {
            Model.Resources.Add(resource.Model);
            return resource.Model;
        }
        else if (res.Entity == EControllerResult.Error)
        {
            View.ShowInfo($"Error: {res.ErrorMessage}");
            return null;
        }
        else
            return null;            
    }

    public ResourceDto NewResourceFromClipboard(bool contentInDB = false)
    {
        try
        {
            if (!Clipboard.GetDataObject().GetDataPresent(DataFormats.Bitmap))
            {
                View.ShowInfo("You do not have any images on the Clipboard to insert into this note.", KntConst.AppName);
                return null;
            }
            var bm = (Bitmap)Clipboard.GetDataObject().GetData(DataFormats.Bitmap);
            var converter = new ImageConverter();

            var newResource = new ResourceDto();
            newResource.SetIsNew(true);
            newResource.ResourceId = Guid.NewGuid();
            newResource.NoteId = Model.NoteId;
            newResource.ContentInDB = contentInDB;
            newResource.Description = "Image inserted from clipboard.";
            newResource.Order = 0;
            newResource.Name = "ClipboardImg_" + newResource.ResourceId.ToString() + ".png";
            newResource.FileType = Store.ExtensionFileToFileType(".png");
            newResource.Container = Service.Notes.UtilGetDefaultNewResourceContainer();
            newResource.ContentArrayBytes = (byte[])converter.ConvertTo(bm, typeof(byte[]));

            Service.Notes.UtilManageResourceContent(newResource);
            
            Model.Resources.Add(newResource);
            return newResource;
        }
        catch (Exception ex)
        {
            View.ShowInfo($"Error: {ex.Message}");
            return null;
        }
    }


    public Task<ResourceDto> EditResource(Guid resourceId)
    {
        var resourceEditor = new ResourceEditorCtrl(Store);
        resourceEditor.AutoDBSave = false;  // don't save automatically

        var resource = Model.Resources.Where(_ => _.ResourceId == resourceId).SingleOrDefault();                     

        resourceEditor.LoadModel(Service, resource, false);
        
        var res = resourceEditor.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            return Task.FromResult(resourceEditor.Model);
        }
        else
            return Task.FromResult<ResourceDto>(null);
    }

    public bool DeleteResource(Guid resourceId)
    {
        bool res = false;

        ResourceDto resDel = null;
        foreach (var item in Model.Resources)
        {
            if (item.ResourceId == resourceId)
            {
                resDel = item;
                break;
            }
        }

        if (resDel != null)
        {
            if (resDel.IsNew())
            {
                Model.Resources.Remove(resDel);                
                var fullPathRec = Path.Combine(Service.RepositoryRef.ResourcesContainerRootPath, resDel.Container, resDel.Name);
                if (File.Exists(fullPathRec))
                    File.Delete(fullPathRec);
            }
            else
                resDel.SetIsDeleted(true);
            res = true;
        }
 
        return res;
    }

    public void FinalizeAndPostItEdit()
    {
        OnPostItEdit();
        Finalize();
    }

    public void CleanView()
    {
        View.CleanView();
    }

    public void RunScript()
    {
        Store.RunScript(Model.Script);
    }

    public async Task<string> GetCatalogTemplate()
    {
        return await Store.GetCatalogItem(ServiceRef, KntConst.TemplateTag, "Select template");
    }

    public async Task<string> GetCatalogCode()
    {        
        return await Store.GetCatalogItem(ServiceRef, KntConst.CodeTag, "Select code snippet");
    }

    #endregion
}

