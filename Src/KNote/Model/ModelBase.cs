using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Reflection;

namespace KNote.Model;

public abstract class ModelBase : IValidatableObject    
{
    public abstract IEnumerable<ValidationResult> Validate(ValidationContext validationContext);
  
    public bool IsValid(bool incluideChildsValidations = true)
    {
        IEnumerable<ValidationResult> Validations = Validate(null);
        if (Validations != null)
            if (Validations.Count() > 0)
                return false;

        var childs = GetChilds<ModelBase>();

        if (incluideChildsValidations)
        {
            foreach(var child in childs)
            {
                if (!child.IsValid())
                    return false;
            }
        }
        
        return true;
    }

    public virtual string GetErrorMessage(bool includeChildErrors = true)
    {
        var validations = Validate(null);
        string msgVal = "";
        if (validations != null)
        {
            foreach (var v in validations)
                msgVal += v.ErrorMessage + "\n";
        }

        if (includeChildErrors)
        {
            var childs = GetChilds<ModelBase>();
            foreach (var child in childs)
            {
                var chilValidations = child.Validate(null);
                if(chilValidations != null)   
                {
                    foreach(var v in chilValidations)
                        msgVal += v.ErrorMessage + "\n";
                }                
            }
        }

        return msgVal;
    }

    protected List<T> GetChilds<T>()
    {
        List<T>
            modelChilds = new List<T>();

        var allClassFields = ReflectionExtensions.GetAllFields(this.GetType(), BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach (FieldInfo field in allClassFields)
        {
            object v = field.GetValue(this);
            if ((v != null && v is T))
            {
                modelChilds.Add((T)v);
            }
            else if (v != null && ReflectionExtensions.IsEnumerable<T>(v))
            {
                foreach (var e in (IEnumerable<T>)v)
                {
                    modelChilds.Add(e);
                }
            }
        }

        return modelChilds;
    }
}
