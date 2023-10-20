using KNote.ClientWin.Core;
using System;

namespace KNote.ClientWin.Components;

public class HeavyProcessComponent : ComponentBase
{
    #region Fields

    private bool _processInExecution;

    #endregion 

    #region Public properties

    public CancellationTokenSource CancellationToken { get; set; }    

    public IProgress<KNoteProgress> ReportProgress { get; set; }

    #endregion 

    #region  Constructor

    public HeavyProcessComponent(Store store) : base(store)
    {
        ComponentName = "KntLab Component";
        _processInExecution = false;
    }

    #endregion

    #region View

    IViewHeavyProcess _heavyProcessView;
    protected IViewHeavyProcess HeavyProcessView
    {
        get
        {
            if (_heavyProcessView == null)
                _heavyProcessView = Store.FactoryViews.View(this);
            return _heavyProcessView;
        }
    }

    #endregion

    #region Public components methods

    public void UpdateProgress(int progress)
    {
        HeavyProcessView.UpdateProgress(progress);
    }

    public void UpdateProcessName(string process)
    {
        HeavyProcessView.UpdateProcessName(process);
    }

    public void UpdateProcessInfo(string info)
    {
        HeavyProcessView.UpdateProcessInfo(info);
    }

    public async Task Exec2<TParam1, TParam2>(Func<TParam1, TParam2, CancellationTokenSource, IProgress<KNoteProgress>, HeavyProcessComponent, Task> process, TParam1 param1, TParam2 param2)
    {
        if (!await PrepareTask())
            return;

        try
        {
            if (process != null)
                await process(param1, param2, CancellationToken, ReportProgress, this);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            await CompleteTask();
        }
    }

    public async Task Exec3<TParam1, TParam2, TParam3>(Func<TParam1, TParam2, TParam3, CancellationTokenSource, IProgress<KNoteProgress>, HeavyProcessComponent, Task> process, TParam1 param1, TParam2 param2, TParam3 param3)
    {
        if (!await PrepareTask())
            return;

        try
        {
            if (process != null)
                await process(param1, param2, param3, CancellationToken, ReportProgress, this);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            await CompleteTask();
        }
    }

    #endregion

    #region Private methods

    private async Task<bool> PrepareTask() 
    {
        if (_processInExecution)
        {
            HeavyProcessView.ShowInfo("Cannot run this process, there is another heavy process running.");
            return await Task.FromResult<bool>(false);
        }
        _processInExecution = true;
        HeavyProcessView.UpdateProgress(0);
        CancellationToken = new CancellationTokenSource();
        HeavyProcessView.CancellationToken = CancellationToken;

        HeavyProcessView.ShowView();

        return await Task.FromResult<bool>(true);
    }

    private async Task CompleteTask()
    {
        HeavyProcessView.HideView();
        _processInExecution = false;
        await Task.CompletedTask;
    }

    #endregion 
}

public class KNoteProgress
{
    public int Progress { get; set; }
    public string Info { get; set; }    
    public HeavyProcessComponent HeavyProcessComponent { get; set; }    
}