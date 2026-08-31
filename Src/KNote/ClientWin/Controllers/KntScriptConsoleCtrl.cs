using KNote.ClientWin.Core;
using KNote.ClientWin.Views;
using KNote.Model;
using KntScript;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KNote.ClientWin.Controllers;

public class KntScriptConsoleCtrl : CtrlViewBase<IViewBase>
{
    #region Private fields

    private KntSEngine _kntSEngine;

    #endregion

    #region Properties

    public string CodeFile { get; set; }

    public IInOutDevice KntScriptInOutDevice
    {
        get { return _kntSEngine.InOutDevice; }
    }
    
    #endregion 

    #region Constructor 

    public KntScriptConsoleCtrl(Store store): base(store)
    {
        ControllerName = "KeyNote script console";
        _kntSEngine = new KntSEngine(new InOutDeviceForm(), new KNoteScriptLibrary(Store));
    }

    #endregion

    #region Controller overrid methods

    protected override IViewBase CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<KntScriptConsoleCtrl, IViewBase>(this);
    }

    #endregion

    #region Public methods

    public void RunKntSCode(string code)
    {
        _kntSEngine.InOutDevice.Clear();
        _kntSEngine.ClearAllVars();
        _kntSEngine.Run(code);        
    }

    public void RunCSCode(string code)
    {
        KntScriptInOutDevice.Clear();
        (var result, var error) = Store.RunCSCode(code, true);
        if(string.IsNullOrEmpty(error))
            error = "CS code executed.";
        else
            error = "CS code executed with the following errors:\n" + error;
        KntScriptInOutDevice.Print($"{result}\r\n\r\n{"----"}\r\n{error}");
    }

    public void RunCSCodeStdOut(string code)
    {
        KntScriptInOutDevice.Clear();
        Store.RunCSCode(code, false);
        KntScriptInOutDevice.Print("\r\n\r\n----\r\nCS code executed.");
    }

    public void RunPyCode(string code)
    {
        KntScriptInOutDevice.Clear();
        (var result, var error) = Store.RunPyCode(code, true);
        if (string.IsNullOrEmpty(error))
            error = "Python code executed.";
        else
            error = "Python code executed with the following errors:\n" + error;
        KntScriptInOutDevice.Print($"{result}\r\n\r\n{"----"}\r\n{error}");
    }

    public void RunPyCodeStdOut(string code)
    {
        KntScriptInOutDevice.Clear();
        Store.RunPyCode(code, false);
        KntScriptInOutDevice.Print("\r\n\r\n----\r\nPython code executed.");
    }

    public void RunJsCode(string code)
    {
        KntScriptInOutDevice.Clear();
        (var result, var error) = Store.RunJsCode(code, true);
        if (string.IsNullOrEmpty(error))
            error = "JavaScript code executed.";
        else
            error = "JavaScript code executed with the following errors:\n" + error;
        KntScriptInOutDevice.Print($"{result}\r\n\r\n{"----"}\r\n{error}");
    }

    public void RunJsCodeStdOut(string code)
    {
        KntScriptInOutDevice.Clear();
        Store.RunJsCode(code, false);
        KntScriptInOutDevice.Print("\r\n\r\n----\r\nJavaScript code executed.");
    }

    public async Task RunNaturalLanguageCode(string prompt)
    {
        KntScriptInOutDevice.Clear();
        await Store.RunNaturalLanguageCode(prompt);
        KntScriptInOutDevice.Print("Natural language prompt sent to the KNote AI Assistant - see its window for the response.");
    }

    #endregion
}
