using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Service;

namespace KNote.ClientWin.Components;

public class OptionsEditorComponent : ComponentEditor<IEditorView<AppConfig>, AppConfig>
{
    #region Constructor 

    public OptionsEditorComponent(Store store) : base(store)
    {
        ComponentName = "Options editor";
    }

    #endregion 

    #region ComponentEditor implementation 

    protected override IEditorView<AppConfig> CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> NewModel(IKntService service = null)
    {
        throw new NotImplementedException();
    }

    public async override Task<bool> SaveModel()
    {
        View.RefreshModel();

        if (!Model.IsDirty())
            return await Task.FromResult<bool>(true);

        var msgVal = Model.GetErrorMessage();
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return await Task.FromResult<bool>(false);
        }

        Store.AppConfig.AlarmActivated = Model.AlarmActivated;
        Store.AppConfig.AlarmSeconds = Model.AlarmSeconds;
        Store.AppConfig.AutoSaveActivated = Model.AutoSaveActivated;
        Store.AppConfig.AutoSaveSeconds = Model.AutoSaveSeconds;
        Store.AppConfig.CompactViewNoteslist = Model.CompactViewNoteslist;
        Store.AppConfig.Plugins = Model.Plugins;
        //Store.AppConfig.Plugins = new List<string>();
        //foreach (var p in Model.Plugins)
        //    Store.AppConfig.Plugins.Add(p);
        Store.SaveConfig();

        return await Task.FromResult<bool>(true);
    }

    public override Task<bool> DeleteModel(IKntService service, Guid id)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> DeleteModel()
    {
        throw new NotImplementedException();
    }

    #endregion
}

