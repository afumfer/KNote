using KNote.Model;

namespace KNote.ClientWin.Core;

//TODO: refactor view hierarchy

#region  Base views

public interface IViewBase
{        
    void ShowView();
    Result<EControllerResult> ShowModalView();
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

#endregion

#region Generals views

public interface IViewEditor<T> : IViewBase
{    
    void CleanView();
    void RefreshModel();
}

public interface IViewEditorEmbeddable<T> : IViewEmbeddable
{
    void CleanView();
    void RefreshModel();
    void RefreshViewOnlyRequiredCtrl();
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

public interface IViewHeavyProcess : IViewBase
{
    CancellationTokenSource CancellationToken { get; set; }
    public IProgress<KeyValuePair<int, string>> ReportProgress { get; set; }
    void UpdateProgress(int progress);
    void UpdateProcessName(string process);
    void UpdateProcessInfo(string info);
    void HideView();    
}


#endregion 

