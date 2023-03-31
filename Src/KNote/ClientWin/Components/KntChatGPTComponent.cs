using KNote.ClientWin.Core;
using KNote.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.ClientWin.Components;

public class KntChatGPTComponent : ComponentBase
{
    public KntChatGPTComponent(Store store) : base(store)
    {
        ComponentName = "KntChatGPT Component";
    }

    IViewBase _chatGPTView;

    public void ShowChatGPTView()
    {
        if (_chatGPTView == null)
            _chatGPTView = Store.FactoryViews.View(this);

        _chatGPTView.ShowView();
    }

    //protected override Result<EComponentResult> OnInitialized()
    //{
    //    try
    //    {
    //        VisibleWindows();

    //        kntTimerAlarms = new System.Windows.Forms.Timer();
    //        kntTimerAlarms.Tick += kntTimerAlarms_Tick;
    //        kntTimerAlarms.Interval = Store.AppConfig.AlarmSeconds * 1000;
    //        kntTimerAlarms.Start();

    //        kntTimerAutoSave = new System.Windows.Forms.Timer();
    //        kntTimerAutoSave.Tick += KntTimerAutoSave_Tick;
    //        kntTimerAutoSave.Interval = Store.AppConfig.AutoSaveSeconds * 1000;
    //        kntTimerAutoSave.Start();

    //        return new Result<EComponentResult>(EComponentResult.Executed);
    //    }
    //    catch (Exception)
    //    {
    //        return new Result<EComponentResult>(EComponentResult.Error);
    //    }
    //}

}
