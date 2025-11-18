using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto;

public class NoteInfoDto : NoteMinimalDto
{        
    #region Property definitions

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

    private string _contentType = "markdown";
    [MaxLength(1024)]
    public string ContentType
    {
        get 
        {
            if (_contentType == null)
                _contentType = "";
            return _contentType; 
        }
        set
        {
            if (_contentType != value)
            {                
                _contentType = value;
                OnPropertyChanged("ContentType");
            }
        }
    }

    private string _script;
    public string Script
    {
        get { return _script; }
        set
        {
            if (_script != value)
            {
                _script = value;
                OnPropertyChanged("Script");
            }
        }
    }

    private Guid? _noteTypeId;
    public Guid? NoteTypeId
    {
        get { return _noteTypeId; }
        set
        {
            if (_noteTypeId != value)
            {
                _noteTypeId = value;
                OnPropertyChanged("NoteTypeId");
            }
        }
    }

    #endregion 

    #region Validations

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
       return base.Validate(validationContext);
    }

    #endregion
}
