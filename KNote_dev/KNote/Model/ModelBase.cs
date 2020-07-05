using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Reflection;

namespace KNote.Model
{
    public abstract class ModelBase : IValidatableObject    
    {
        //public ModelBase()
        //{

        //}

        public abstract IEnumerable<ValidationResult> Validate(ValidationContext validationContext);

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
       
    }
}
