using KNote.Model.Dto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model
{
    public abstract class SmartModelDtoBase : ModelBase, INotifyPropertyChanged
    {
        protected const string KMSG = "Attribute {0} is required. ";
        
        protected bool _isDirty = false;

        public virtual bool IsDirty()
        {
            return _isDirty;
        }

        public virtual void SetIsDirty(bool isDirty)
        {
            _isDirty = isDirty;
        }

        protected bool _isNew = false;

        public virtual bool IsNew()
        {
            return _isNew;
        }

        public virtual void SetIsNew(bool isNew)
        {
            _isNew = isNew;
        }

        // TODO: !!! IsDeleted implementation ....
        protected bool _isDeleted = false;

        public virtual bool IsDeleted()
        {
            return _isDeleted;
        }

        public virtual void SetIsDeleted(bool isDeleted)
        {
            _isDeleted = isDeleted;
        }
        // ....

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            _isDirty = true;
            if (PropertyChanged != null)                           
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));            
        }

        #endregion

        public List<FieldInfo> GetAllClassFields()
        {
            return ReflectionExtensions.GetAllFields(this.GetType(), BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        }

        public List<SmartModelDtoBase> GetDtoChilds()
        {
            List<SmartModelDtoBase>
                dtoChilds = new List<SmartModelDtoBase>();

            foreach (FieldInfo field in GetAllClassFields())
            {
                
                object v = field.GetValue(this);
                if ((v != null && v is SmartModelDtoBase))
                {
                    dtoChilds.Add((SmartModelDtoBase)v);
                }

                if (v != null && ReflectionExtensions.IsEnumerableDto(v))
                {                    
                    foreach(var e in (IEnumerable<SmartModelDtoBase>)v)
                    {
                        dtoChilds.Add(e);
                    }
                }
            }

            // || (v != null && v is List<KAttributeTabulatedValueDto> ...)

            return dtoChilds;
        }


    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ChildDtoIgnoreAttribute : Attribute
    {

    }

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

        public  static Type GetElementTypeOfDtoEnumerable(object o)
        {
            var enumerable = o as IEnumerable<SmartModelDtoBase>;            
            if (enumerable == null)
                return null;

            Type[] interfaces = enumerable.GetType().GetInterfaces();

            return (from i in interfaces
                    where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    select i.GetGenericArguments()[0]).FirstOrDefault();
        }

        public static bool IsEnumerableDto(object o)
        {
            var enumerable = o as IEnumerable<SmartModelDtoBase>;            
            if (enumerable == null)
                return false;
            else 
                return true;
        }
    }
}
