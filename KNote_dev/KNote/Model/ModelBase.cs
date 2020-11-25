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
        public abstract IEnumerable<ValidationResult> Validate(ValidationContext validationContext);
      
        public bool IsValid()
        {
            IEnumerable<ValidationResult> Validations = Validate(null);
            if (Validations != null)
                return (Validations.Count() == 0);
            else
                return true;
        }

        public string GetErrorMessage()
        {
            var validations = Validate(null);
            string msgVal = null;
            if (validations != null)
            {
                foreach (var v in validations)
                    msgVal += v.ErrorMessage + "\n";
            }

            if (!string.IsNullOrEmpty(msgVal))            
                msgVal = "Errors: \n" + msgVal;

            return msgVal;            
        }

        // TODO: add property IsDirty

    }
}
