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
    DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information);
}

public interface IViewEmbeddable : IViewBase
{
    Control PanelView();
    void ConfigureEmbededMode();
    void ConfigureWindowMode();   
}

public interface IViewIViewEmbeddableExt : IViewEmbeddable
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

public interface IViewEditorEmbeddable<T> : IViewEmbeddable
{
    void CleanView();
    void RefreshModel();                
}

public interface IViewEditorEmbeddableExt<T> : IViewIViewEmbeddableExt
{
    void CleanView();
    void RefreshModel();
}

public interface IViewSelector<TItem> : IViewEmbeddable
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
    void ActivateWaitState();
    void DeactivateWaitState();
    void ReportProgressKNoteManagment(int porcentaje);
    void SetVisibleProgressBar(bool visible);    
}

public interface IViewPostIt<T> : IViewBase
{    
    void CleanView();
    void RefreshModel();
    void HideView();
    void ActivateView();
}

public interface IViewChat : IViewBase
{
    void VisibleView(bool visible);
}

public interface IViewServerCOM : IViewChat
{
    void RefreshStatus();
}

#endregion 

