using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Reflection;

namespace KNote.Model
{
    public abstract class ModelBase : KntModelBase, IValidatableObject, INotifyPropertyChanged        
    {

        #region IValidatable region

        public abstract IEnumerable<ValidationResult> Validate(ValidationContext validationContext);

        #endregion

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Constructor

        public ModelBase()
        {

        }

        #endregion

        #region Others members

        [NotMapped]
        public bool IsValid
        {
            get
            {
                IEnumerable<ValidationResult> Validations = Validate(null);
                if (Validations != null)
                    return (Validations.Count() == 0);
                else
                    return true;
            }
        }

        // TODO: add property IsDirty

        #endregion

        [Timestamp]
        public Byte[] Timestamp { get; set; }
       
    }
}
