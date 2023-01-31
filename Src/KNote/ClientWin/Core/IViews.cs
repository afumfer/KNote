using KNote.Model;

namespace KNote.ClientWin.Core;

//TODO: refactor view hierarchy

#region  Base view

public interface IViewBase
{        
    void ShowView();
    Result<EComponentResult> ShowModalView();
    void OnClosingView();        
    DialogResult ShowInfo(string info, string caption = "KaNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information);
}

public interface IViewConfigurable: IViewBase
{
    Control PanelView();
    void ConfigureEmbededMode();
    void ConfigureWindowMode();        
    void RefreshView();
}

public interface IViewConfigurableExt : IViewConfigurable
{
    void HideView();
    void ActivateView();
}

#endregion

#region Generals views

public interface IEditorView<T> : IViewConfigurable
{
    void CleanView();
    void RefreshModel();                
}

public interface IEditorViewExt<T> : IViewConfigurableExt
{
    void CleanView();
    void RefreshModel();
}

public interface ISelectorView<TItem> : IViewConfigurable
{                
    void RefreshItem(TItem item);
    void DeleteItem(TItem item);
    void AddItem(TItem item);
    object SelectItem(TItem item);
    List<TItem> GetSelectedListItem();
}

#endregion

#region Specific views

#endregion 

