using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;

using KNote.Model;

namespace KNote.ClientWin.Core
{
    abstract public class BaseCtrl : IDisposable
    {

        #region Public properties

        public readonly Guid IdCtrl;

        public KntContext Context { get; protected set; }

        public CtrlState CtrlState { get; protected set; } = CtrlState.NotStarted;

        public CtrlResult CtrlResult { get; protected set; } = CtrlResult.None;

        public bool EmbededMode { get; set; } = false;

        public bool ModalMode { get; set; } = false;

        public bool ThrowKntException { get; set; } = false;

        #endregion

        #region Protected properties

        private List<FieldInfo> _fields;   
        protected List<FieldInfo> Fields
        {
            get
            {
                if (_fields == null)
                    _fields = GetAllTheClassField();
                return _fields;
            }
        }

        #endregion region 

        #region Standard events for base controller

        public event EventHandler<StateCtrlEventArgs> StateCtrlChanged;

        protected void OnStateCtrlChanged(CtrlState state)
        {
            CtrlState = state;
            if (StateCtrlChanged != null)
                StateCtrlChanged(this, new StateCtrlEventArgs(state));
        }

        // TODO: Pendiente de valorar la implementación de eventos específicos 
        //       de inicialización de la controladora y finalización de la controladora

        #endregion

        #region Constructor

        public BaseCtrl(KntContext context)
        {
            IdCtrl = Guid.NewGuid();
            OnStateCtrlChanged(CtrlState.NotStarted);           
            Context = context;
            Context.AddController(this);
            AddExtensions();
        }

        #endregion


        protected abstract Result<CtrlResult> OnStart() ;

        protected virtual Result<CtrlResult> CheckPreconditions()
        {
            // TODO: En el futuro se implementarán reglas genéricas
            //       para todas las controladoras.
            //       Estas reglas se podrán sobreescibir o complementar en mis  
            //       clases derivadas. 
            //       Por ahora las precondiciones de la clase base 
            //       siempre se superan 
            var res = new Result<CtrlResult>();
            res.Entity = CtrlResult.None;
            return res;
        }

        protected virtual void End() {
            
        }

        protected virtual void AddExtensions()
        {
            
        }

        public Result<CtrlResult> StartCtrl() 
        {
            var preconditionResult = CheckPreconditions();
            if (preconditionResult.IsValid) 
            {
                OnStateCtrlChanged(CtrlState.PreconditionsOvercome);
                var ret = OnStart();
                OnStateCtrlChanged(CtrlState.Started);
                return ResultControllerAction<CtrlResult>(ret);
            }
            else
            {
                OnStateCtrlChanged(CtrlState.Error);
                return ResultControllerAction<CtrlResult>(preconditionResult);
            }
        }

        public Result<CtrlResult> StartCtrlModal()
        {            
            ModalMode = true;
            return StartCtrl();
        }

        public Result<CtrlResult> FinalizeCtrl()
        {
            var result = new Result<CtrlResult>();
            
            if (CtrlState == CtrlState.Finalized)
            {
                result.Entity = CtrlResult.Executed;
                return ResultControllerAction<CtrlResult>(result);
            }

            try
            {
                End();
                Context.RemoveController(this);
                FinalizeViewsControllers();                               
                result.Entity = CtrlResult.Executed;                
                OnStateCtrlChanged(CtrlState.Finalized);
                // 
                // OJO: no cambiar la propiedad CtrlResult aquí. ()
                // 
            }
            catch (Exception ex)
            {
                result.Entity = CtrlResult.Error;
                result.AddErrorMessage(ex.Message);
                OnStateCtrlChanged(CtrlState.Error);
            }
           
            return ResultControllerAction<CtrlResult>(result);
        }

        public void Dispose()
        {
            FinalizeCtrl();
        }

        #region Utils methods

        protected void FinalizeViewsControllers()
        {            
            List<BaseCtrl> lc = GetControllers(Fields);
            foreach (BaseCtrl c in lc)
                c.FinalizeCtrl();

            List<IViewBase> lv = GetViews(Fields);
            foreach (IViewBase v in lv)
                v.CloseView();

            // Reset fields
            foreach (FieldInfo field in Fields)
            {
                object v = field.GetValue(this);
                if (v != null && v is BaseCtrl)
                {
                    field.SetValue(this, null);
                }
                else if (v != null && v is ModelBase)
                {
                    field.SetValue(this, null);
                }
                else if (v != null && v is IViewBase)
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

        protected Result<T> ResultControllerAction<T>(Result<T> resultAction)
        {
            if (resultAction.IsValid == false)
                if (ThrowKntException == true)
                    throw new Exception(resultAction.Message);
            return resultAction;
        }

        protected List<FieldInfo> GetAllTheClassField()
        {
            return ReflectionExtensions.GetAllFields(this.GetType(), BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        protected List<BaseCtrl> GetControllers(List<FieldInfo> fields)
        {
            List<BaseCtrl>
                myList = new List<BaseCtrl>();

            foreach (FieldInfo field in fields)
            {
                object v = field.GetValue(this);
                if (v != null && v is BaseCtrl) 
                {
                    myList.Add((BaseCtrl)field.GetValue(this));
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

    public enum CtrlState
    {
        NotStarted,
        PreconditionsOvercome,
        Started,
        Refresh,
        Finalized,
        Error
    }

    public enum CtrlResult
    {
        None,
        Executed,
        Canceled,
        Error
    }

    public class StateCtrlEventArgs : EventArgs
    {
        public CtrlState State { get; set; }

        public StateCtrlEventArgs(CtrlState state)
            : base()
        {
            this.State = state;
        }
    }

    public class EntityEventArgs<T> : EventArgs
    {
        public T Entity { get; set; }

        public EntityEventArgs(T entity)
            : base()
        {
            this.Entity = entity;
        }
    }

    public delegate void ExtensionsEventHandler<T>(object sender, EntityEventArgs<T> e);

    public static class ReflectionExtensions
    {
        public static List<FieldInfo> GetAllFields(Type type, BindingFlags flags)
        {
            if (type == typeof(object))
                return new List<FieldInfo>();

            // Get all fields recursively           
            List<FieldInfo> myList = GetAllFields(type.BaseType, flags);
            myList.AddRange(type.GetFields(flags));
            return myList;
        }
    }

    /// <summary>
    /// Attribute to identify the variables of the controller that you want to reset
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ResetControllerFieldAttribute : Attribute
    {        
        /// <summary>
        /// Represents Guid.Empty
        /// </summary>
        public const string GUID_EMPTY = "00000000-0000-0000-0000-000000000000";
        
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
}
