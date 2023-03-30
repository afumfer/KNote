using KNote.Model;

namespace KNote.ClientWin.Core;

//TODO: refactor view hierarchy

#region  Base views

public interface IViewBase
{        
    void ShowView();
    Result<EComponentResult> ShowModalView();
    void RefreshView();
    void OnClosingView();        
    DialogResult ShowInfo(string info, string caption = "KaNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information);
}

public interface IViewConfigurable: IViewBase
{
    Control PanelView();
    void ConfigureEmbededMode();
    void ConfigureWindowMode();   
}

public interface IViewConfigurableExt : IViewConfigurable
{
    void HideView();
    void ActivateView();
}

#endregion

#region Generals views

public interface IViewEditor<T> : IViewBase
{
    //void RefreshView();
    void CleanView();
    void RefreshModel();
}

public interface IViewEditorEmbeddable<T> : IViewConfigurable
{
    void CleanView();
    void RefreshModel();                
}

public interface IViewEditorEmbeddableExt<T> : IViewConfigurableExt
{
    void CleanView();
    void RefreshModel();
}

public interface IViewSelector<TItem> : IViewConfigurable
{                
    void RefreshItem(TItem item);
    void DeleteItem(TItem item);
    void AddItem(TItem item);
    object SelectItem(TItem item);
    List<TItem> GetSelectedListItem();
}

#endregion

#region Specific views

public interface IViewKNoteManagment : IViewBase
{
    void HideView();
    void ActivateView();    
}

public interface IViewPostIt<T> : IViewBase
{
    //void RefreshView();
    void CleanView();
    void RefreshModel();
    void HideView();
    void ActivateView();
}

#endregion 

