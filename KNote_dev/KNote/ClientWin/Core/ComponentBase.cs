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
    abstract public class ComponentBase : IDisposable
    {
        #region Public properties

        public readonly Guid ComponentId;

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
                    _fields = GetAllTheClassField();
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

        public ComponentBase(Store store)
        {
            ComponentId = Guid.NewGuid();
            OnStateComponentChanged(EComponentState.NotStarted);           
            Store = store;
            Store.AddComponent(this);
            AddExtensions();
        }

        #endregion

        protected virtual Result<EComponentResult> OnInitialized()
        {
            return new Result<EComponentResult>(EComponentResult.Executed);

        } 

        protected virtual Result<EComponentResult> OnAfterRenderView()
        {
            return new Result<EComponentResult>(EComponentResult.Executed);
        }

        protected virtual Result<EComponentResult> CheckPreconditions()
        {


            // TODO: En el futuro se implementarán reglas genéricas
            //       para todas las controladoras.
            //       Estas reglas se podrán sobreescibir o complementar en mis  
            //       clases derivadas. 
            //       Por ahora las precondiciones de la clase base 
            //       siempre se superan             

            var res = new Result<EComponentResult>(EComponentResult.Executed);
            return res;
        }

        protected virtual Result<EComponentResult> OnFinalized() 
        {
            return new Result<EComponentResult>(EComponentResult.Executed);
        }

        protected virtual Result<EComponentResult> AddExtensions()
        {
            return new Result<EComponentResult>(EComponentResult.Executed);
        }

        public virtual Result<EComponentResult> Run() 
        {
            Result<EComponentResult> result;
            var preconditionResult = CheckPreconditions();
            if (preconditionResult.IsValid) 
            {
                OnStateComponentChanged(EComponentState.PreconditionsOvercome);
                result = OnInitialized();
                OnStateComponentChanged(EComponentState.Initialized);                
            }
            else
            {                
                OnStateComponentChanged(EComponentState.Error);
                result = preconditionResult;
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
                Store.RemoveComponent(this);
                FinalizeViewsComponent();                                               
                OnStateComponentChanged(EComponentState.Finalized);
            }
            catch (Exception ex)
            {
                result = new Result<EComponentResult>(EComponentResult.Error);
                result.AddErrorMessage(ex.Message);
                OnStateComponentChanged(EComponentState.Error);
            }
           
            return result;
        }

        public void Dispose()
        {
            Finalize();
        }

        #region Utils methods

        protected void FinalizeViewsComponent()
        {            
            List<ComponentBase> lc = GetControllers(Fields);
            foreach (ComponentBase c in lc)
                c.Finalize();

            List<IViewBase> lv = GetViews(Fields);
            foreach (IViewBase v in lv)
                v.OnClosingView();

            // Reset fields
            foreach (FieldInfo field in Fields)
            {
                object v = field.GetValue(this);
                if (v != null && v is ComponentBase)
                {
                    field.SetValue(this, null);
                }
                // TODO: pensar mejor esto, dejar el resto de campos del objeto intacto.
                //else if (v != null && v is ModelBase)
                //{
                //    field.SetValue(this, null);
                //}
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
                        if (attribute is ResetComponentFieldAttribute)
                        {
                            field.SetValue(this, ((ResetComponentFieldAttribute)attribute).ValueReset);
                            break;
                        }
                    }
                }
            }            
        }

        protected List<FieldInfo> GetAllTheClassField()
        {
            return ReflectionExtensions.GetAllFields(this.GetType(), BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        protected List<ComponentBase> GetControllers(List<FieldInfo> fields)
        {
            List<ComponentBase>
                myList = new List<ComponentBase>();

            foreach (FieldInfo field in fields)
            {
                object v = field.GetValue(this);
                if (v != null && v is ComponentBase) 
                {
                    myList.Add((ComponentBase)field.GetValue(this));
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
}
