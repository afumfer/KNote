using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model
{
    public abstract class DtoModelBase : ModelBase, INotifyPropertyChanged
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
    }
}
