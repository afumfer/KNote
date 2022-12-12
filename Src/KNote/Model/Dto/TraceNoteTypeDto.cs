using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto;

public class TraceNoteTypeDto : SmartModelDtoBase
{        
    #region Property definitions

    private Guid _traceNoteTypeId;        
    public Guid TraceNoteTypeId
    {
        get { return _traceNoteTypeId; }
        set
        {
            if (_traceNoteTypeId != value)
            {
                _traceNoteTypeId = value;
                OnPropertyChanged("TraceNoteTypeId");
            }
        }
    }

    private string _name;
    [Required(ErrorMessage = KMSG)]
    [MaxLength(256)]
    public string Name
    {
        get { return _name; }
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
    }

    private string _description;
    public string Description
    {
        get { return _description; }
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged("Description");
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

        Validator.TryValidateProperty(this.Name,
           new ValidationContext(this, null, null) { MemberName = "Name" },
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