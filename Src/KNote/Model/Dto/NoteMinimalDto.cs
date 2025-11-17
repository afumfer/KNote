using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto;

public class NoteMinimalDto : SmartModelDtoBase
{
    #region Property definitions

    private Guid _noteId;
    public Guid NoteId
    {
        get { return _noteId; }
        set
        {
            if (_noteId != value)
            {
                _noteId = value;
                OnPropertyChanged("NoteId");
            }
        }
    }

    private int _noteNumber;
    public int NoteNumber
    {
        get { return _noteNumber; }
        set
        {
            if (_noteNumber != value)
            {
                _noteNumber = value;
                OnPropertyChanged("NoteNumber");
            }
        }
    }

    private string _topic;
    [Required(ErrorMessage = KMSG)]
    [MaxLength(1024)]
    public string Topic
    {
        get { return _topic; }
        set
        {
            if (_topic != value)
            {
                _topic = value;
                OnPropertyChanged("Topic");
            }
        }
    }

    private int _priority;
    public int Priority
    {
        get { return _priority; }
        set
        {
            if (_priority != value)
            {
                _priority = value;
                OnPropertyChanged("Priority");
            }
        }
    }

    private string _tags;
    [MaxLength(1024)]
    public string Tags
    {
        get { return _tags; }
        set
        {
            if (_tags != value)
            {
                _tags = value;
                OnPropertyChanged("Tags");
            }
        }
    }

    private string _internalTags;
    [MaxLength(256)]
    public string InternalTags
    {
        get { return _internalTags; }
        set
        {
            if (_internalTags != value)
            {
                _internalTags = value;
                OnPropertyChanged("InternalTags");
            }
        }
    }


    private DateTime _modificationDateTime;
    [Required]
    public DateTime ModificationDateTime
    {
        get { return _modificationDateTime; }
        set
        {
            if (_modificationDateTime != value)
            {
                _modificationDateTime = value;
                OnPropertyChanged("ModificationDateTime");
            }
        }
    }

    private DateTime _creationDateTime;
    [Required]
    public DateTime CreationDateTime
    {
        get { return _creationDateTime; }
        set
        {
            if (_creationDateTime != value)
            {
                _creationDateTime = value;
                OnPropertyChanged("CreationDateTime");
            }
        }
    }

    private Guid _folderId;
    public Guid FolderId
    {
        get { return _folderId; }
        set
        {
            if (_folderId != value)
            {
                _folderId = value;
                OnPropertyChanged("FolderId");
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

        // TODO: !!! Usar yield return para devolver los resultados uno a uno y no crear la lista results

        Validator.TryValidateProperty(Topic,
            new ValidationContext(this, null, null) { MemberName = "Topic" },
            results);

        Validator.TryValidateProperty(InternalTags,
            new ValidationContext(this, null, null) { MemberName = "InternalTags" },
            results);

        Validator.TryValidateProperty(Tags,
            new ValidationContext(this, null, null) { MemberName = "Tags" },
            results);

        //----
        // Specific validations
        //----

        if (FolderId == Guid.Empty)
        {
            //results.Add(new ValidationResult
            //    ("KMSG: The FolderId attribute cannot be empty."
            //    , new[] { "FolderId" }));

            results.Add(new ValidationResult
                ("KMSG: The FolderId attribute cannot be empty."
                , new[] { nameof(FolderId) }));  // !!! Aplicar este ambio a todas las validaciones
        }

        if (ModificationDateTime < CreationDateTime)
        {
            results.Add(new ValidationResult
                ("KMSG: Modification date cannot be greater than creation date."
                , new[] { "ModificationDateTime", "CreationDateTime" }));
        }

        // TODO: Validar NoteNumber an more ....

        return results;
    }

    #endregion
}
