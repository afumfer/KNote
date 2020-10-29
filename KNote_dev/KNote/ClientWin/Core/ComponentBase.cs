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

        public readonly Guid IdComponent;

        public Store Store { get; protected set; }

        public ComponentState ComponentState { get; protected set; } = ComponentState.NotStarted;
       
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

        public event EventHandler<StateComponentEventArgs> StateCtrlChanged;

        protected void OnStateCtrlChanged(ComponentState state)
        {
            ComponentState = state;
            if (StateCtrlChanged != null)
                StateCtrlChanged(this, new StateComponentEventArgs(state));
        }

        // TODO: Pendiente de valorar la implementación de eventos específicos 
        //       de inicialización de la controladora y finalización de la controladora

        #endregion

        #region Constructor

        public ComponentBase(Store store)
        {
            IdComponent = Guid.NewGuid();
            OnStateCtrlChanged(ComponentState.NotStarted);           
            Store = store;
            Store.AddComponent(this);
            AddExtensions();
        }

        #endregion


        protected virtual Result OnInitialized()
        {
            return new Result();
        } 

        protected virtual Result OnAfterRenderView()
        {
            return new Result();
        }

        protected virtual Result CheckPreconditions()
        {
            // TODO: En el futuro se implementarán reglas genéricas
            //       para todas las controladoras.
            //       Estas reglas se podrán sobreescibir o complementar en mis  
            //       clases derivadas. 
            //       Por ahora las precondiciones de la clase base 
            //       siempre se superan 
            var res = new Result();
            
            return res;
        }

        protected virtual Result OnFinalized() 
        {
            return new Result();
        }

        protected virtual Result AddExtensions()
        {
            return new Result();
        }

        public Result Run() 
        {
            Result result;
            var preconditionResult = CheckPreconditions();
            if (preconditionResult.IsValid) 
            {
                OnStateCtrlChanged(ComponentState.PreconditionsOvercome);
                result = OnInitialized();
                OnStateCtrlChanged(ComponentState.Initialized);                
            }
            else
            {
                result = preconditionResult;
                OnStateCtrlChanged(ComponentState.Error);                
            }

            OnStateCtrlChanged(ComponentState.Started);
            return result;
        }

        public Result RunModal()
        {            
            ModalMode = true;
            return Run();
        }

        public Result Finalize()
        {
            Result result;
            
            if (ComponentState == ComponentState.Finalized)
            {
                result = new Result();
                result.AddErrorMessage("The component is already finalized.");
                return result;
            }

            try
            {
                result = OnFinalized();
                Store.RemoveComponent(this);
                FinalizeViewsComponent();                                               
                OnStateCtrlChanged(ComponentState.Finalized);
            }
            catch (Exception ex)
            {
                result = new Result();
                result.AddErrorMessage(ex.Message);
                OnStateCtrlChanged(ComponentState.Error);
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

    public enum ComponentState
    {
        NotStarted,
        PreconditionsOvercome,
        Initialized,
        Started,        
        Finalized,
        Error
    }

    public class StateComponentEventArgs : EventArgs
    {
        public ComponentState State { get; set; }

        public StateComponentEventArgs(ComponentState state)
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
