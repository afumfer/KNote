using KNote.ClientWin.Core;
using System;

namespace KNote.ClientWin.Components;

public class HeavyProcessComponent : ComponentBase
{
    #region Public properties

    public CancellationTokenSource CancellationToken { get; set; }
    public IProgress<KeyValuePair<int, string>> ReportProgress { get; set; }

    #endregion 

    #region  Constructor

    public HeavyProcessComponent(Store store) : base(store)
    {
        ComponentName = "KntLab Component";
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

    public async Task Exec2<TParam1, TParam2>(Func<TParam1, TParam2, IProgress<KeyValuePair<int, string>>, CancellationTokenSource, Task> process, TParam1 action, TParam2 selectedNotes)
    {
        try
        {
            HeavyProcessView.UpdateProgress(0);
            CancellationToken = new CancellationTokenSource();
            HeavyProcessView.CancellationToken = CancellationToken;

            HeavyProcessView.ShowView();

            if (process != null)
            {
                await process(action, selectedNotes, ReportProgress, CancellationToken);
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            HeavyProcessView.HideView();
        }
    }

    public async Task Exec3<TParam1, TParam2, TParam3>(Func<TParam1, TParam2, TParam3, IProgress<KeyValuePair<int, string>>, CancellationTokenSource, Task> process, TParam1 action, TParam2 selectedNotes, TParam3 tag)
    {
        try
        {
            HeavyProcessView.UpdateProgress(0);
            CancellationToken = new CancellationTokenSource();
            HeavyProcessView.CancellationToken = CancellationToken;

            HeavyProcessView.ShowView();

            if (process != null)
            {
                await process(action, selectedNotes, tag, ReportProgress, CancellationToken);
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            HeavyProcessView.HideView();
        }
    }

    #endregion 

}