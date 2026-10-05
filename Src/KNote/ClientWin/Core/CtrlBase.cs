using System.Reflection;

using KNote.Model;
using KNote.ClientWin.Utils;
using KNote.Service.Core;

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

    #region Authorization

    /// <summary>
    /// The role this use case requires, declared with [KntAuthorize] on the controller class - the KNote
    /// counterpart of ASP.NET Core's [Authorize] on a controller. Null (no attribute) means no requirement:
    /// that is how a controller included in another one (a selector, a tab of the repository editor...)
    /// leaves the decision to the controller that includes it. A controller whose requirement depends on
    /// its state overrides this (see RepositoryEditorCtrl and its EditorMode). The finer-grained operations
    /// of a use case (saving, deleting...) are authorized by the Service layer, on each command.
    /// </summary>
    protected virtual KntAuthorizeAttribute RequiredAuthorization
        => GetType().GetCustomAttribute<KntAuthorizeAttribute>(inherit: false);

    /// <summary>
    /// The repository a Repository-scoped requirement is checked against: the user's role is per
    /// repository. Editors return the service they work on (see CtrlEditorBase).
    /// </summary>
    protected virtual IKntService AuthorizationResource => null;

    /// <summary>
    /// False when CheckPreconditions refused to start this controller: its view must not be shown.
    /// </summary>
    public bool PreconditionsMet { get; private set; } = true;

    /// <summary>
    /// Whether the user meets this use case's requirement, without telling the user anything (e.g. to
    /// skip a use case started automatically, such as a panel reopened at startup).
    /// </summary>
    public bool IsAuthorized()
        => Store.Security.IsAuthorized(RequiredAuthorization, AuthorizationResource);

    /// <summary>
    /// Checks this use case's requirement before it starts: if the user doesn't meet it, tells the user
    /// (Store.NotifyAccessDenied) and finalizes the controller. Run() calls it; a controller that doesn't
    /// start through Run() calls it first thing in its own entry point.
    /// </summary>
    public bool CheckAccess()
    {
        if (IsAuthorized())
            return true;

        PreconditionsMet = false;
        Store.NotifyAccessDenied(AccessDeniedMessage(RequiredAuthorization));
        Finalize();
        return false;
    }

    private string AccessDeniedMessage(KntAuthorizeAttribute requirement)
    {
        var roleName = KntConst.Roles[requirement.MinimumRole];
        var useCase = string.IsNullOrEmpty(ControllerName) ? "This option" : $"'{ControllerName}'";
        var repositoryAlias = AuthorizationResource?.RepositoryRef?.Alias;

        return requirement.Scope == AuthorizationScope.Repository && repositoryAlias != null
            ? $"{useCase} requires the role '{roleName}' in the repository '{repositoryAlias}'."
            : $"{useCase} requires the role '{roleName}'.";
    }

    #endregion

    protected virtual Result<EControllerResult> CheckPreconditions()
    {
        var result = new Result<EControllerResult>(EControllerResult.Executed);
        if (!CheckAccess())
        {
            result = new Result<EControllerResult>(EControllerResult.Error);
            result.AddErrorMessage("Access denied.");
        }
        return result;
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
            // Refused by CheckAccess: already finalized.
            if (PreconditionsMet)
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
        return dialogResult.ToControllerResult();
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

    /// <summary>
    /// Walks down an exception's InnerException chain to the innermost one. KntServiceBase.ExecuteCommand
    /// wraps every exception thrown by a command into a generic "KNote service error. (...)" exception
    /// (and a DB error may already be wrapped once more below that, e.g. "KNote repository error.
    /// (...)"), so the message actually worth showing a user is usually several layers down from what a
    /// Ctrl's catch block receives. Used by every editor Ctrl's SaveModel/DeleteModel catch block instead
    /// of showing the generic wrapper message.
    /// </summary>
    protected static string RootExceptionMessage(Exception ex)
    {
        var rootEx = ex;
        while (rootEx.InnerException != null)
            rootEx = rootEx.InnerException;
        return rootEx.Message;
    }

    #endregion

    #region ShowMessage

    public DialogResult ShowMessage(string messageText, string title)
    {
        return KntMessageBox.Show(messageText, title);
    }

    public DialogResult ShowMessage(string messageText, string title, MessageBoxButtons buttons)
    {
        return KntMessageBox.Show(messageText, title, buttons);
    }

    public DialogResult ShowMessage(string messageText, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        return KntMessageBox.Show(messageText, title, buttons, icon);
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
