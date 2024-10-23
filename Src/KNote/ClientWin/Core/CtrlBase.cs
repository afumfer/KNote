using System.Reflection;

using KNote.Model;

namespace KNote.ClientWin.Core;

abstract public class CtrlBase : IDisposable
{
    #region Public properties

    public readonly Guid ComponentId;

    public string ComponentName { get; protected set; }

    public Store Store { get; protected set; }

    public EComponentState ComponentState { get; protected set; } = EComponentState.NotStarted;
            
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
    
    public event EventHandler<ComponentEventArgs<EComponentState>> StateComponentChanged;

    protected void OnStateComponentChanged(EComponentState state)
    {
        ComponentState = state;
        StateComponentChanged?.Invoke(this, new ComponentEventArgs<EComponentState>(state));
    }
    
    #endregion

    #region Constructor

    public CtrlBase(Store store)
    {
        ComponentId = Guid.NewGuid();
        OnStateComponentChanged(EComponentState.NotStarted);           
        Store = store;
        Store.AddComponent(this);
    }

    #endregion

    protected virtual Result<EComponentResult> OnInitialized()
    {
        return new Result<EComponentResult>(EComponentResult.Executed);
    } 

    protected virtual Result<EComponentResult> CheckPreconditions()
    {
        // TODO: In the future, generic rules will be implemented for all controllers.
        //       These rules can be overwritten or supplemented in derived classes.
        //       For now, the base class preconditions always return success.
                
        return new Result<EComponentResult>(EComponentResult.Executed); 
    }

    protected virtual Result<EComponentResult> OnFinalized() 
    {
        return new Result<EComponentResult>(EComponentResult.Executed);
    }

    public virtual Result<EComponentResult> Run() 
    {        
        var result = CheckPreconditions();
        if (result.IsValid) 
        {
            OnStateComponentChanged(EComponentState.PreconditionsOvercome);
            result = OnInitialized();
            if(result.IsValid)
                OnStateComponentChanged(EComponentState.Initialized);
            else
            {
                OnStateComponentChanged(EComponentState.Error);
                return result;
            }
        }
        else
        {                
            OnStateComponentChanged(EComponentState.Error);
            return result;
        }

        OnStateComponentChanged(EComponentState.Started);
        return result;
    }

    public Result<EComponentResult> Finalize()
    {
        Result<EComponentResult> result;
        
        if (ComponentState == EComponentState.Finalized)
        {
            result = new Result<EComponentResult>(EComponentResult.Error);
            result.AddErrorMessage("The component is already finalized.");
            return result;
        }

        try
        {
            result = OnFinalized();
            OnStateComponentChanged(EComponentState.Finalized);
            FinalizeViewsComponent();
            Store.RemoveComponent(this);
        }
        catch (Exception ex)
        {
            result = new Result<EComponentResult>(EComponentResult.Error);
            result.AddErrorMessage(ex.Message);
            OnStateComponentChanged(EComponentState.Error);
        }
       
        return result;
    }

    public virtual void NotifyMessage(string message)
    {
        Store.OnComponentNotification(this, message);
    }

    public virtual Result<EComponentResult> DialogResultToComponentResult(DialogResult dialogResult)
    {
        var result = new Result<EComponentResult>();
        if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes)
            result.Entity = EComponentResult.Executed;
        else
            result.Entity = EComponentResult.Canceled;
        return result;
    }

    public virtual void Dispose()
    {
        Finalize();
    }

    #region Utils protected methods

    protected void FinalizeViewsComponent()
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
                    if (attribute is ResetComponentFieldAttribute)
                    {
                        field.SetValue(this, ((ResetComponentFieldAttribute)attribute).ValueReset);
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

    #region Utils public methods

    public DateTime? TextToDateTime(string text)
    {
        DateTime output;
        if (DateTime.TryParse(text, out output))
            return output;
        else
            return null;
    }

    public int TextToInt(string text)
    {
        int output;
        if (int.TryParse(text, out output))
            return output;
        else
            return 0;
    }

    public double? TextToDouble(string text)
    {
        double output;
        if (double.TryParse(text, out output))
            return output;
        else
            return null;
    }

    #endregion 
}

#region  Controller typos 

public enum EComponentState
{
    NotStarted,
    PreconditionsOvercome,
    Initialized,
    Started,        
    Finalized,
    Error
}

public enum EComponentResult
{
    Null,
    Executed,
    Canceled,
    Error
}

public class ComponentEventArgs<T> : EventArgs
{
    public T Entity { get; set; }

    public ComponentEventArgs(T entity)
        : base()
    {
        this.Entity = entity;
    }
}

public delegate void ExtensionsEventHandler<T>(object sender, ComponentEventArgs<T> e);

/// <summary>
/// Attribute to identify the variables of the controller that you want to reset
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class ResetComponentFieldAttribute : Attribute
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
    public ResetComponentFieldAttribute(object valueReset)
        : base()
    {
        this._valueReset = valueReset;
    }

    /// <summary>
    /// Attribute to identify the variables of the controller that you want to reset (overload 2)
    /// </summary>
    public ResetComponentFieldAttribute(Type typeValueReset, object valueReset)
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
