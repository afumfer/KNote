﻿using System;
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
        protected bool _isDirty = false;

        public bool IsDirty()
        {
            return _isDirty;
        }

        public virtual void SetIsDirty(bool isDirty)
        {
            _isDirty = isDirty;
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
}
