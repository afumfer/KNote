using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto;

public class SystemValueDto : SmartModelDtoBase
{        
    #region Property definitions

    private Guid _systemValueId;        
    public Guid SystemValueId
    {
        get { return _systemValueId; }
        set
        {
            if (_systemValueId != value)
            {
                _systemValueId = value;
                OnPropertyChanged("SystemValueId");
            }
        }
    }

    private string _scope;
    [Required(ErrorMessage = KMSG)]
    [MaxLength(256)]
    public string Scope
    {
        get { return _scope; }
        set
        {
            if (_scope != value)
            {
                _scope = value;
                OnPropertyChanged("Scope");
            }
        }
    }

    private string _key;
    [Required(ErrorMessage = KMSG)]
    [MaxLength(256)]
    public string Key
    {
        get { return _key; }
        set
        {
            if (_key != value)
            {
                _key = value;
                OnPropertyChanged("Key");
            }
        }
    }

    private string _value;
    public string Value
    {
        get { return _value; }
        set
        {
            if (_value != value)
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }
    }

    #endregion

    #region Validations

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        // ---
        // Capture the validations implemented with attributes.
        // ---

        Validator.TryValidateProperty(this.Key,
           new ValidationContext(this, null, null) { MemberName = "Key" },
           results);

        //----
        // Specific validations
        //----

        // ---- Ejemplo
        //if (ModificationDateTime < CreationDateTime)
        //{
        //    results.Add(new ValidationResult
        //     ("KMSG: The modification date cannot be greater than the creation date "
        //     , new[] { "ModificationDateTime", "CreationDateTime" }));
        //}

        // ---
        // Return List<ValidationResult>()
        // ---           

        return results;
    }

    #endregion
}
