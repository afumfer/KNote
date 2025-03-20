using System.Reflection;

using KNote.Model;

namespace KNote.ClientWin.Core;

abstract public class CtrlBase : IDisposable
{
    #region Public properties

    public readonly Guid ControllerId;

    public string ControllerName { get; protected set; }

    public Store Store { get; protected set; }

    public EControllerState ControllerState { get; protected set; } = EControllerState.NotStarted;
            
    public bool EmbededMode { get; set; } = false;
    
    public bool ThrowKntException { get; set; } = false;

    #endregion

    #region Protected properties

    private List<FieldInfo> _fields;   
    protected List<FieldInfo> Fields
    {
        get
        {
            if (_fields == null)
                _fields = GetAllClassFields();
            return _fields;
        }
    }

    #endregion region 

    #region Standard events for base controller
    
    public event EventHandler<ControllerEventArgs<EControllerState>> StateControllerChanged;

    protected void OnStateControllerChanged(EControllerState state)
    {
        ControllerState = state;
        StateControllerChanged?.Invoke(this, new ControllerEventArgs<EControllerState>(state));
    }
    
    #endregion

    #region Constructor

    public CtrlBase(Store store)
    {
        ControllerId = Guid.NewGuid();
        OnStateControllerChanged(EControllerState.NotStarted);           
        Store = store;
        Store.AddController(this);
    }

    #endregion

    protected virtual Result<EControllerResult> OnInitialized()
    {
        return new Result<EControllerResult>(EControllerResult.Executed);
    } 

    protected virtual Result<EControllerResult> CheckPreconditions()
    {
        // TODO: In the future, generic rules will be implemented for all controllers.
        //       These rules can be overwritten or supplemented in derived classes.
        //       For now, the base class preconditions always return success.
                
        return new Result<EControllerResult>(EControllerResult.Executed); 
    }

    protected virtual Result<EControllerResult> OnFinalized() 
    {
        return new Result<EControllerResult>(EControllerResult.Executed);
    }

    public virtual Result<EControllerResult> Run() 
    {        
        var result = CheckPreconditions();
        if (result.IsValid) 
        {
            OnStateControllerChanged(EControllerState.PreconditionsOvercome);
            result = OnInitialized();
            if(result.IsValid)
                OnStateControllerChanged(EControllerState.Initialized);
            else
            {
                OnStateControllerChanged(EControllerState.Error);
                return result;
            }
        }
        else
        {                
            OnStateControllerChanged(EControllerState.Error);
            return result;
        }

        OnStateControllerChanged(EControllerState.Started);
        return result;
    }

    public Result<EControllerResult> Finalize()
    {
        Result<EControllerResult> result;
        
        if (ControllerState == EControllerState.Finalized)
        {
            result = new Result<EControllerResult>(EControllerResult.Error);
            result.AddErrorMessage("The controller is already finalized.");
            return result;
        }

        try
        {
            result = OnFinalized();
            OnStateControllerChanged(EControllerState.Finalized);
            FinalizeViewsController();
            Store.RemoveController(this);
        }
        catch (Exception ex)
        {
            result = new Result<EControllerResult>(EControllerResult.Error);
            result.AddErrorMessage(ex.Message);
            OnStateControllerChanged(EControllerState.Error);
        }
       
        return result;
    }

    public virtual void NotifyMessage(string message)
    {
        Store.OnControllerNotification(this, message);
    }

    public virtual Result<EControllerResult> DialogResultToControllerResult(DialogResult dialogResult)
    {
        var result = new Result<EControllerResult>();
        if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes)
            result.Entity = EControllerResult.Executed;
        else
            result.Entity = EControllerResult.Canceled;
        return result;
    }

    public virtual void Dispose()
    {
        Finalize();
    }

    #region Utils protected methods

    protected void FinalizeViewsController()
    {            
        List<CtrlBase> lc = GetControllers(Fields);
        foreach (CtrlBase c in lc)
            c.Finalize();

        List<IViewBase> lv = GetViews(Fields);
        foreach (IViewBase v in lv)
            v.OnClosingView();

        // Reset fields
        foreach (FieldInfo field in Fields)
        {
            object v = field.GetValue(this);
            if ((v != null && v is CtrlBase) || ((v != null && v is IViewBase)))
            {
                field.SetValue(this, null);
            }
            else
            {
                // Reset other marked fields
                Attribute[] attributes = Attribute.GetCustomAttributes(field);

                if (attributes == null || attributes.Length.Equals(0))
                    continue;

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is ResetControllerFieldAttribute)
                    {
                        field.SetValue(this, ((ResetControllerFieldAttribute)attribute).ValueReset);
                        break;
                    }
                }
            }
        }            
    }

    protected List<FieldInfo> GetAllClassFields()
    {
        return ReflectionExtensions.GetAllFields(this.GetType(), BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    }

    protected List<CtrlBase> GetControllers(List<FieldInfo> fields)
    {
        List<CtrlBase>
            myList = new List<CtrlBase>();

        foreach (FieldInfo field in fields)
        {
            object v = field.GetValue(this);
            if (v != null && v is CtrlBase) 
            {
                myList.Add((CtrlBase)field.GetValue(this));
            }
        }

        return myList;
    }

    protected List<IViewBase> GetViews(List<FieldInfo> fields)
    {
        List<IViewBase> myList = new List<IViewBase>();

        foreach (FieldInfo field in fields)
        {
            object v = field.GetValue(this);
            if (v != null && v is IViewBase) 
                myList.Add((IViewBase)field.GetValue(this));
        }

        return myList;
    }

    #endregion

    #region ShowMessage

    public DialogResult ShowMessage(string messageText, string title)
    {
        return MessageBox.Show(messageText, title);
    }

    public DialogResult ShowMessage(string messageText, string title, MessageBoxButtons buttons)
    {
        return MessageBox.Show(messageText, title, buttons);
    }

    public DialogResult ShowMessage(string messageText, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        return MessageBox.Show(messageText, title, buttons, icon);
    }

    #endregion
}

#region  Controller typos 

public enum EControllerState
{
    NotStarted,
    PreconditionsOvercome,
    Initialized,
    Started,        
    Finalized,
    Error
}

public enum EControllerResult
{
    Null,
    Executed,
    Canceled,
    Error
}

public class ControllerEventArgs<T> : EventArgs
{
    public T Entity { get; set; }

    public ControllerEventArgs(T entity)
        : base()
    {
        this.Entity = entity;
    }
}

public delegate void ExtensionsEventHandler<T>(object sender, ControllerEventArgs<T> e);

/// <summary>
/// Attribute to identify the variables of the controller that you want to reset
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class ResetControllerFieldAttribute : Attribute
{                
    private object _valueReset;
    
    /// <summary>
    /// Value to be assigned to the variable reset
    /// </summary>
    public object ValueReset
    {
        get { return _valueReset; }
    }
    
    /// <summary>
    /// Attribute to identify the variables of the controller that you want to reset
    /// </summary>
    /// <param name="valueReset">Value to be assigned to perform a reset</param>
    public ResetControllerFieldAttribute(object valueReset)
        : base()
    {
        this._valueReset = valueReset;
    }

    /// <summary>
    /// Attribute to identify the variables of the controller that you want to reset (overload 2)
    /// </summary>
    public ResetControllerFieldAttribute(Type typeValueReset, object valueReset)
        : base()
    {
        try
        {
            if (typeValueReset == typeof(Guid))
                this._valueReset = new Guid(valueReset.ToString());
            else
                this._valueReset = Convert.ChangeType(valueReset, typeValueReset);
        }
        catch
        {
            this._valueReset = null;
        }
    }

}

#endregion
