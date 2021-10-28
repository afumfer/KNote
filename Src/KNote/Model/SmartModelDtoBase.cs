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
        protected bool _isNew = false;
        protected bool _isDeleted = false;        

        public virtual bool IsDirty()
        {
            bool _isChilsDirty = false;
            
            var childs = GetChilds<SmartModelDtoBase>();

            foreach (var child in childs)
            {
                if (child.IsDirty())
                {
                    _isChilsDirty = true;
                    break;
                }
            }

            if (_isDirty || _isChilsDirty)
                return true;
            else
                return false;
        }

        public virtual void SetIsDirty(bool isDirty)
        {
            _isDirty = isDirty;
            
            var childs = GetChilds<SmartModelDtoBase>();

            foreach (var child in childs)
            {
                child.SetIsDirty(isDirty);                
            }
        }
        
        public virtual bool IsNew()
        {
            return _isNew;
        }

        public virtual void SetIsNew(bool isNew)
        {
            _isNew = isNew;
        }

        public virtual bool IsDeleted()
        {
            return _isDeleted;
        }

        public virtual void SetIsDeleted(bool isDeleted)
        {
            _isDeleted = isDeleted;
        }
        
        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            _isDirty = true;
            if (PropertyChanged != null)                           
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));            
        }

        #endregion

    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ChildModelDtoIgnoreAttribute : Attribute
    {

    }

}
